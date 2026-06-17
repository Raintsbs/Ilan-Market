namespace AdvertisementApp.Dtos.Platform
{
    public class MessageThreadDto
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public string AdvertisementTitle { get; set; } = null!;
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = null!;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
        public int SenderUserId { get; set; }
        public bool IsMine { get; set; }
        public string Body { get; set; } = null!;
        public DateTime CreatedTime { get; set; }
    }

    public class SendMessageDto
    {
        public int AdvertisementId { get; set; }
        public int? ThreadId { get; set; }
        public string Body { get; set; } = null!;
    }

    public class ThreadReplyDto
    {
        public string Body { get; set; } = null!;
    }

    public class OfferDto
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public string? AdvertisementTitle { get; set; }
        public int BuyerUserId { get; set; }
        public string? BuyerName { get; set; }
        public decimal Amount { get; set; }
        public string? Message { get; set; }
        public int Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public int? MessageThreadId { get; set; }
    }

    public class PublicBlogListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Summary { get; set; }
        public DateTime? PublishedTime { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class PublicBlogDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Summary { get; set; }
        public string Content { get; set; } = null!;
        public DateTime? PublishedTime { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class PublicStaticPageDto
    {
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime UpdatedTime { get; set; }
    }

    public class PublicStaticPageListDto
    {
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
    }

    public class CreateOfferDto
    {
        public int AdvertisementId { get; set; }
        public decimal Amount { get; set; }
        public string? Message { get; set; }
    }

    public class ReportListingDto
    {
        public int AdvertisementId { get; set; }
        public string Reason { get; set; } = null!;
        public string? Details { get; set; }
    }

    public class SellerPublicProfileDto
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = null!;
        public bool IsVerified { get; set; }
        public int ActiveListingCount { get; set; }
        public int TotalViews { get; set; }
        public DateTime MemberSince { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? StoreSlug { get; set; }
        public string? CompanyName { get; set; }
        public string? StoreDescription { get; set; }
        public string? StoreBannerPath { get; set; }
        public bool IsCorporateStore { get; set; }
        public int CompletedOrderCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class PushSubscriptionDto
    {
        public string Endpoint { get; set; } = null!;
        public string P256dh { get; set; } = null!;
        public string Auth { get; set; } = null!;
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string? Link { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class AnalyticsOverviewDto
    {
        public List<CategorySearchStatDto> TopCategories { get; set; } = new();
        public int TotalSearchesLast7Days { get; set; }
    }

    public class CategorySearchStatDto
    {
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int SearchCount { get; set; }
    }

    public class AdAnalyticsDto
    {
        public int AdvertisementId { get; set; }
        public int ViewCount { get; set; }
        public int OfferCount { get; set; }
        public int MessageThreadCount { get; set; }
    }

    public class AdPackageDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int FeaturedDays { get; set; }
    }

    public class CheckoutDto
    {
        public int AdPackageId { get; set; }
        public int AdvertisementId { get; set; }
        public string? CouponCode { get; set; }
    }

    public class CheckoutResultDto
    {
        public int PurchaseId { get; set; }
        public string CheckoutUrl { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsDemo { get; set; }
        public string? StripeSessionId { get; set; }
        public string? PaymentProvider { get; set; }
        public string? IyzicoToken { get; set; }
    }

    public class ExternalLoginDto
    {
        public string Provider { get; set; } = null!;
        public string IdToken { get; set; } = null!;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class FavoritePriceAlertDto
    {
        public bool PriceAlertEnabled { get; set; }
        public decimal? AlertPrice { get; set; }
    }

    public class MapListingDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public decimal? Price { get; set; }
        public string City { get; set; } = null!;
        public string? District { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string? ImagePath { get; set; }
    }
}
