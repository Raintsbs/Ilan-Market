namespace AdvertisementApp.Entities
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? IpAddress { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
