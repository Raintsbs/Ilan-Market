using AdvertisementApp.Business.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.API.Services
{
    public class PriceAlertNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private readonly ILogger<PriceAlertNotificationBackgroundService> _logger;

        public PriceAlertNotificationBackgroundService(
            IServiceProvider services,
            IConfiguration config,
            ILogger<PriceAlertNotificationBackgroundService> logger)
        {
            _services = services;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalMinutes = _config.GetValue("PriceAlertNotification:IntervalMinutes", 30);
            if (intervalMinutes < 1) intervalMinutes = 30;

            _logger.LogInformation("Fiyat alarmı job'ı başlatıldı (her {Minutes} dk).", intervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var notifier = scope.ServiceProvider.GetRequiredService<IPriceAlertNotificationService>();
                    await notifier.ProcessPriceAlertsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fiyat alarmı job hatası.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            }
        }
    }
}
