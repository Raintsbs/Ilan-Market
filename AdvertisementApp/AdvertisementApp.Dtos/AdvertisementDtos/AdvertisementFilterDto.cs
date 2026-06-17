using AdvertisementApp.Entities;

namespace AdvertisementApp.Dtos.AdvertisementDtos
{
    public class AdvertisementFilterDto
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? UserId { get; set; }
        /// <summary>Public seller storefront (approved listings only).</summary>
        public int? SellerUserId { get; set; }
        public AdvertisementStatus? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? City { get; set; }
        public bool? FeaturedOnly { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public int? MinMileage { get; set; }
        public int? MaxMileage { get; set; }
        public int? ListingId { get; set; }
        public bool AdminMode { get; set; }
        public bool ExpiredOnly { get; set; }
        public bool ArchivedOnly { get; set; }
        public DateTime? CreatedAfter { get; set; }
    }
}
