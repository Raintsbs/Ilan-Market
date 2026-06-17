namespace AdvertisementApp.Business.Interface
{
    public interface IRealtimeNotifier
    {
        Task PushToUserAsync(int userId, string channel, object payload);
    }
}
