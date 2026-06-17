namespace AdvertisementApp.Dtos.Platform
{
    public class VerificationRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserEmail { get; set; }
        public string DocumentType { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ReviewVerificationDto
    {
        public bool Approve { get; set; }
        public string? AdminNote { get; set; }
    }

    public class BulkImportResultDto
    {
        public int Created { get; set; }
        public int Failed { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class PackageExperimentDto
    {
        public string Variant { get; set; } = "A";
        public List<AdPackageDto> Packages { get; set; } = new();
    }

    public class LogPackageExperimentDto
    {
        public string Variant { get; set; } = null!;
        public string Event { get; set; } = null!;
        public int? PackageId { get; set; }
    }

    public class ListingQuestionDto
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string Question { get; set; } = null!;
        public string? Answer { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? AnsweredTime { get; set; }
        public bool IsOwnerAnswer { get; set; }
    }

    public class CreateListingQuestionDto
    {
        public int AdvertisementId { get; set; }
        public string Question { get; set; } = null!;
    }

    public class AnswerListingQuestionDto
    {
        public string Answer { get; set; } = null!;
    }

    public class SellerFollowDto
    {
        public int SellerUserId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string? StoreSlug { get; set; }
        public int ActiveListingCount { get; set; }
        public DateTime FollowedAt { get; set; }
    }

    public class UpdateStoreSettingsDto
    {
        public string? StoreSlug { get; set; }
        public string? CompanyName { get; set; }
        public string? StoreDescription { get; set; }
        public bool? IsCorporateStore { get; set; }
    }

    public class SellerEarningsDto
    {
        public decimal TotalCompletedAmount { get; set; }
        public decimal PendingPayoutAmount { get; set; }
        public decimal PaidOutAmount { get; set; }
        public int CompletedOrderCount { get; set; }
    }

    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public int MaxUses { get; set; }
        public int UsedCount { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ValidateCouponDto
    {
        public string Code { get; set; } = null!;
        public decimal OrderAmount { get; set; }
    }

    public class CouponValidationResultDto
    {
        public bool Valid { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? Message { get; set; }
    }

    public class ReferralStatsDto
    {
        public string ReferralCode { get; set; } = null!;
        public int ReferredUserCount { get; set; }
        public string ShareUrl { get; set; } = null!;
    }
}
