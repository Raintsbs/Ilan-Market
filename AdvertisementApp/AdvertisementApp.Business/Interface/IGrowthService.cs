using AdvertisementApp.Dtos.Platform;

namespace AdvertisementApp.Business.Interface
{
    public interface IGrowthService
    {
        Task<VerificationRequestDto?> GetMyVerificationAsync(int userId);
        Task<List<ListingQuestionDto>> GetListingQuestionsAsync(int advertisementId);
        Task<ListingQuestionDto?> AskQuestionAsync(int userId, CreateListingQuestionDto dto);
        Task<ListingQuestionDto?> AnswerQuestionAsync(int userId, int questionId, AnswerListingQuestionDto dto);
        Task<bool> FollowSellerAsync(int followerUserId, int sellerUserId);
        Task<bool> UnfollowSellerAsync(int followerUserId, int sellerUserId);
        Task<bool> IsFollowingSellerAsync(int followerUserId, int sellerUserId);
        Task<List<SellerFollowDto>> GetFollowedSellersAsync(int followerUserId);
        Task<bool> UpdateStoreSettingsAsync(int userId, UpdateStoreSettingsDto dto);
        Task<UpdateStoreSettingsDto?> GetStoreSettingsAsync(int userId);
        Task<SellerEarningsDto> GetSellerEarningsAsync(int sellerUserId);
        Task<ReferralStatsDto?> GetReferralStatsAsync(int userId, string siteBaseUrl);
        Task<CouponValidationResultDto> ValidateCouponAsync(ValidateCouponDto dto);
        Task<List<CouponDto>> GetCouponsAdminAsync();
        Task<bool> SaveCouponAsync(CouponDto dto, int? id);
        Task<bool> DeleteCouponAsync(int id);
        Task<bool> RedeemCouponAsync(string code);
        Task<BulkImportResultDto> ImportSellerAdvertisementsCsvAsync(string csv, int sellerUserId);
        Task NotifyFollowersOnNewListingAsync(int sellerUserId, int advertisementId, string title);
    }
}
