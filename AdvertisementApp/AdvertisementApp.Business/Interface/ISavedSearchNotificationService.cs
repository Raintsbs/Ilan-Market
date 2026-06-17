namespace AdvertisementApp.Business.Interface
{
    public interface ISavedSearchNotificationService
    {
        Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default);
    }
}
