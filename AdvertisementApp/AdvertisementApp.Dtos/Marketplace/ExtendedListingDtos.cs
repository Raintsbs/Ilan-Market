using AdvertisementApp.Entities;

namespace AdvertisementApp.Dtos.Marketplace
{
    public class AuctionDto
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public decimal StartingBid { get; set; }
        public decimal MinIncrement { get; set; }
        public decimal? CurrentBid { get; set; }
        public int? HighBidderUserId { get; set; }
        public string Status { get; set; } = null!;
        public int BidCount { get; set; }
        public List<AuctionBidDto> RecentBids { get; set; } = new();
    }

    public class AuctionBidDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserDisplayName { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class CreateAuctionDto
    {
        public int AdvertisementId { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public decimal StartingBid { get; set; }
        public decimal MinIncrement { get; set; }
    }

    public class PlaceBidDto
    {
        public decimal Amount { get; set; }
    }

    public class TramerQueryRequest
    {
        public string Plate { get; set; } = null!;
        public string? ChassisNumber { get; set; }
    }

    public class TramerQueryResult
    {
        public string Plate { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int DamageCount { get; set; }
        public decimal? TotalDamageAmount { get; set; }
        public string? Summary { get; set; }
        public DateTime QueriedAt { get; set; }
        public bool IsSimulated { get; set; }
    }
}
