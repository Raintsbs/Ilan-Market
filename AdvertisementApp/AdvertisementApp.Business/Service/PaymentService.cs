using AdvertisementApp.Business.Configuration;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Dtos.Platform;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stripe.Checkout;

namespace AdvertisementApp.Business.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly AdvertisementAppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IyzicoPaymentHelper _iyzico;
        private readonly IHostEnvironment _env;
        private readonly IEmailTemplateService _emailTemplates;
        private readonly IGrowthService _growth;

        public PaymentService(
            AdvertisementAppDbContext context,
            IConfiguration config,
            IyzicoPaymentHelper iyzico,
            IHostEnvironment env,
            IEmailTemplateService emailTemplates,
            IGrowthService growth)
        {
            _context = context;
            _config = config;
            _iyzico = iyzico;
            _env = env;
            _emailTemplates = emailTemplates;
            _growth = growth;
        }

        private bool DemoPaymentsAllowed =>
            _config.GetValue("Payments:AllowDemo", !_env.IsProduction());

        private bool PreferIyzico =>
            (_config["Payments:PreferredProvider"] ?? "iyzico")
                .Equals("iyzico", StringComparison.OrdinalIgnoreCase);

        public async Task<CheckoutResultDto?> CreateFeaturedCheckoutAsync(int userId, CheckoutDto dto)
        {
            var pkg = await _context.AdPackages.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.AdPackageId && p.IsActive);
            var ad = await _context.Advertisements.FirstOrDefaultAsync(a => a.Id == dto.AdvertisementId && a.UserId == userId);
            if (pkg == null || ad == null) return null;

            decimal discount = 0;
            string? couponCode = null;
            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var couponResult = await _growth.ValidateCouponAsync(new ValidateCouponDto
                {
                    Code = dto.CouponCode,
                    OrderAmount = pkg.Price,
                });
                if (!couponResult.Valid)
                    return null;
                discount = couponResult.DiscountAmount;
                couponCode = dto.CouponCode.Trim().ToUpperInvariant();
            }

            var finalPrice = Math.Max(0, pkg.Price - discount);
            var frontend = _config["App:FrontendUrl"] ?? "http://localhost:3000";
            var apiUrl = _config["App:ApiUrl"] ?? "http://localhost:5050";

            var purchase = new UserPurchase
            {
                UserId = userId,
                AdvertisementId = dto.AdvertisementId,
                AdPackageId = pkg.Id,
                Status = "pending",
                CouponCode = couponCode,
                DiscountAmount = discount,
                PaidAmount = finalPrice,
                CreatedTime = DateTime.UtcNow,
            };
            _context.UserPurchases.Add(purchase);
            await _context.SaveChangesAsync();

            if (finalPrice <= 0 && !string.IsNullOrWhiteSpace(couponCode))
            {
                await CompletePurchaseAsync(purchase.Id, userId);
                return new CheckoutResultDto
                {
                    PurchaseId = purchase.Id,
                    CheckoutUrl = $"{frontend}/one-cikan/basarili?purchaseId={purchase.Id}",
                    Message = "Kupon ile ödeme tamamlandı.",
                    IsDemo = true,
                    PaymentProvider = "coupon",
                };
            }

            CheckoutResultDto? checkout = PreferIyzico
                ? await TryFeaturedIyzicoAsync(userId, purchase, pkg, apiUrl, finalPrice)
                    ?? await TryFeaturedStripeAsync(purchase, pkg, frontend, dto.AdvertisementId, finalPrice)
                : await TryFeaturedStripeAsync(purchase, pkg, frontend, dto.AdvertisementId, finalPrice)
                    ?? await TryFeaturedIyzicoAsync(userId, purchase, pkg, apiUrl, finalPrice);

            if (checkout != null)
                return checkout;

            if (!DemoPaymentsAllowed)
                return null;

            return new CheckoutResultDto
            {
                PurchaseId = purchase.Id,
                CheckoutUrl = $"{frontend}/one-cikan?purchaseId={purchase.Id}&demo=1",
                Message = PaymentOptionsHelper.DemoPaymentHint(_config),
                IsDemo = true,
                PaymentProvider = "demo",
            };
        }

        private async Task<CheckoutResultDto?> TryFeaturedStripeAsync(
            UserPurchase purchase,
            AdPackage pkg,
            string frontend,
            int advertisementId,
            decimal chargeAmount)
        {
            var stripeKey = _config["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(stripeKey)) return null;

            Stripe.StripeConfiguration.ApiKey = stripeKey;
            var session = await new SessionService().CreateAsync(new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = $"{frontend}/one-cikan/basarili?session_id={{CHECKOUT_SESSION_ID}}&provider=stripe",
                CancelUrl = $"{frontend}/one-cikan?adId={advertisementId}",
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "try",
                            UnitAmount = (long)(chargeAmount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = pkg.Name,
                                Description = $"{pkg.FeaturedDays} gün öne çıkarma",
                            },
                        },
                    },
                ],
                Metadata = new Dictionary<string, string>
                {
                    ["purchaseId"] = purchase.Id.ToString(),
                    ["userId"] = purchase.UserId.ToString(),
                },
            });

            purchase.StripeSessionId = session.Id;
            await _context.SaveChangesAsync();

            return new CheckoutResultDto
            {
                PurchaseId = purchase.Id,
                CheckoutUrl = session.Url ?? $"{frontend}/one-cikan?adId={advertisementId}",
                Message = "Stripe ödeme sayfasına yönlendiriliyorsunuz.",
                IsDemo = false,
                StripeSessionId = session.Id,
                PaymentProvider = "stripe",
            };
        }

        private async Task<CheckoutResultDto?> TryFeaturedIyzicoAsync(
            int userId,
            UserPurchase purchase,
            AdPackage pkg,
            string apiUrl,
            decimal chargeAmount)
        {
            if (!_iyzico.IsConfigured) return null;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var iyzico = await _iyzico.CreateCheckoutAsync(
                user,
                purchase.Id.ToString(),
                purchase.Id.ToString(),
                chargeAmount,
                pkg.Name,
                "One Cikan",
                $"{apiUrl}/api/payments/iyzico-callback?type=featured&purchaseId={purchase.Id}");
            if (iyzico == null) return null;

            purchase.IyzicoToken = iyzico.Token;
            await _context.SaveChangesAsync();
            return new CheckoutResultDto
            {
                PurchaseId = purchase.Id,
                CheckoutUrl = iyzico.PaymentPageUrl,
                Message = "iyzico ödeme sayfasına yönlendiriliyorsunuz.",
                IsDemo = false,
                PaymentProvider = "iyzico",
                IyzicoToken = iyzico.Token,
            };
        }

        public async Task<bool> CompleteCheckoutAsync(int userId, int purchaseId)
        {
            if (!DemoPaymentsAllowed)
            {
                var pending = await _context.UserPurchases.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == purchaseId && p.UserId == userId);
                if (pending != null &&
                    pending.Status == "pending" &&
                    string.IsNullOrEmpty(pending.StripeSessionId) &&
                    string.IsNullOrEmpty(pending.IyzicoToken))
                    return false;
            }

            return await CompletePurchaseAsync(purchaseId, userId);
        }

        public async Task<bool> CompleteStripeSessionAsync(string sessionId)
        {
            var purchase = await _context.UserPurchases
                .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId && p.Status == "pending");
            if (purchase != null)
                return await CompletePurchaseAsync(purchase.Id, purchase.UserId);

            var order = await _context.MarketplaceOrders
                .Include(o => o.Advertisement)
                .FirstOrDefaultAsync(o => o.StripeSessionId == sessionId && o.Status == MarketplaceOrderStatus.AwaitingPayment);
            if (order == null) return false;

            order.Status = MarketplaceOrderStatus.PaidEscrow;
            order.PaidAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await SendEscrowPaymentEmailAsync(order);
            return true;
        }

        public async Task<bool> CompleteIyzicoTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            var checkout = await _iyzico.RetrieveCheckoutAsync(token);
            if (checkout == null || checkout.PaymentStatus != "SUCCESS") return false;

            var purchase = await _context.UserPurchases
                .FirstOrDefaultAsync(p => p.IyzicoToken == token && p.Status == "pending");
            if (purchase != null)
                return await CompletePurchaseAsync(purchase.Id, purchase.UserId);

            var order = await _context.MarketplaceOrders
                .Include(o => o.Advertisement)
                .FirstOrDefaultAsync(o => o.IyzicoToken == token && o.Status == MarketplaceOrderStatus.AwaitingPayment);
            if (order == null) return false;

            order.Status = MarketplaceOrderStatus.PaidEscrow;
            order.PaidAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await SendEscrowPaymentEmailAsync(order);
            return true;
        }

        public async Task<PayMarketplaceOrderResultDto?> CreateEscrowCheckoutAsync(int buyerUserId, int orderId)
        {
            var order = await _context.MarketplaceOrders
                .Include(o => o.Advertisement)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerUserId == buyerUserId);
            if (order == null || order.Status != MarketplaceOrderStatus.AwaitingPayment) return null;

            var frontend = _config["App:FrontendUrl"] ?? "http://localhost:3000";
            var apiUrl = _config["App:ApiUrl"] ?? "http://localhost:5050";
            var title = order.Advertisement?.Title ?? "İlan";

            PayMarketplaceOrderResultDto? checkout = PreferIyzico
                ? await TryEscrowIyzicoAsync(buyerUserId, order, title, apiUrl)
                    ?? await TryEscrowStripeAsync(order, buyerUserId, title, frontend)
                : await TryEscrowStripeAsync(order, buyerUserId, title, frontend)
                    ?? await TryEscrowIyzicoAsync(buyerUserId, order, title, apiUrl);

            return checkout;
        }

        private async Task SendEscrowPaymentEmailAsync(MarketplaceOrder order)
        {
            var buyer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == order.BuyerUserId);
            if (string.IsNullOrWhiteSpace(buyer?.Email)) return;
            var title = order.Advertisement?.Title ?? "İlan";
            await _emailTemplates.SendPaymentConfirmedAsync(
                buyer.Email,
                $"\"{title}\" için escrow ödemeniz alındı.",
                order.Amount,
                $"/siparisler/{order.Id}");
        }

        private async Task<PayMarketplaceOrderResultDto?> TryEscrowStripeAsync(
            MarketplaceOrder order,
            int buyerUserId,
            string title,
            string frontend)
        {
            var stripeKey = _config["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(stripeKey)) return null;

            Stripe.StripeConfiguration.ApiKey = stripeKey;
            var session = await new SessionService().CreateAsync(new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = $"{frontend}/siparisler/{order.Id}?paid=1&session_id={{CHECKOUT_SESSION_ID}}&provider=stripe",
                CancelUrl = $"{frontend}/satin-al?adId={order.AdvertisementId}",
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "try",
                            UnitAmount = (long)(order.Amount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = title,
                                Description = "Param Güvende — escrow ödemesi",
                            },
                        },
                    },
                ],
                Metadata = new Dictionary<string, string>
                {
                    ["orderId"] = order.Id.ToString(),
                    ["buyerUserId"] = buyerUserId.ToString(),
                    ["type"] = "escrow",
                },
            });

            order.StripeSessionId = session.Id;
            await _context.SaveChangesAsync();

            return new PayMarketplaceOrderResultDto
            {
                StripeCheckoutUrl = session.Url,
                CheckoutUrl = session.Url,
                IsDemo = false,
                PaymentProvider = "stripe",
                Message = "Stripe ödeme sayfasına yönlendiriliyorsunuz.",
            };
        }

        private async Task<PayMarketplaceOrderResultDto?> TryEscrowIyzicoAsync(
            int buyerUserId,
            MarketplaceOrder order,
            string title,
            string apiUrl)
        {
            if (!_iyzico.IsConfigured) return null;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == buyerUserId);
            if (user == null) return null;

            var iyzico = await _iyzico.CreateCheckoutAsync(
                user,
                order.Id.ToString(),
                order.Id.ToString(),
                order.Amount,
                title,
                "Escrow",
                $"{apiUrl}/api/payments/iyzico-callback?type=escrow&orderId={order.Id}");
            if (iyzico == null) return null;

            order.IyzicoToken = iyzico.Token;
            await _context.SaveChangesAsync();
            return new PayMarketplaceOrderResultDto
            {
                CheckoutUrl = iyzico.PaymentPageUrl,
                IsDemo = false,
                PaymentProvider = "iyzico",
                Message = "iyzico ödeme sayfasına yönlendiriliyorsunuz.",
            };
        }

        private async Task<bool> CompletePurchaseAsync(int purchaseId, int userId)
        {
            var purchase = await _context.UserPurchases
                .Include(p => p.AdPackage)
                .FirstOrDefaultAsync(p => p.Id == purchaseId && p.UserId == userId);
            if (purchase == null || purchase.Status == "completed") return false;

            purchase.Status = "completed";
            if (!string.IsNullOrWhiteSpace(purchase.CouponCode))
                await _growth.RedeemCouponAsync(purchase.CouponCode);

            if (purchase.AdvertisementId.HasValue)
            {
                var ad = await _context.Advertisements.FirstOrDefaultAsync(a => a.Id == purchase.AdvertisementId);
                if (ad != null)
                {
                    ad.IsFeatured = true;
                    ad.FeaturedUntil = DateTime.UtcNow.AddDays(purchase.AdPackage.FeaturedDays);
                    ad.LastBumpedAt = DateTime.UtcNow;
                }
            }
            await _context.SaveChangesAsync();

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (!string.IsNullOrWhiteSpace(user?.Email))
            {
                var paid = purchase.PaidAmount > 0 ? purchase.PaidAmount : purchase.AdPackage.Price;
                await _emailTemplates.SendPaymentConfirmedAsync(
                    user.Email,
                    $"\"{purchase.AdPackage.Name}\" paketi ödemeniz onaylandı.",
                    paid,
                    purchase.AdvertisementId.HasValue ? $"/ilan/{purchase.AdvertisementId}" : "/ilanlarim");
            }

            return true;
        }
    }
}
