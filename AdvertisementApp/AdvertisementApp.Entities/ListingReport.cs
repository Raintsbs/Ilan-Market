namespace AdvertisementApp.Entities
{
    public class ListingReport
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public int ReporterUserId { get; set; }
        public string Reason { get; set; } = null!;
        public string? Details { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "open";
        public string? AdminAction { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public Advertisement Advertisement { get; set; } = null!;
    }
}
