using AdvertisementApp.Dtos.Platform;

namespace AdvertisementApp.Business.Interface
{
    public interface IPlatformService
    {
        Task<List<MessageThreadDto>> GetThreadsAsync(int userId);
        Task<List<MessageDto>> GetMessagesAsync(int userId, int threadId);
        Task<MessageDto?> SendMessageAsync(int userId, SendMessageDto dto);
        Task<MessageDto?> SendThreadMessageAsync(int userId, int threadId, string body);
        Task<OfferDto?> CreateOfferAsync(int userId, CreateOfferDto dto);
        Task<List<OfferDto>> GetOffersForAdAsync(int userId, int advertisementId, bool isOwner);
        Task<List<OfferDto>> GetIncomingOffersForSellerAsync(int userId);
        Task<OfferDto?> RespondToOfferAsync(int sellerUserId, int offerId, bool accept);
        Task<List<PublicBlogListDto>> GetPublishedBlogPostsAsync();
        Task<PublicBlogDetailDto?> GetPublishedBlogPostBySlugAsync(string slug);
        Task<List<PublicStaticPageListDto>> GetActiveStaticPagesAsync();
        Task<PublicStaticPageDto?> GetActiveStaticPageBySlugAsync(string slug);
        Task<bool> ReportListingAsync(int userId, ReportListingDto dto);
        Task<SellerPublicProfileDto?> GetSellerPublicProfileAsync(int sellerUserId);
        Task<SellerPublicProfileDto?> GetSellerPublicProfileBySlugAsync(string slug);
        Task<PackageExperimentDto> GetPackageExperimentAsync(string? variantHint);
        Task LogPackageExperimentAsync(int? userId, LogPackageExperimentDto dto);
        Task<VerificationRequestDto?> SubmitVerificationAsync(int userId, string documentType, string filePath);
        Task<List<VerificationRequestDto>> GetPendingVerificationsAsync();
        Task<bool> ReviewVerificationAsync(int requestId, ReviewVerificationDto dto, int adminUserId);
        Task<SellerAnalyticsDto?> GetSellerAnalyticsAsync(int sellerUserId);
        Task<List<NotificationDto>> GetNotificationsAsync(int userId);
        Task<int> GetUnreadNotificationCountAsync(int userId);
        Task MarkNotificationReadAsync(int userId, int id);
        Task LogSearchAsync(int? userId, int? categoryId, string? searchTerm);
        Task<AnalyticsOverviewDto> GetAnalyticsOverviewAsync();
        Task<AdAnalyticsDto?> GetAdAnalyticsAsync(int userId, int advertisementId);
        Task RecordViewAsync(int advertisementId);
        Task<List<MapListingDto>> GetMapListingsAsync(AdvertisementApp.Dtos.AdvertisementDtos.AdvertisementFilterDto filter);
        Task<List<AdPackageDto>> GetPackagesAsync();
        Task SetFavoritePriceAlertAsync(int userId, int advertisementId, FavoritePriceAlertDto dto);
        Task SavePushSubscriptionAsync(int userId, PushSubscriptionDto dto);
        Task SendUserNotificationAsync(int userId, string type, string title, string body, string? link, string emailSubject);
    }
}
