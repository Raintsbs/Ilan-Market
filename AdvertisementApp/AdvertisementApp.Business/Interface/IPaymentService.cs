using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Dtos.Platform;

namespace AdvertisementApp.Business.Interface
{
    public interface IPaymentService
    {
        Task<CheckoutResultDto?> CreateFeaturedCheckoutAsync(int userId, CheckoutDto dto);
        Task<PayMarketplaceOrderResultDto?> CreateEscrowCheckoutAsync(int buyerUserId, int orderId);
        Task<bool> CompleteCheckoutAsync(int userId, int purchaseId);
        Task<bool> CompleteStripeSessionAsync(string sessionId);
        Task<bool> CompleteIyzicoTokenAsync(string token);
    }
}
