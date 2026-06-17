using AdvertisementApp.Business.Interface;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.Business.Service
{
    public class PriceAlertNotificationService : IPriceAlertNotificationService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IPlatformService _platform;
        private readonly ILogger<PriceAlertNotificationService> _logger;

        public PriceAlertNotificationService(
            AdvertisementAppDbContext db,
            IPlatformService platform,
            ILogger<PriceAlertNotificationService> logger)
        {
            _db = db;
            _platform = platform;
            _logger = logger;
        }

        public async Task ProcessPriceAlertsAsync(CancellationToken cancellationToken = default)
        {
            var favorites = await _db.Favorites
                .Include(f => f.Advertisement)
                .Where(f => f.PriceAlertEnabled && f.Advertisement.IsActive && f.Advertisement.Status == AdvertisementStatus.Approved)
                .ToListAsync(cancellationToken);

            foreach (var fav in favorites)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var currentPrice = fav.Advertisement.ListingPrice;
                if (currentPrice == null || currentPrice <= 0) continue;

                var shouldNotify = false;
                if (fav.AlertPrice.HasValue && currentPrice <= fav.AlertPrice)
                {
                    shouldNotify = fav.LastKnownPrice == null || currentPrice < fav.LastKnownPrice;
                }
                else if (fav.LastKnownPrice.HasValue && currentPrice < fav.LastKnownPrice)
                {
                    shouldNotify = true;
                }

                if (!shouldNotify)
                {
                    if (!fav.LastKnownPrice.HasValue) fav.LastKnownPrice = currentPrice;
                    continue;
                }

                var title = fav.Advertisement.Title;
                var body = $"\"{title}\" fiyatı {currentPrice:N0} TL oldu.";
                await _platform.SendUserNotificationAsync(
                    fav.UserId,
                    "price_drop",
                    "Fiyat düştü",
                    body,
                    $"/ilan/{fav.AdvertisementId}",
                    body);

                fav.LastKnownPrice = currentPrice;
                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Fiyat alarmı: kullanıcı {UserId}, ilan {AdId}", fav.UserId, fav.AdvertisementId);
            }
        }
    }
}
