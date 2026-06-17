using AdvertisementApp.Dtos.Marketplace;

namespace AdvertisementApp.Business.Interface
{
    public interface IReviewService
    {
        Task<SellerReviewDto?> CreateSellerReviewAsync(int buyerUserId, CreateSellerReviewDto dto);
        Task<SellerRatingSummaryDto> GetSellerRatingAsync(int sellerUserId, int page = 1, int pageSize = 10, int? viewerUserId = null);
        Task<AdvertisementReviewDto?> CreateAdvertisementReviewAsync(int userId, CreateAdvertisementReviewDto dto);
        Task<AdvertisementRatingSummaryDto> GetAdvertisementRatingAsync(int advertisementId, int? viewerUserId = null, int page = 1, int pageSize = 10);
        Task<AdvertisementReviewDto?> UpdateAdvertisementReviewAsync(int userId, int reviewId, UpdateReviewDto dto);
        Task<bool> DeleteAdvertisementReviewAsync(int userId, int reviewId, bool isAdmin = false);
        Task<BuyerReviewDto?> CreateBuyerReviewAsync(int sellerUserId, CreateBuyerReviewDto dto);
        Task<SellerReviewDto?> UpdateSellerReviewAsync(int userId, int reviewId, UpdateReviewDto dto);
        Task<bool> DeleteSellerReviewAsync(int userId, int reviewId, bool isAdmin = false);
        Task<List<AdminReviewItemDto>> GetAdminReviewsAsync(string? type = null, int take = 50);
        Task<bool> SetReviewHiddenAsync(string reviewType, int reviewId, bool hidden);
        Task<bool> DeleteReviewAdminAsync(string reviewType, int reviewId);
    }
}
