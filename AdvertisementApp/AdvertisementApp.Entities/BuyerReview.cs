namespace AdvertisementApp.Entities
{
    public class BuyerReview
    {
        public int Id { get; set; }
        public int BuyerUserId { get; set; }
        public int SellerUserId { get; set; }
        public int? MarketplaceOrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsHidden { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    }
}
