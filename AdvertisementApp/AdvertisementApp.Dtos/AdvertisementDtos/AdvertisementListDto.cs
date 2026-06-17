using AdvertisementApp.Common.Helpers;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Entities;

namespace AdvertisementApp.Dtos.AdvertisementDtos
{
    public class AdvertisementListDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? UserDisplayName { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImagePath { get; set; }
        public List<string> ImagePaths { get; set; } = new();
        public string? VideoPath { get; set; }
        public string? PanoramaPath { get; set; }
        public ListingType ListingType { get; set; }
        public string Content { get; set; } = null!;
        public ListingDetailsDto? ListingDetails { get; set; }
        public TramerQueryResult? TramerResult { get; set; }
        public AuctionDto? Auction { get; set; }
        public bool IsActive { get; set; }
        public AdvertisementStatus Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int ViewCount { get; set; }
        public bool IsFeatured { get; set; }
        public bool SellerIsVerified { get; set; }
        public double? SellerAverageRating { get; set; }
        public int SellerReviewCount { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string? RejectReason { get; set; }
    }
}
