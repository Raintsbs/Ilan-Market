namespace AdvertisementApp.Entities
{
    public class WebPushSubscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Endpoint { get; set; } = null!;
        public string P256dh { get; set; } = null!;
        public string Auth { get; set; } = null!;
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
