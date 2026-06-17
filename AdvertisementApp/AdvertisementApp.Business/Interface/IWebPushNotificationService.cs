namespace AdvertisementApp.Business.Interface
{
    public interface IWebPushNotificationService
    {
        Task SendToUserAsync(int userId, string title, string body, string? url = null);
    }
}
