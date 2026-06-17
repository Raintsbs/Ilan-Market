using AdvertisementApp.Business.Configuration;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Helpers;
using AdvertisementApp.Common.Models;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Dtos.Admin;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.Business.Service
{
    public class MarketplaceOrderService : IMarketplaceOrderService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IRealtimeNotifier _realtime;
        private readonly IEmailTemplateService _emailTemplates;
        private readonly IConfiguration _config;
        private readonly IPaymentService _payments;
        private readonly IHostEnvironment _env;

        public MarketplaceOrderService(
            AdvertisementAppDbContext db,
            IRealtimeNotifier realtime,
            IEmailTemplateService emailTemplates,
            IConfiguration config,
            IPaymentService payments,
            IHostEnvironment env)
        {
            _db = db;
            _realtime = realtime;
            _emailTemplates = emailTemplates;
            _config = config;
            _payments = payments;
            _env = env;
        }

        public Task<List<CargoCarrierDto>> GetCarriersAsync() =>
            Task.FromResult(CargoCarrierUrls.Names.Select(kv => new CargoCarrierDto { Code = kv.Key, Name = kv.Value }).ToList());

        public async Task<MarketplaceOrderDto?> CreateOrderAsync(int buyerUserId, CreateMarketplaceOrderDto dto)
        {
            var ad = await _db.Advertisements.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == dto.AdvertisementId && a.IsActive && a.Status == AdvertisementStatus.Approved);
            if (ad == null || ad.UserId == buyerUserId) return null;

            var details = ListingDetailsHelper.Parse(ad.ListingDetailsJson);
            if (details?.Price is not > 0) return null;

            var method = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? "param_guvende" : dto.PaymentMethod.Trim().ToLowerInvariant();
            if (method is not ("card" or "param_guvende")) return null;

            var order = new MarketplaceOrder
            {
                AdvertisementId = ad.Id,
                BuyerUserId = buyerUserId,
                SellerUserId = ad.UserId,
                Amount = details.Price.Value,
                PaymentMethod = method,
                Status = MarketplaceOrderStatus.AwaitingPayment,
                CreatedTime = DateTime.UtcNow,
            };
            _db.MarketplaceOrders.Add(order);
            await _db.SaveChangesAsync();
            return await MapOrderAsync(order.Id, buyerUserId);
        }

        public async Task<PayMarketplaceOrderResultDto?> PayOrderAsync(int buyerUserId, int orderId, PayMarketplaceOrderDto? dto)
        {
            var order = await _db.MarketplaceOrders
                .Include(o => o.Advertisement)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerUserId == buyerUserId);
            if (order == null || order.Status != MarketplaceOrderStatus.AwaitingPayment) return null;

            var checkout = await _payments.CreateEscrowCheckoutAsync(buyerUserId, orderId);
            if (checkout != null && !string.IsNullOrWhiteSpace(checkout.CheckoutUrl ?? checkout.StripeCheckoutUrl))
                return checkout;

            if (PaymentOptionsHelper.IsRealPaymentConfigured(_config))
                return null;

            var allowDemo = _config.GetValue("Payments:AllowDemo", !_env.IsProduction());
            if (!allowDemo)
                return null;

            order.Status = MarketplaceOrderStatus.PaidEscrow;
            order.PaidAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var adTitle = order.Advertisement?.Title ?? "İlan";
            await NotifyAsync(order.SellerUserId, "order_paid",
                "Yeni satın alma (escrow)",
                $"\"{adTitle}\" için {order.Amount:N0} TL ödeme alındı (güvende).",
                $"/siparisler/{order.Id}");

            await _realtime.PushToUserAsync(order.SellerUserId, "notification", new { type = "order_paid", orderId = order.Id });
            var mapped = await MapOrderAsync(order.Id, buyerUserId);
            return new PayMarketplaceOrderResultDto
            {
                Order = mapped,
                IsDemo = true,
                Message = PaymentOptionsHelper.DemoPaymentHint(_config),
            };
        }

        public async Task<MarketplaceOrderDto?> ShipOrderAsync(int sellerUserId, int orderId, ShipOrderDto dto)
        {
            var order = await _db.MarketplaceOrders
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.SellerUserId == sellerUserId);
            if (order == null || order.Status is not (MarketplaceOrderStatus.PaidEscrow or MarketplaceOrderStatus.Preparing)) return null;
            if (string.IsNullOrWhiteSpace(dto.CarrierCode) || string.IsNullOrWhiteSpace(dto.TrackingNumber)) return null;

            var shipment = order.Shipment ?? new OrderShipment { MarketplaceOrderId = order.Id };
            shipment.CarrierCode = dto.CarrierCode.Trim().ToLowerInvariant();
            shipment.TrackingNumber = dto.TrackingNumber.Trim();
            shipment.Status = "shipped";
            shipment.ShippedAt = DateTime.UtcNow;
            if (order.Shipment == null) _db.OrderShipments.Add(shipment);

            order.Status = MarketplaceOrderStatus.Shipped;
            await _db.SaveChangesAsync();

            var trackingUrl = CargoCarrierUrls.GetTrackingUrl(shipment.CarrierCode, shipment.TrackingNumber);
            await NotifyAsync(order.BuyerUserId, "order_shipped",
                "Kargonuz yola çıktı",
                $"{CargoCarrierUrls.GetCarrierName(shipment.CarrierCode)} — takip: {shipment.TrackingNumber}",
                trackingUrl.StartsWith("http") ? null : $"/siparisler/{order.Id}");

            await _realtime.PushToUserAsync(order.BuyerUserId, "notification", new { type = "order_shipped", orderId = order.Id, trackingUrl });
            return await MapOrderAsync(order.Id, sellerUserId);
        }

        public async Task<MarketplaceOrderDto?> ConfirmDeliveryAsync(int buyerUserId, int orderId)
        {
            var order = await _db.MarketplaceOrders
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerUserId == buyerUserId);
            if (order == null || order.Status is not (MarketplaceOrderStatus.Shipped or MarketplaceOrderStatus.Delivered)) return null;

            order.Status = MarketplaceOrderStatus.Completed;
            order.CompletedAt = DateTime.UtcNow;
            if (order.Shipment != null)
            {
                order.Shipment.Status = "delivered";
                order.Shipment.DeliveredAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();

            await NotifyAsync(order.SellerUserId, "order_completed",
                "Satış tamamlandı",
                $"{order.Amount:N0} TL escrow serbest bırakıldı.",
                $"/siparisler/{order.Id}");

            await NotifyAsync(order.BuyerUserId, "review_prompt",
                "Değerlendirme zamanı",
                "Satıcıyı ve ilanı değerlendirerek diğer alıcılara yardımcı olun.",
                $"/siparisler/{order.Id}");

            await _realtime.PushToUserAsync(order.SellerUserId, "notification", new { type = "order_completed", orderId = order.Id });
            return await MapOrderAsync(order.Id, buyerUserId);
        }

        public async Task<MarketplaceOrderDto?> GetOrderAsync(int userId, int orderId)
        {
            var exists = await _db.MarketplaceOrders.AsNoTracking()
                .AnyAsync(o => o.Id == orderId && (o.BuyerUserId == userId || o.SellerUserId == userId));
            if (!exists) return null;
            return await MapOrderAsync(orderId, userId);
        }

        public async Task<List<MarketplaceOrderDto>> GetMyOrdersAsync(int userId, bool asSeller)
        {
            var query = _db.MarketplaceOrders.AsNoTracking().AsQueryable();
            query = asSeller ? query.Where(o => o.SellerUserId == userId) : query.Where(o => o.BuyerUserId == userId);
            var ids = await query.OrderByDescending(o => o.CreatedTime).Take(50).Select(o => o.Id).ToListAsync();
            var result = new List<MarketplaceOrderDto>();
            foreach (var id in ids)
            {
                var mapped = await MapOrderAsync(id, userId);
                if (mapped != null) result.Add(mapped);
            }
            return result;
        }

        public async Task<MarketplaceOrderDto?> OpenDisputeAsync(int userId, int orderId, OpenDisputeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return null;

            var order = await _db.MarketplaceOrders
                .Include(o => o.Advertisement)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return null;
            if (order.BuyerUserId != userId && order.SellerUserId != userId) return null;
            if (order.Status is MarketplaceOrderStatus.Disputed
                or MarketplaceOrderStatus.Completed
                or MarketplaceOrderStatus.Cancelled
                or MarketplaceOrderStatus.AwaitingPayment)
                return null;

            order.Status = MarketplaceOrderStatus.Disputed;
            order.DisputeReason = dto.Reason.Trim();
            order.DisputedAt = DateTime.UtcNow;
            order.DisputedByUserId = userId;
            await _db.SaveChangesAsync();

            var adTitle = order.Advertisement?.Title ?? "İlan";
            var otherUserId = userId == order.BuyerUserId ? order.SellerUserId : order.BuyerUserId;
            await NotifyAsync(otherUserId, "order_disputed",
                "Sipariş itirazı",
                $"\"{adTitle}\" siparişi için itiraz açıldı.",
                $"/siparisler/{order.Id}");

            return await MapOrderAsync(order.Id, userId);
        }

        public async Task<List<MarketplaceOrderDto>> GetDisputedOrdersAsync()
        {
            var ids = await _db.MarketplaceOrders.AsNoTracking()
                .Where(o => o.Status == MarketplaceOrderStatus.Disputed)
                .OrderByDescending(o => o.DisputedAt)
                .Select(o => o.Id)
                .Take(100)
                .ToListAsync();

            var result = new List<MarketplaceOrderDto>();
            foreach (var id in ids)
            {
                var mapped = await MapOrderAsync(id, 0);
                if (mapped != null) result.Add(mapped);
            }
            return result;
        }

        public async Task<MarketplaceOrderDto?> ResolveDisputeAsync(int orderId, ResolveDisputeDto dto, int adminUserId)
        {
            var order = await _db.MarketplaceOrders
                .Include(o => o.Advertisement)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null || order.Status != MarketplaceOrderStatus.Disputed) return null;

            var resolution = (dto.Resolution ?? "complete").Trim().ToLowerInvariant();
            order.DisputeResolutionNote = dto.AdminNote?.Trim();
            order.DisputeResolvedAt = DateTime.UtcNow;

            if (resolution == "cancel")
            {
                order.Status = MarketplaceOrderStatus.Cancelled;
                order.CancelledAt = DateTime.UtcNow;
            }
            else
            {
                order.Status = MarketplaceOrderStatus.Completed;
                order.CompletedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();

            var adTitle = order.Advertisement?.Title ?? "İlan";
            var msg = resolution == "cancel"
                ? $"\"{adTitle}\" siparişi iptal edildi (itiraz çözüldü)."
                : $"\"{adTitle}\" siparişi tamamlandı (itiraz çözüldü).";

            await NotifyAsync(order.BuyerUserId, "order_dispute_resolved", "İtiraz sonuçlandı", msg, $"/siparisler/{order.Id}");
            await NotifyAsync(order.SellerUserId, "order_dispute_resolved", "İtiraz sonuçlandı", msg, $"/siparisler/{order.Id}");

            if (resolution != "cancel")
            {
                await NotifyAsync(order.BuyerUserId, "review_prompt",
                    "Değerlendirme zamanı",
                    "Satıcıyı ve ilanı değerlendirerek diğer alıcılara yardımcı olun.",
                    $"/siparisler/{order.Id}");
            }

            return await MapOrderAsync(order.Id, adminUserId);
        }

        public async Task<PagedResult<MarketplaceOrderDto>> GetAdminOrdersAsync(AdminMarketplaceOrderFilterDto filter)
        {
            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);
            var search = filter.Search?.Trim();

            var query =
                from o in _db.MarketplaceOrders.AsNoTracking()
                join a in _db.Advertisements.AsNoTracking() on o.AdvertisementId equals a.Id
                select new { Order = o, AdTitle = a.Title };

            if (filter.Status.HasValue)
                query = query.Where(x => (int)x.Order.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                if (int.TryParse(search, out var orderId))
                    query = query.Where(x => x.Order.Id == orderId);
                else
                    query = query.Where(x => x.AdTitle.Contains(search));
            }

            var total = await query.CountAsync();
            var ids = await query
                .OrderByDescending(x => x.Order.CreatedTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Order.Id)
                .ToListAsync();

            var items = new List<MarketplaceOrderDto>();
            foreach (var id in ids)
            {
                var mapped = await MapOrderAsync(id, 0);
                if (mapped != null) items.Add(mapped);
            }

            return new PagedResult<MarketplaceOrderDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
            };
        }

        public async Task<MarketplaceOrderDto?> CancelOrderAsync(int actorUserId, int orderId, bool asAdmin)
        {
            var order = await _db.MarketplaceOrders
                .Include(o => o.Advertisement)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return null;

            if (!asAdmin)
            {
                if (order.BuyerUserId != actorUserId) return null;
                if (order.Status != MarketplaceOrderStatus.AwaitingPayment) return null;
            }
            else if (order.Status is MarketplaceOrderStatus.Completed or MarketplaceOrderStatus.Cancelled)
            {
                return null;
            }

            order.Status = MarketplaceOrderStatus.Cancelled;
            order.CancelledAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var adTitle = order.Advertisement?.Title ?? "İlan";
            var msg = $"\"{adTitle}\" siparişi iptal edildi.";
            await NotifyAsync(order.BuyerUserId, "order_cancelled", "Sipariş iptal", msg, $"/siparisler/{order.Id}");
            await NotifyAsync(order.SellerUserId, "order_cancelled", "Sipariş iptal", msg, $"/siparisler/{order.Id}");
            return await MapOrderAsync(order.Id, actorUserId);
        }

        public async Task<MarketplaceOrderDto?> MarkRefundedAsync(int orderId, string? note)
        {
            var order = await _db.MarketplaceOrders
                .Include(o => o.Advertisement)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null || order.Status != MarketplaceOrderStatus.Cancelled || order.RefundedAt != null)
                return null;

            order.RefundedAt = DateTime.UtcNow;
            order.RefundNote = note?.Trim();
            await _db.SaveChangesAsync();

            var adTitle = order.Advertisement?.Title ?? "İlan";
            var buyer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == order.BuyerUserId);
            if (!string.IsNullOrWhiteSpace(buyer?.Email))
            {
                await _emailTemplates.SendPaymentConfirmedAsync(
                    buyer.Email,
                    $"\"{adTitle}\" siparişi için iade işlemi kaydedildi.",
                    order.Amount,
                    $"/siparisler/{order.Id}");
            }

            await NotifyAsync(order.BuyerUserId, "order_refunded", "İade kaydedildi",
                $"{order.Amount:N0} TL iade işlemi tamamlandı olarak işaretlendi.", $"/siparisler/{order.Id}");
            return await MapOrderAsync(order.Id, 0);
        }

        public async Task<MarketplaceOrderDto?> MarkSellerPayoutAsync(int orderId, string? note)
        {
            var order = await _db.MarketplaceOrders
                .Include(o => o.Advertisement)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null || order.Status != MarketplaceOrderStatus.Completed || order.SellerPaidOutAt != null)
                return null;

            order.SellerPaidOutAt = DateTime.UtcNow;
            order.SellerPayoutNote = note?.Trim();
            await _db.SaveChangesAsync();

            await NotifyAsync(order.SellerUserId, "seller_payout", "Satıcı ödemesi",
                $"{order.Amount:N0} TL satıcı ödemesi manuel olarak işaretlendi.", $"/siparisler/{order.Id}");

            var seller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == order.SellerUserId);
            if (!string.IsNullOrWhiteSpace(seller?.Email))
            {
                await _emailTemplates.SendSellerPayoutEmailAsync(
                    seller.Email,
                    order.Amount,
                    order.SellerPayoutNote,
                    $"/siparisler/{order.Id}");
            }

            return await MapOrderAsync(order.Id, 0);
        }

        private async Task<MarketplaceOrderDto?> MapOrderAsync(int orderId, int viewerUserId)
        {
            var row = await (
                from o in _db.MarketplaceOrders.AsNoTracking()
                join a in _db.Advertisements.AsNoTracking() on o.AdvertisementId equals a.Id
                where o.Id == orderId
                select new { Order = o, AdTitle = a.Title }
            ).FirstOrDefaultAsync();
            if (row == null) return null;

            var shipment = await _db.OrderShipments.AsNoTracking()
                .FirstOrDefaultAsync(s => s.MarketplaceOrderId == orderId);

            OrderShipmentDto? shipDto = null;
            if (shipment != null)
            {
                shipDto = new OrderShipmentDto
                {
                    Id = shipment.Id,
                    CarrierCode = shipment.CarrierCode,
                    CarrierName = CargoCarrierUrls.GetCarrierName(shipment.CarrierCode),
                    TrackingNumber = shipment.TrackingNumber,
                    TrackingUrl = CargoCarrierUrls.GetTrackingUrl(shipment.CarrierCode, shipment.TrackingNumber),
                    Status = shipment.Status,
                    ShippedAt = shipment.ShippedAt,
                    DeliveredAt = shipment.DeliveredAt,
                };
            }

            var hasSellerReview = await _db.SellerReviews.AsNoTracking()
                .AnyAsync(r => r.MarketplaceOrderId == orderId && r.BuyerUserId == viewerUserId);

            var hasAdReview = await _db.AdvertisementReviews.AsNoTracking()
                .AnyAsync(r => r.MarketplaceOrderId == orderId);

            var hasBuyerReview = await _db.BuyerReviews.AsNoTracking()
                .AnyAsync(r => r.MarketplaceOrderId == orderId && r.SellerUserId == viewerUserId);

            var isCompleted = row.Order.Status == MarketplaceOrderStatus.Completed;

            var canDispute = viewerUserId > 0
                && (row.Order.BuyerUserId == viewerUserId || row.Order.SellerUserId == viewerUserId)
                && row.Order.Status is MarketplaceOrderStatus.PaidEscrow
                    or MarketplaceOrderStatus.Preparing
                    or MarketplaceOrderStatus.Shipped
                    or MarketplaceOrderStatus.Delivered;

            return new MarketplaceOrderDto
            {
                Id = row.Order.Id,
                AdvertisementId = row.Order.AdvertisementId,
                AdvertisementTitle = row.AdTitle,
                BuyerUserId = row.Order.BuyerUserId,
                SellerUserId = row.Order.SellerUserId,
                Amount = row.Order.Amount,
                PaymentMethod = row.Order.PaymentMethod,
                Status = (int)row.Order.Status,
                StatusLabel = StatusLabel(row.Order.Status),
                CreatedTime = row.Order.CreatedTime,
                PaidAt = row.Order.PaidAt,
                CompletedAt = row.Order.CompletedAt,
                Shipment = shipDto,
                CanReview = isCompleted && row.Order.BuyerUserId == viewerUserId && !hasSellerReview,
                CanReviewAd = isCompleted && row.Order.BuyerUserId == viewerUserId && !hasAdReview,
                CanReviewBuyer = isCompleted && row.Order.SellerUserId == viewerUserId && !hasBuyerReview,
                CanOpenDispute = canDispute,
                DisputeReason = row.Order.DisputeReason,
                DisputedAt = row.Order.DisputedAt,
                DisputeResolutionNote = row.Order.DisputeResolutionNote,
                CancelledAt = row.Order.CancelledAt,
                RefundedAt = row.Order.RefundedAt,
                RefundNote = row.Order.RefundNote,
                SellerPaidOutAt = row.Order.SellerPaidOutAt,
                SellerPayoutNote = row.Order.SellerPayoutNote,
            };
        }

        private static string StatusLabel(MarketplaceOrderStatus s) => s switch
        {
            MarketplaceOrderStatus.AwaitingPayment => "Ödeme bekleniyor",
            MarketplaceOrderStatus.PaidEscrow => "Ödeme güvende",
            MarketplaceOrderStatus.Preparing => "Hazırlanıyor",
            MarketplaceOrderStatus.Shipped => "Kargoda",
            MarketplaceOrderStatus.Delivered => "Teslim edildi",
            MarketplaceOrderStatus.Completed => "Tamamlandı",
            MarketplaceOrderStatus.Cancelled => "İptal",
            MarketplaceOrderStatus.Disputed => "İtiraz",
            _ => s.ToString(),
        };

        private async Task NotifyAsync(int userId, string type, string title, string body, string? link)
        {
            var n = new AppNotification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                Link = link,
                CreatedTime = DateTime.UtcNow,
            };
            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();
            await _realtime.PushToUserAsync(userId, "notification", new { id = n.Id, type, title, body, link });

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (string.IsNullOrWhiteSpace(user?.Email)) return;
            await _emailTemplates.SendOrderNotificationAsync(user.Email, title, body, link);
        }
    }
}
