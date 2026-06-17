namespace AdvertisementApp.Entities
{
    public class Auction
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public decimal StartingBid { get; set; }
        public decimal MinIncrement { get; set; }
        public decimal? CurrentBid { get; set; }
        public int? HighBidderUserId { get; set; }
        public string Status { get; set; } = "active";
        public DateTime CreatedTime { get; set; }

        public Advertisement Advertisement { get; set; } = null!;
        public ICollection<AuctionBid> Bids { get; set; } = new List<AuctionBid>();
    }
}
