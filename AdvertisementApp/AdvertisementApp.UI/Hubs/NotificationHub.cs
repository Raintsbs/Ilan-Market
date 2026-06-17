using Microsoft.AspNetCore.SignalR;

namespace AdvertisementApp.UI.Hubs
{
    public class NotificationHub : Hub
    {
        // Admin değişiklik yaptığında tüm bağlı istemcilere bildirim gönderir
        public async Task NotifyChange(string type)
        {
            await Clients.All.SendAsync("OnChange", type);
        }
    }
}
