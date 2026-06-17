namespace AdvertisementApp.Entities
{
    public class AdvertisementReview
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public int UserId { get; set; }
        public int? MarketplaceOrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsHidden { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedTime { get; set; }
    }
}
