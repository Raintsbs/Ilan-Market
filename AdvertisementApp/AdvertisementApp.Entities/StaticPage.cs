namespace AdvertisementApp.Entities
{
    public class StaticPage
    {
        public int Id { get; set; }
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedTime { get; set; } = DateTime.UtcNow;
    }
}
