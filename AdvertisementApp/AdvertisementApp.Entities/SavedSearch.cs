namespace AdvertisementApp.Entities
{
    public class SavedSearch
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string FilterJson { get; set; } = "{}";
        public bool NotifyOnNew { get; set; }
        public DateTime? LastNotifiedAt { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
