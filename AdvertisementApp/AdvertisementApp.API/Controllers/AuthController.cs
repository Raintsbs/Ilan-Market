using AdvertisementApp.API.Extensions;
using AdvertisementApp.API.Services;
using AdvertisementApp.Business.Helpers;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Constants;
using AdvertisementApp.Common.Models;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.Dtos.AuthDtos;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Dtos.Platform;
using AdvertisementApp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IImageStorageService _imageStorageService;
        private readonly IExternalAuthService _externalAuth;
        private readonly IAdminService _adminService;
        private readonly IEmailService _email;
        private readonly IEmailTemplateService _emailTemplates;
        private readonly ICaptchaService _captcha;
        private readonly IRefreshTokenService _refreshTokens;
        private readonly ISmsService _sms;
        private readonly AdvertisementAppDbContext _db;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IJwtTokenService jwtTokenService,
            IImageStorageService imageStorageService,
            IExternalAuthService externalAuth,
            IAdminService adminService,
            IEmailService email,
            IEmailTemplateService emailTemplates,
            ICaptchaService captcha,
            IRefreshTokenService refreshTokens,
            ISmsService sms,
            AdvertisementAppDbContext db,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _imageStorageService = imageStorageService;
            _externalAuth = externalAuth;
            _adminService = adminService;
            _email = email;
            _emailTemplates = emailTemplates;
            _captcha = captcha;
            _refreshTokens = refreshTokens;
            _sms = sms;
            _db = db;
            _configuration = configuration;
        }

        [HttpGet("public-config")]
        [AllowAnonymous]
        public IActionResult PublicConfig()
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var pickupDir = _configuration["Email:PickupDirectory"];
            var emailEnabled = _configuration.GetValue<bool>("Email:Enabled");
            var config = new AuthPublicConfigDto
            {
                GoogleClientId = _configuration["Google:ClientId"]?.Trim() ?? "",
                EmailEnabled = emailEnabled,
                EmailUsesPickup = emailEnabled
                    && string.IsNullOrWhiteSpace(smtpHost)
                    && !string.IsNullOrWhiteSpace(pickupDir),
                StripePublishableKey = _configuration["Stripe:PublishableKey"]?.Trim() ?? "",
                StripeEnabled = !string.IsNullOrWhiteSpace(_configuration["Stripe:SecretKey"]),
                IyzicoEnabled = !string.IsNullOrWhiteSpace(_configuration["Iyzico:ApiKey"])
                    && !string.IsNullOrWhiteSpace(_configuration["Iyzico:SecretKey"]),
                WebPushVapidPublicKey = _configuration["WebPush:VapidPublicKey"]?.Trim() ?? "",
                CaptchaSiteKey = _configuration["Captcha:SiteKey"]?.Trim() ?? "",
                CaptchaEnabled = _captcha.IsEnabled,
            };
            return Ok(ApiResponse<AuthPublicConfigDto>.Ok(config));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!await _captcha.VerifyAsync(request.CaptchaToken, HttpContext.Connection.RemoteIpAddress?.ToString()))
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("CAPTCHA doğrulaması başarısız."));

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Geçersiz e-posta veya şifre."));

            var valid = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!valid.Succeeded)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Geçersiz e-posta veya şifre."));

            var blockReason = AccountStatusHelper.GetBlockReason(user);
            if (blockReason != null)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail(blockReason));

            await _adminService.LogActivityAsync(user.Id, "login", $"Giriş: {user.Email}", HttpContext.Connection.RemoteIpAddress?.ToString());

            var response = await BuildAuthResponseAsync(user);
            return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Giriş başarılı."));
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!await _captcha.VerifyAsync(request.CaptchaToken, HttpContext.Connection.RemoteIpAddress?.ToString()))
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("CAPTCHA doğrulaması başarısız."));

            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var message = string.Join(" ", result.Errors.Select(e => e.Description));
                return BadRequest(ApiResponse<AuthResponseDto>.Fail(message));
            }

            user.ReferralCode = await GenerateUniqueReferralCodeAsync();
            if (!string.IsNullOrWhiteSpace(request.ReferralCode))
            {
                var inviter = await _db.Users.FirstOrDefaultAsync(u =>
                    u.ReferralCode == request.ReferralCode.Trim().ToUpperInvariant());
                if (inviter != null && inviter.Id != user.Id)
                    user.ReferredByUserId = inviter.Id;
            }
            await _userManager.UpdateAsync(user);

            await _userManager.AddToRoleAsync(user, AppRoles.User);
            await _emailTemplates.SendWelcomeAsync(user.Email!, user.FirstName);
            var response = await BuildAuthResponseAsync(user);
            return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Kayıt başarılı."));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var rotated = await _refreshTokens.RotateAsync(request.RefreshToken);
            if (rotated == null)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Oturum yenilenemedi. Lütfen tekrar giriş yapın."));

            var user = await _userManager.FindByIdAsync(rotated.Value.UserId.ToString());
            if (user == null)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Kullanıcı bulunamadı."));

            var blockReason = AccountStatusHelper.GetBlockReason(user);
            if (blockReason != null)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail(blockReason));

            var response = await BuildAuthResponseAsync(user, rotated.Value.NewRefreshToken);
            return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Oturum yenilendi."));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto? request)
        {
            if (!string.IsNullOrWhiteSpace(request?.RefreshToken))
                await _refreshTokens.RevokeAsync(request.RefreshToken);
            return Ok(ApiResponse.Ok("Çıkış yapıldı."));
        }

        [HttpPost("external")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto request)
        {
            var provider = request.Provider?.Trim().ToLowerInvariant();
            string? email;
            string? subject;
            string? firstName = request.FirstName;
            string? lastName = request.LastName;

            if (provider == "google")
            {
                var verified = await _externalAuth.VerifyGoogleTokenAsync(request.IdToken);
                if (verified == null)
                    return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Google doğrulaması başarısız."));
                email = verified.Value.Email;
                subject = verified.Value.Subject;
                firstName ??= verified.Value.FirstName;
                lastName ??= verified.Value.LastName;
            }
            else if (provider == "apple")
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.IdToken))
                    return BadRequest(ApiResponse<AuthResponseDto>.Fail("Apple girişi için e-posta ve token gerekli (demo)."));
                email = request.Email.Trim();
                subject = request.IdToken.Trim();
            }
            else
            {
                return BadRequest(ApiResponse<AuthResponseDto>.Fail("Desteklenmeyen sağlayıcı."));
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u =>
                u.OAuthProvider == provider && u.OAuthSubject == subject);
            if (user == null)
                user = await _userManager.FindByEmailAsync(email!);

            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName ?? "Kullanıcı",
                    LastName = lastName ?? "",
                    OAuthProvider = provider,
                    OAuthSubject = subject,
                    EmailConfirmed = true,
                };
                var create = await _userManager.CreateAsync(user);
                if (!create.Succeeded)
                    return BadRequest(ApiResponse<AuthResponseDto>.Fail(string.Join(" ", create.Errors.Select(e => e.Description))));
                await _userManager.AddToRoleAsync(user, AppRoles.User);
            }
            else
            {
                user.OAuthProvider ??= provider;
                user.OAuthSubject ??= subject;
                await _userManager.UpdateAsync(user);
            }

            var blockReason = AccountStatusHelper.GetBlockReason(user);
            if (blockReason != null)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail(blockReason));

            var response = await BuildAuthResponseAsync(user);
            return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Giriş başarılı."));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(ApiResponse<UserProfileDto>.Fail("Oturum bulunamadı."));

            var blockReason = AccountStatusHelper.GetBlockReason(user);
            if (blockReason != null)
                return StatusCode(403, ApiResponse<UserProfileDto>.Fail(blockReason));

            return Ok(ApiResponse<UserProfileDto>.Ok(await MapProfileAsync(user)));
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(ApiResponse<UserProfileDto>.Fail("Oturum bulunamadı."));

            var email = request.Email.Trim();
            if (string.IsNullOrEmpty(email))
                return BadRequest(ApiResponse<UserProfileDto>.Fail("E-posta gerekli."));

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null && existing.Id != user.Id)
                return BadRequest(ApiResponse<UserProfileDto>.Fail("Bu e-posta adresi kullanılıyor."));

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Email = email;
            user.UserName = email;
            user.NormalizedEmail = _userManager.NormalizeEmail(email);
            user.NormalizedUserName = _userManager.NormalizeName(email);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var message = string.Join(" ", result.Errors.Select(e => e.Description));
                return BadRequest(ApiResponse<UserProfileDto>.Fail(message));
            }

            return Ok(ApiResponse<UserProfileDto>.Ok(await MapProfileAsync(user), "Profil güncellendi."));
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var email = request.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var frontend = _configuration["App:FrontendUrl"] ?? "http://localhost:3000";
                var link = $"{frontend}/sifre-sifirla?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
                await _email.SendAsync(
                    email,
                    "İlanMarket — Şifre sıfırlama",
                    $"<p>Şifrenizi sıfırlamak için bağlantı:</p><p><a href=\"{link}\">{link}</a></p><p>Kod: <code>{token}</code></p>");
            }

            return Ok(ApiResponse.Ok("Hesap varsa sıfırlama e-postası gönderildi."));
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(ApiResponse.Fail("Yeni şifreler eşleşmiyor."));

            var user = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (user == null)
                return BadRequest(ApiResponse.Fail("Geçersiz istek."));

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var message = string.Join(" ", result.Errors.Select(e => e.Description));
                return BadRequest(ApiResponse.Fail(message));
            }

            return Ok(ApiResponse.Ok("Şifre güncellendi."));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(ApiResponse.Fail("Yeni şifreler eşleşmiyor."));

            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(ApiResponse.Fail("Oturum bulunamadı."));

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var message = string.Join(" ", result.Errors.Select(e => e.Description));
                return BadRequest(ApiResponse.Fail(message));
            }

            return Ok(ApiResponse.Ok("Şifre güncellendi."));
        }

        [HttpPost("profile-photo")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile? image)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(ApiResponse<UserProfileDto>.Fail("Oturum bulunamadı."));

            if (image == null || image.Length == 0)
                return BadRequest(ApiResponse<UserProfileDto>.Fail("Görsel seçin."));

            var saved = await _imageStorageService.SaveImageAsync(image);
            if (!saved.Success || string.IsNullOrEmpty(saved.Data))
                return BadRequest(ApiResponse<UserProfileDto>.Fail(saved.Message));

            user.ProfileImagePath = saved.Data;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var message = string.Join(" ", result.Errors.Select(e => e.Description));
                return BadRequest(ApiResponse<UserProfileDto>.Fail(message));
            }

            return Ok(ApiResponse<UserProfileDto>.Ok(await MapProfileAsync(user), "Profil fotoğrafı güncellendi."));
        }

        [HttpPost("phone/send-code")]
        [Authorize]
        public async Task<IActionResult> SendPhoneCode([FromBody] SendPhoneCodeDto dto)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(ApiResponse.Fail("Oturum bulunamadı."));

            var phone = NormalizePhone(dto.PhoneNumber);
            if (phone.Length < 10) return BadRequest(ApiResponse.Fail("Geçerli telefon girin."));

            var code = Random.Shared.Next(100000, 999999).ToString();
            _db.PhoneVerificationCodes.Add(new PhoneVerificationCode
            {
                UserId = user.Id,
                PhoneNumber = phone,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                CreatedTime = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
            await _sms.SendVerificationCodeAsync(phone, code);
            return Ok(ApiResponse.Ok("Doğrulama kodu gönderildi."));
        }

        [HttpPost("phone/verify")]
        [Authorize]
        public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneCodeDto dto)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(ApiResponse<UserProfileDto>.Fail("Oturum bulunamadı."));

            var phone = NormalizePhone(dto.PhoneNumber);
            var row = await _db.PhoneVerificationCodes
                .Where(c => c.UserId == user.Id && !c.Used && c.PhoneNumber == phone && c.Code == dto.Code.Trim())
                .OrderByDescending(c => c.CreatedTime)
                .FirstOrDefaultAsync();
            if (row == null || row.ExpiresAt < DateTime.UtcNow)
                return BadRequest(ApiResponse<UserProfileDto>.Fail("Kod geçersiz veya süresi dolmuş."));

            row.Used = true;
            user.PhoneNumber = phone;
            user.PhoneNumberConfirmed = true;
            user.PhoneVerified = true;
            await _userManager.UpdateAsync(user);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<UserProfileDto>.Ok(await MapProfileAsync(user), "Telefon doğrulandı."));
        }

        [HttpDelete("profile-photo")]
        [Authorize]
        public async Task<IActionResult> RemoveProfilePhoto()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(ApiResponse<UserProfileDto>.Fail("Oturum bulunamadı."));

            user.ProfileImagePath = null;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var message = string.Join(" ", result.Errors.Select(e => e.Description));
                return BadRequest(ApiResponse<UserProfileDto>.Fail(message));
            }

            return Ok(ApiResponse<UserProfileDto>.Ok(await MapProfileAsync(user), "Profil fotoğrafı kaldırıldı."));
        }

        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return null;
            return await _userManager.FindByIdAsync(userId);
        }

        private async Task<UserProfileDto> MapProfileAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserProfileDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImagePath = user.ProfileImagePath,
                Roles = roles.ToList(),
                IsVerified = user.IsVerified,
                PhoneNumber = user.PhoneNumber,
                PhoneVerified = user.PhoneVerified,
            };
        }

        private static string NormalizePhone(string raw) =>
            new string(raw.Where(char.IsDigit).ToArray());

        private async Task<string> GenerateUniqueReferralCodeAsync()
        {
            for (var attempt = 0; attempt < 20; attempt++)
            {
                var code = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("+", "").Replace("/", "").Replace("=", "")[..8]
                    .ToUpperInvariant();
                if (!await _db.Users.AnyAsync(u => u.ReferralCode == code))
                    return code;
            }
            return Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        }

        private async Task<AuthResponseDto> BuildAuthResponseAsync(AppUser user, string? existingRefreshToken = null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var expiresHours = int.TryParse(_configuration["Jwt:ExpiresHours"], out var h) ? h : 8;
            var refreshToken = existingRefreshToken ?? await _refreshTokens.IssueAsync(user.Id);
            return new AuthResponseDto
            {
                Token = await _jwtTokenService.GenerateTokenAsync(user),
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(expiresHours),
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            };
        }
    }
}
