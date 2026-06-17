namespace AdvertisementApp.Dtos.Platform
{
    public class SellerAnalyticsDto
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = null!;
        public int ActiveListingCount { get; set; }
        public int TotalViews { get; set; }
        public int TotalOffers { get; set; }
        public int TotalMessageThreads { get; set; }
        public List<SellerAdStatDto> TopAds { get; set; } = new();
    }

    public class SellerAdStatDto
    {
        public int AdvertisementId { get; set; }
        public string Title { get; set; } = null!;
        public int ViewCount { get; set; }
        public int OfferCount { get; set; }
        public int MessageThreadCount { get; set; }
    }
}
