using System.Security.Claims;
using System.Text.Json;
using AdvertisementApp.API.Extensions;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Constants;
using AdvertisementApp.Common.Models;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.API.Controllers
{
    /// <summary>
    /// Emlak Portföy → İlanMarket yayınlama (API key).
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [EnableRateLimiting("write")]
    public class IntegrationsController : ControllerBase
    {
        public const string PartnerEmail = "emlak-partner@ilanmarket.local";
        private const string DefaultApiKey = "emlak-dev-key";

        private readonly IConfiguration _config;
        private readonly IAdvertisementService _ads;
        private readonly AdvertisementAppDbContext _db;
        private readonly UserManager<AppUser> _users;

        public IntegrationsController(
            IConfiguration config,
            IAdvertisementService ads,
            AdvertisementAppDbContext db,
            UserManager<AppUser> users)
        {
            _config = config;
            _ads = ads;
            _db = db;
            _users = users;
        }

        [HttpPost("api/integrations/emlak/listings")]
        [HttpPost("api/integrations/emlak/publish")]
        [HttpPost("api/integrations/emlak-portfolio/listings")]
        [HttpPost("api/integrations/emlak-portfolio/publish")]
        [HttpPost("api/partner/emlak/listings")]
        public async Task<IActionResult> PublishListing([FromBody] EmlakPublishRequest? body)
        {
            if (!IsAuthorizedPartner())
                return Unauthorized(ApiResponse.Fail("Geçersiz veya eksik API anahtarı."));

            if (body == null || string.IsNullOrWhiteSpace(body.Title))
                return BadRequest(ApiResponse.Fail("title zorunlu."));

            var user = await EnsurePartnerUserAsync();
            if (user == null)
                return StatusCode(500, ApiResponse.Fail("Partner kullanıcı oluşturulamadı."));

            var categoryId = await ResolveEstateCategoryIdAsync();
            if (categoryId == 0)
                return BadRequest(ApiResponse.Fail("Emlak kategorisi bulunamadı. Seed çalıştırın."));

            var listingType = ResolveListingType(body);
            var details = BuildListingDetailsJson(body);
            var imagePaths = body.Images?.Where(u => !string.IsNullOrWhiteSpace(u)).Take(12).ToList()
                ?? new List<string>();

            var dto = new AdvertisementCreateDto
            {
                UserId = user.Id,
                CategoryId = categoryId,
                Title = body.Title.Trim(),
                Description = Truncate(body.Description?.Trim() ?? body.Title.Trim(), 500),
                Content = body.Description?.Trim() ?? body.Title.Trim(),
                ListingType = listingType,
                ListingDetailsJson = details,
                ImagePath = imagePaths.FirstOrDefault(),
                ImagePathsJson = imagePaths.Count > 0 ? JsonSerializer.Serialize(imagePaths) : null,
                CaptchaToken = null,
            };

            var created = await _ads.CreateReturningAsync(dto);
            if (!created.Success || created.Data == null)
                return BadRequest(ApiResponse.Fail(created.Message ?? "İlan oluşturulamadı."));

            // Partner yayınları hemen görünsün
            var entity = await _db.Advertisements.FirstOrDefaultAsync(a => a.Id == created.Data.Id);
            if (entity != null)
            {
                entity.Status = AdvertisementStatus.Approved;
                entity.IsActive = true;
                entity.AdminNote = $"emlak-portfolio:{body.ExternalId ?? "-"}";
                await _db.SaveChangesAsync();
            }

            var frontend = (_config["App:FrontendUrl"] ?? "https://ilan-market.vercel.app").TrimEnd('/');
            return Ok(ApiResponse<object>.Ok(new
            {
                id = created.Data.Id,
                title = created.Data.Title,
                url = $"{frontend}/ilan/{created.Data.Id}",
                externalId = body.ExternalId,
            }, "İlan İlanMarket'e yayınlandı."));
        }

        [HttpGet("api/integrations/emlak/health")]
        [HttpGet("api/integrations/emlak-portfolio/health")]
        public IActionResult Health()
        {
            if (!IsAuthorizedPartner())
                return Unauthorized(ApiResponse.Fail("Geçersiz veya eksik API anahtarı."));
            return Ok(ApiResponse.Ok("Emlak partner entegrasyonu hazır."));
        }

        private bool IsAuthorizedPartner()
        {
            var expected = _config["Integrations:Emlak:ApiKey"]?.Trim();
            if (string.IsNullOrWhiteSpace(expected))
                expected = DefaultApiKey;

            var provided =
                Request.Headers["X-Api-Key"].FirstOrDefault()
                ?? Request.Headers["X-ILAN-MARKET-KEY"].FirstOrDefault()
                ?? Request.Query["apiKey"].FirstOrDefault();

            return !string.IsNullOrWhiteSpace(provided)
                && string.Equals(provided.Trim(), expected, StringComparison.Ordinal);
        }

        private async Task<AppUser?> EnsurePartnerUserAsync()
        {
            var user = await _users.FindByEmailAsync(PartnerEmail);
            if (user != null) return user;

            user = new AppUser
            {
                UserName = PartnerEmail,
                Email = PartnerEmail,
                EmailConfirmed = true,
                FirstName = "Emlak",
                LastName = "Portföy",
            };
            var result = await _users.CreateAsync(user, "EmlakPartner123!");
            if (!result.Succeeded) return null;
            if (!await _users.IsInRoleAsync(user, AppRoles.User))
                await _users.AddToRoleAsync(user, AppRoles.User);
            return user;
        }

        private async Task<int> ResolveEstateCategoryIdAsync()
        {
            var names = new[] { "Konut", "Daire", "Emlak", "Satılık Daire", "Kiralık Daire" };
            foreach (var name in names)
            {
                var id = await _db.Categories.AsNoTracking()
                    .Where(c => c.Name == name && c.IsActive)
                    .OrderByDescending(c => c.ParentId != null)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync();
                if (id != 0) return id;
            }

            return await _db.Categories.AsNoTracking()
                .Where(c => c.IsActive && (c.Name.Contains("Emlak") || c.Name.Contains("Konut")))
                .Select(c => c.Id)
                .FirstOrDefaultAsync();
        }

        private static ListingType ResolveListingType(EmlakPublishRequest body)
        {
            var raw = $"{body.ListingType} {body.Status} {body.Type}".ToLowerInvariant();
            if (raw.Contains("rent") || raw.Contains("kiral"))
                return ListingType.Standard;
            return ListingType.Standard;
        }

        private static string BuildListingDetailsJson(EmlakPublishRequest body)
        {
            var payload = new Dictionary<string, object?>
            {
                ["price"] = body.Price,
                ["city"] = body.City,
                ["district"] = body.District,
                ["address"] = body.Address,
                ["rooms"] = body.Rooms,
                ["area"] = body.Area,
                ["floor"] = body.Floor,
                ["buildingAge"] = body.BuildingAge,
                ["heating"] = body.Heating,
                ["sellerType"] = "Mağazadan",
                ["condition"] = "İkinci El",
                ["source"] = "emlak-portfolio",
                ["externalId"] = body.ExternalId,
            };
            return JsonSerializer.Serialize(payload);
        }

        private static string Truncate(string value, int max)
            => value.Length <= max ? value : value[..max];
    }

    public sealed class EmlakPublishRequest
    {
        public string? ExternalId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Address { get; set; }
        public string? Rooms { get; set; }
        public double? Area { get; set; }
        public int? Floor { get; set; }
        public int? BuildingAge { get; set; }
        public string? Heating { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? ListingType { get; set; }
        public List<string>? Images { get; set; }
    }
}
