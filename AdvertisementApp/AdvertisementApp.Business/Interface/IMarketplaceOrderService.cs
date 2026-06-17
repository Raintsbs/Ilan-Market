using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.Admin;
using AdvertisementApp.Dtos.Marketplace;

namespace AdvertisementApp.Business.Interface
{
    public interface IMarketplaceOrderService
    {
        Task<List<CargoCarrierDto>> GetCarriersAsync();
        Task<MarketplaceOrderDto?> CreateOrderAsync(int buyerUserId, CreateMarketplaceOrderDto dto);
        Task<PayMarketplaceOrderResultDto?> PayOrderAsync(int buyerUserId, int orderId, PayMarketplaceOrderDto? dto);
        Task<MarketplaceOrderDto?> ShipOrderAsync(int sellerUserId, int orderId, ShipOrderDto dto);
        Task<MarketplaceOrderDto?> ConfirmDeliveryAsync(int buyerUserId, int orderId);
        Task<MarketplaceOrderDto?> GetOrderAsync(int userId, int orderId);
        Task<List<MarketplaceOrderDto>> GetMyOrdersAsync(int userId, bool asSeller);
        Task<MarketplaceOrderDto?> OpenDisputeAsync(int userId, int orderId, OpenDisputeDto dto);
        Task<List<MarketplaceOrderDto>> GetDisputedOrdersAsync();
        Task<MarketplaceOrderDto?> ResolveDisputeAsync(int orderId, ResolveDisputeDto dto, int adminUserId);
        Task<PagedResult<MarketplaceOrderDto>> GetAdminOrdersAsync(AdminMarketplaceOrderFilterDto filter);
        Task<MarketplaceOrderDto?> CancelOrderAsync(int actorUserId, int orderId, bool asAdmin);
        Task<MarketplaceOrderDto?> MarkRefundedAsync(int orderId, string? note);
        Task<MarketplaceOrderDto?> MarkSellerPayoutAsync(int orderId, string? note);
    }
}
