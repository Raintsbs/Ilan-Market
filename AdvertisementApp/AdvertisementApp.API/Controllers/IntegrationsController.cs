using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using AdvertisementApp.API.Extensions;
using AdvertisementApp.Business.Helpers;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Constants;
using AdvertisementApp.Common.Helpers;
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
            var imagePaths = (body.Images ?? new List<string>())
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Select(u => u.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .ToList();

            var description = BuildDescription(body);

            var dto = new AdvertisementCreateDto
            {
                UserId = user.Id,
                CategoryId = categoryId,
                Title = body.Title.Trim(),
                Description = Truncate(description, 500),
                Content = description,
                ListingType = listingType,
                ListingDetailsJson = details,
                ImagePath = imagePaths.FirstOrDefault(),
                ImagePathsJson = imagePaths.Count > 0 ? JsonSerializer.Serialize(imagePaths) : null,
                CaptchaToken = null,
            };

            var created = await _ads.CreateReturningAsync(dto);
            if (!created.Success || created.Data == null)
                return BadRequest(ApiResponse.Fail(created.Message ?? "İlan oluşturulamadı."));

            // Partner yayınları hemen görünsün; indeks alanlarını da güncelle
            var entity = await _db.Advertisements.FirstOrDefaultAsync(a => a.Id == created.Data.Id);
            if (entity != null)
            {
                entity.Status = AdvertisementStatus.Approved;
                entity.IsActive = true;
                entity.ListingDetailsJson = details;
                entity.AdminNote = $"emlak-portfolio:{body.ExternalId ?? "-"}";
                ListingIndexSync.Apply(entity);
                await _db.SaveChangesAsync();
            }

            var frontend = (_config["App:FrontendUrl"] ?? "https://ilan-market.vercel.app").TrimEnd('/');
            return Ok(ApiResponse<object>.Ok(new
            {
                id = created.Data.Id,
                title = body.Title.Trim(),
                url = $"{frontend}/ilan/{created.Data.Id}",
                externalId = body.ExternalId,
                price = body.Price,
                city = body.City,
                district = body.District,
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
            var details = new ListingDetailsDto
            {
                Price = body.Price,
                City = NullIfEmpty(body.City),
                District = NullIfEmpty(body.District),
                RoomCount = NullIfEmpty(body.Rooms),
                SquareMeters = body.Area is > 0
                    ? body.Area.Value.ToString("0.##", CultureInfo.InvariantCulture)
                    : null,
                Floor = body.Floor?.ToString(CultureInfo.InvariantCulture),
                BuildingAge = body.BuildingAge?.ToString(CultureInfo.InvariantCulture),
                Heating = NullIfEmpty(body.Heating),
                SellerType = "Mağazadan",
                Condition = "İkinci El",
                DeedStatus = "Belirtilmemiş",
            };

            // Adres varsa district satırına ek bilgi olarak yazılmasın; açıklamada kalsın.
            // ListingDetailsDto'da address yok — getLocationLine city+district kullanır.
            return ListingDetailsHelper.Serialize(details)
                ?? JsonSerializer.Serialize(new { price = body.Price, city = body.City, district = body.District });
        }

        private static string BuildDescription(EmlakPublishRequest body)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(body.Description))
                parts.Add(body.Description.Trim());

            var meta = new List<string>();
            if (!string.IsNullOrWhiteSpace(body.City) || !string.IsNullOrWhiteSpace(body.District))
                meta.Add($"{body.District} {body.City}".Trim());
            if (!string.IsNullOrWhiteSpace(body.Address))
                meta.Add(body.Address.Trim());
            if (!string.IsNullOrWhiteSpace(body.Rooms))
                meta.Add($"{body.Rooms} oda");
            if (body.Area is > 0)
                meta.Add($"{body.Area.Value.ToString("0.##", CultureInfo.InvariantCulture)} m²");
            if (body.Floor != null)
                meta.Add($"{body.Floor}. kat");
            if (body.BuildingAge != null)
                meta.Add($"Bina yaşı: {body.BuildingAge}");
            if (!string.IsNullOrWhiteSpace(body.Heating))
                meta.Add($"Isıtma: {body.Heating}");
            if (body.Price != null)
                meta.Add($"Fiyat: {body.Price.Value.ToString("N0", new CultureInfo("tr-TR"))} TL");
            if (!string.IsNullOrWhiteSpace(body.Type))
                meta.Add($"Tip: {body.Type}");
            if (!string.IsNullOrWhiteSpace(body.Status))
                meta.Add($"Durum: {body.Status}");

            if (meta.Count > 0)
                parts.Add(string.Join(" · ", meta));

            var text = string.Join("\n\n", parts);
            return string.IsNullOrWhiteSpace(text) ? body.Title.Trim() : text;
        }

        private static string? NullIfEmpty(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
