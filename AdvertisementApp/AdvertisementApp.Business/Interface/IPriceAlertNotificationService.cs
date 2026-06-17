using AdvertisementApp.Business.Interface;

namespace AdvertisementApp.Business.Interface
{
    public interface IPriceAlertNotificationService
    {
        Task ProcessPriceAlertsAsync(CancellationToken cancellationToken = default);
    }
}
