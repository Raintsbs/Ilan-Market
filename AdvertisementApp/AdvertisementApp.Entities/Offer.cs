namespace AdvertisementApp.Entities
{
    public enum OfferStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Withdrawn = 3
    }

    public class Offer
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public int BuyerUserId { get; set; }
        public decimal Amount { get; set; }
        public string? Message { get; set; }
        public OfferStatus Status { get; set; } = OfferStatus.Pending;
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        public Advertisement Advertisement { get; set; } = null!;
    }
}
