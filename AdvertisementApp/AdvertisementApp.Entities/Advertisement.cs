namespace AdvertisementApp.Entities
{
    public class Advertisement : BaseEntity
    {
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImagePath { get; set; }
        /// <summary>JSON dizi: ["/uploads/a.jpg","/uploads/b.jpg"]</summary>
        public string? ImagePathsJson { get; set; }
        public string Content { get; set; } = null!;
        /// <summary>Ek ilan alanları (fiyat, konum, marka vb.) JSON.</summary>
        public string? ListingDetailsJson { get; set; }
        public AdvertisementStatus Status { get; set; } = AdvertisementStatus.Pending;
        public int ViewCount { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime? FeaturedUntil { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastBumpedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public string? RejectReason { get; set; }
        public string? AdminNote { get; set; }
        public string? VideoPath { get; set; }
        public string? PanoramaPath { get; set; }
        public ListingType ListingType { get; set; } = ListingType.Standard;
        public string? TramerResultJson { get; set; }
        public decimal? ListingPrice { get; set; }
        public int? ListingYear { get; set; }
        public int? ListingMileageKm { get; set; }

        public Category Category { get; set; } = null!;
        public Auction? Auction { get; set; }
    }
}
