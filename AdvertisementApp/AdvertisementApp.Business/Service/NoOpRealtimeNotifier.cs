using AdvertisementApp.Business.Interface;

namespace AdvertisementApp.Business.Service
{
    public class NoOpRealtimeNotifier : IRealtimeNotifier
    {
        public Task PushToUserAsync(int userId, string channel, object payload) => Task.CompletedTask;
    }
}
