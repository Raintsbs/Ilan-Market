using AdvertisementApp.Business.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.API.Services
{
    public class SavedSearchNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private readonly ILogger<SavedSearchNotificationBackgroundService> _logger;

        public SavedSearchNotificationBackgroundService(
            IServiceProvider services,
            IConfiguration config,
            ILogger<SavedSearchNotificationBackgroundService> logger)
        {
            _services = services;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalMinutes = _config.GetValue("SavedSearchNotification:IntervalMinutes", 15);
            if (intervalMinutes < 1) intervalMinutes = 15;

            _logger.LogInformation(
                "Kayıtlı arama bildirim job'ı başlatıldı (her {Minutes} dk).",
                intervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var notifier = scope.ServiceProvider.GetRequiredService<ISavedSearchNotificationService>();
                    await notifier.ProcessPendingNotificationsAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kayıtlı arama bildirim job hatası.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
