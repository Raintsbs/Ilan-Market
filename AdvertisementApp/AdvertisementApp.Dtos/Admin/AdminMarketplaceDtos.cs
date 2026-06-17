using AdvertisementApp.Dtos.Marketplace;

namespace AdvertisementApp.Dtos.Admin
{
    public class AdminMarketplaceOrderFilterDto
    {
        public int? Status { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AdminOrderNoteDto
    {
        public string? Note { get; set; }
    }

    public class AdminAdPackageDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int FeaturedDays { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
