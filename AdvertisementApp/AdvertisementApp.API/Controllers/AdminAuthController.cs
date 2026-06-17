using AdvertisementApp.API.Services;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Constants;
using AdvertisementApp.Common.Models;
using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.Dtos.AuthDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAdminService _adminService;

        public AdminAuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IJwtTokenService jwtTokenService,
            IAdminService adminService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _adminService = adminService;
        }

        /// <summary>
        /// Yalnızca Admin veya Moderatör rolleri için panel girişi (genel /api/auth/login'den ayrı).
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Geçersiz e-posta veya şifre."));

            var valid = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!valid.Succeeded)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Geçersiz e-posta veya şifre."));

            if (user.IsBanned)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Hesabınız engellenmiş."));
            if (user.IsFrozen && (user.FrozenUntil == null || user.FrozenUntil > DateTime.UtcNow))
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Hesabınız dondurulmuş."));

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any(r => r == AppRoles.Admin || r == AppRoles.Moderator))
                return StatusCode(403, ApiResponse<AuthResponseDto>.Fail("Bu hesabın yönetim paneline erişimi yok."));

            await _adminService.LogActivityAsync(
                user.Id,
                "admin_login",
                $"Panel girişi: {user.Email}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            var response = new AuthResponseDto
            {
                Token = await _jwtTokenService.GenerateTokenAsync(user),
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
            };

            return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Panel girişi başarılı."));
        }
    }
}
