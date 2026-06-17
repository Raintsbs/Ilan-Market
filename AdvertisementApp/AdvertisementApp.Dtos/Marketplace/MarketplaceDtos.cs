namespace AdvertisementApp.Dtos.Marketplace
{
    public class ProvinceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? PlateCode { get; set; }
    }

    public class DistrictDto
    {
        public int Id { get; set; }
        public int ProvinceId { get; set; }
        public string Name { get; set; } = null!;
    }

    public class NeighborhoodDto
    {
        public int Id { get; set; }
        public int DistrictId { get; set; }
        public string Name { get; set; } = null!;
    }

    public class CargoCarrierDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class CreateMarketplaceOrderDto
    {
        public int AdvertisementId { get; set; }
        /// <summary>card | param_guvende</summary>
        public string PaymentMethod { get; set; } = "param_guvende";
    }

    public class PayMarketplaceOrderDto
    {
        public string? CardHolder { get; set; }
        public string? CardNumberLast4 { get; set; }
    }

    public class PayMarketplaceOrderResultDto
    {
        public MarketplaceOrderDto? Order { get; set; }
        public string? StripeCheckoutUrl { get; set; }
        public string? CheckoutUrl { get; set; }
        public string? PaymentProvider { get; set; }
        public bool IsDemo { get; set; }
        public string? Message { get; set; }
    }

    public class MarketplaceOrderDto
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public string? AdvertisementTitle { get; set; }
        public int BuyerUserId { get; set; }
        public int SellerUserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public int Status { get; set; }
        public string StatusLabel { get; set; } = null!;
        public DateTime CreatedTime { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public OrderShipmentDto? Shipment { get; set; }
        public bool CanReview { get; set; }
        public bool CanReviewAd { get; set; }
        public bool CanReviewBuyer { get; set; }
        public bool CanOpenDispute { get; set; }
        public string? DisputeReason { get; set; }
        public DateTime? DisputedAt { get; set; }
        public string? DisputeResolutionNote { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public string? RefundNote { get; set; }
        public DateTime? SellerPaidOutAt { get; set; }
        public string? SellerPayoutNote { get; set; }
    }

    public class OpenDisputeDto
    {
        public string Reason { get; set; } = null!;
    }

    public class ResolveDisputeDto
    {
        /// <summary>complete | cancel</summary>
        public string Resolution { get; set; } = "complete";
        public string? AdminNote { get; set; }
    }

    public class OrderShipmentDto
    {
        public int Id { get; set; }
        public string CarrierCode { get; set; } = null!;
        public string CarrierName { get; set; } = null!;
        public string TrackingNumber { get; set; } = null!;
        public string? TrackingUrl { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    public class ShipOrderDto
    {
        public string CarrierCode { get; set; } = null!;
        public string TrackingNumber { get; set; } = null!;
    }

    public class CreateSellerReviewDto
    {
        public int SellerUserId { get; set; }
        public int MarketplaceOrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class CreateAdvertisementReviewDto
    {
        public int AdvertisementId { get; set; }
        public int MarketplaceOrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class CreateBuyerReviewDto
    {
        public int BuyerUserId { get; set; }
        public int MarketplaceOrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateReviewDto
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class SellerReviewDto
    {
        public int Id { get; set; }
        public int SellerUserId { get; set; }
        public string? BuyerName { get; set; }
        public int? MarketplaceOrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class AdvertisementReviewDto
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public string? UserName { get; set; }
        public int? MarketplaceOrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class BuyerReviewDto
    {
        public int Id { get; set; }
        public int BuyerUserId { get; set; }
        public string? SellerName { get; set; }
        public int? MarketplaceOrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class RatingSummaryDto
    {
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<SellerReviewDto> SellerReviews { get; set; } = new();
        public List<AdvertisementReviewDto> AdvertisementReviews { get; set; } = new();
    }

    public class AdvertisementRatingSummaryDto
    {
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<AdvertisementReviewDto> Reviews { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool CanReview { get; set; }
        public int? ReviewOrderId { get; set; }
        public bool AlreadyReviewed { get; set; }
    }

    public class SellerRatingSummaryDto
    {
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<SellerReviewDto> RecentReviews { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool CanReview { get; set; }
        public int? ReviewOrderId { get; set; }
        public bool AlreadyReviewed { get; set; }
    }

    public class AdminReviewItemDto
    {
        public int Id { get; set; }
        public string ReviewType { get; set; } = null!;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsHidden { get; set; }
        public DateTime CreatedTime { get; set; }
        public string? AuthorName { get; set; }
        public string? TargetName { get; set; }
        public int? AdvertisementId { get; set; }
        public string? AdvertisementTitle { get; set; }
    }

    public class SendPhoneCodeDto
    {
        public string PhoneNumber { get; set; } = null!;
    }

    public class VerifyPhoneCodeDto
    {
        public string PhoneNumber { get; set; } = null!;
        public string Code { get; set; } = null!;
    }

    public class CategoryTreeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? Slug { get; set; }
        public string? FieldSchemaJson { get; set; }
        public List<CategoryTreeDto> Children { get; set; } = new();
    }
}
