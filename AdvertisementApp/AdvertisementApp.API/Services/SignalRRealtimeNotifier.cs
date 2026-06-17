using AdvertisementApp.API.Hubs;
using AdvertisementApp.Business.Interface;
using Microsoft.AspNetCore.SignalR;

namespace AdvertisementApp.API.Services
{
    public class SignalRRealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<AppHub> _hub;

        public SignalRRealtimeNotifier(IHubContext<AppHub> hub) => _hub = hub;

        public Task PushToUserAsync(int userId, string channel, object payload) =>
            _hub.Clients.Group(AppHub.UserGroup(userId)).SendAsync("event", channel, payload);
    }
}
