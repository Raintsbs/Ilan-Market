namespace AdvertisementApp.Entities
{
    public class AppNotification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string? Link { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
