using AdvertisementApp.Business.Interface;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebPush;

namespace AdvertisementApp.Business.Service
{
    public class WebPushNotificationService : IWebPushNotificationService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<WebPushNotificationService> _logger;

        public WebPushNotificationService(
            AdvertisementAppDbContext db,
            IConfiguration config,
            ILogger<WebPushNotificationService> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        public async Task SendToUserAsync(int userId, string title, string body, string? url = null)
        {
            var publicKey = _config["WebPush:VapidPublicKey"]?.Trim();
            var privateKey = _config["WebPush:VapidPrivateKey"]?.Trim();
            var subject = _config["WebPush:Subject"]?.Trim() ?? "mailto:admin@ilanmarket.local";
            if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey)) return;

            var subs = await _db.WebPushSubscriptions.AsNoTracking()
                .Where(s => s.UserId == userId)
                .ToListAsync();
            if (subs.Count == 0) return;

            var client = new WebPushClient();
            var vapid = new VapidDetails(subject, publicKey, privateKey);
            var payload = System.Text.Json.JsonSerializer.Serialize(new { title, body, url });

            foreach (var sub in subs)
            {
                try
                {
                    var pushSub = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                    await client.SendNotificationAsync(pushSub, payload, vapid);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Web push failed for user {UserId}", userId);
                }
            }
        }
    }
}
