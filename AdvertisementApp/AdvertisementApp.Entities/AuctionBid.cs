namespace AdvertisementApp.Entities
{
    public class AuctionBid
    {
        public int Id { get; set; }
        public int AuctionId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedTime { get; set; }

        public Auction Auction { get; set; } = null!;
    }
}
