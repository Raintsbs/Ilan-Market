using System.Text.Json;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.Business.Service
{
    public class SavedSearchNotificationService : ISavedSearchNotificationService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IAdvertisementService _ads;
        private readonly IPlatformService _platform;
        private readonly ILogger<SavedSearchNotificationService> _logger;

        public SavedSearchNotificationService(
            AdvertisementAppDbContext db,
            IAdvertisementService ads,
            IPlatformService platform,
            ILogger<SavedSearchNotificationService> logger)
        {
            _db = db;
            _ads = ads;
            _platform = platform;
            _logger = logger;
        }

        public async Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default)
        {
            var searches = await _db.SavedSearches
                .Where(s => s.NotifyOnNew)
                .ToListAsync(cancellationToken);

            if (searches.Count == 0) return;

            var now = DateTime.UtcNow;
            foreach (var search in searches)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await ProcessSearchAsync(search, now, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Kayıtlı arama bildirimi başarısız: {Id}", search.Id);
                }
            }
        }

        private async Task ProcessSearchAsync(SavedSearch search, DateTime now, CancellationToken cancellationToken)
        {
            var since = search.LastNotifiedAt ?? search.CreatedTime;
            var filter = ParseFilter(search.FilterJson);
            filter.CreatedAfter = since;
            filter.Page = 1;
            filter.PageSize = 20;
            filter.Status = AdvertisementStatus.Approved;

            var result = await _ads.GetPagedAsync(filter);
            if (!result.Success || result.Data?.Items == null || result.Data.Items.Count == 0)
            {
                search.LastNotifiedAt = now;
                await _db.SaveChangesAsync(cancellationToken);
                return;
            }

            foreach (var ad in result.Data.Items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var link = $"/ilan/{ad.Id}";
                var body = $"\"{ad.Title}\" aramanıza uyuyor.";
                await _platform.SendUserNotificationAsync(
                    search.UserId,
                    "saved_search",
                    "Yeni ilan — kayıtlı arama",
                    body,
                    link,
                    $"Kayıtlı aramanız: {search.Name}");
            }

            search.LastNotifiedAt = now;
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Kayıtlı arama {Id}: {Count} bildirim gönderildi.",
                search.Id,
                result.Data.Items.Count);
        }

        private static AdvertisementFilterDto ParseFilter(string json)
        {
            var filter = new AdvertisementFilterDto();
            if (string.IsNullOrWhiteSpace(json)) return filter;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (TryString(root, "search", out var search) && !string.IsNullOrWhiteSpace(search))
                    filter.Search = search;

                if (TryString(root, "categoryId", out var catStr) && int.TryParse(catStr, out var catId))
                    filter.CategoryId = catId;

                if (TryString(root, "city", out var city) && !string.IsNullOrWhiteSpace(city))
                    filter.City = city;

                if (TryString(root, "minPrice", out var minP) && decimal.TryParse(minP, out var minPrice))
                    filter.MinPrice = minPrice;

                if (TryString(root, "maxPrice", out var maxP) && decimal.TryParse(maxP, out var maxPrice))
                    filter.MaxPrice = maxPrice;

                if (TryString(root, "brand", out var brand) && !string.IsNullOrWhiteSpace(brand))
                    filter.Brand = brand;

                if (TryString(root, "model", out var model) && !string.IsNullOrWhiteSpace(model))
                    filter.Model = model;

                if (TryString(root, "minYear", out var minY) && int.TryParse(minY, out var minYear))
                    filter.MinYear = minYear;

                if (TryString(root, "maxYear", out var maxY) && int.TryParse(maxY, out var maxYear))
                    filter.MaxYear = maxYear;

                if (TryString(root, "minMileage", out var minM) && int.TryParse(minM, out var minMileage))
                    filter.MinMileage = minMileage;

                if (TryString(root, "maxMileage", out var maxM) && int.TryParse(maxM, out var maxMileage))
                    filter.MaxMileage = maxMileage;

                if (root.TryGetProperty("featuredOnly", out var featured) &&
                    featured.ValueKind is JsonValueKind.True or JsonValueKind.False)
                    filter.FeaturedOnly = featured.GetBoolean();
            }
            catch
            {
                /* malformed filter */
            }

            return filter;
        }

        private static bool TryString(JsonElement root, string name, out string? value)
        {
            value = null;
            if (!root.TryGetProperty(name, out var el)) return false;
            value = el.ValueKind == JsonValueKind.String ? el.GetString() : el.ToString();
            return true;
        }
    }
}
