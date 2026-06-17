namespace AdvertisementApp.Entities
{
    public class SearchLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? CategoryId { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
