using AdvertisementApp.Business.Interface;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.Business.Service
{
    public class ReviewService : IReviewService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IRealtimeNotifier _realtime;

        public ReviewService(AdvertisementAppDbContext db, IRealtimeNotifier realtime)
        {
            _db = db;
            _realtime = realtime;
        }

        public async Task<SellerReviewDto?> CreateSellerReviewAsync(int buyerUserId, CreateSellerReviewDto dto)
        {
            if (dto.Rating is < 1 or > 5) return null;

            var order = await _db.MarketplaceOrders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == dto.MarketplaceOrderId && o.BuyerUserId == buyerUserId);
            if (order == null || order.Status != MarketplaceOrderStatus.Completed) return null;
            if (order.SellerUserId != dto.SellerUserId) return null;

            var exists = await _db.SellerReviews.AnyAsync(r => r.MarketplaceOrderId == dto.MarketplaceOrderId);
            if (exists) return null;

            var review = new SellerReview
            {
                SellerUserId = dto.SellerUserId,
                BuyerUserId = buyerUserId,
                MarketplaceOrderId = dto.MarketplaceOrderId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                CreatedTime = DateTime.UtcNow,
            };
            _db.SellerReviews.Add(review);
            await _db.SaveChangesAsync();

            await _realtime.PushToUserAsync(dto.SellerUserId, "notification", new
            {
                type = "new_review",
                rating = dto.Rating,
            });

            return await MapSellerReviewAsync(review);
        }

        public async Task<SellerRatingSummaryDto> GetSellerRatingAsync(int sellerUserId, int page = 1, int pageSize = 10, int? viewerUserId = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var baseQuery = _db.SellerReviews.AsNoTracking()
                .Where(r => r.SellerUserId == sellerUserId && !r.IsHidden);

            var total = await baseQuery.CountAsync();
            var reviews = await baseQuery
                .OrderByDescending(r => r.CreatedTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allRatings = await _db.SellerReviews.AsNoTracking()
                .Where(r => r.SellerUserId == sellerUserId && !r.IsHidden)
                .Select(r => r.Rating)
                .ToListAsync();

            var mapped = new List<SellerReviewDto>();
            foreach (var r in reviews)
                mapped.Add(await MapSellerReviewAsync(r));

            var canReview = false;
            var alreadyReviewed = false;
            int? reviewOrderId = null;
            if (viewerUserId > 0)
            {
                var eligibleOrder = await _db.MarketplaceOrders.AsNoTracking()
                    .Where(o => o.SellerUserId == sellerUserId
                        && o.BuyerUserId == viewerUserId
                        && o.Status == MarketplaceOrderStatus.Completed)
                    .OrderByDescending(o => o.CompletedAt)
                    .Select(o => o.Id)
                    .FirstOrDefaultAsync();

                if (eligibleOrder > 0)
                {
                    var reviewed = await _db.SellerReviews.AsNoTracking()
                        .AnyAsync(r => r.MarketplaceOrderId == eligibleOrder);
                    if (!reviewed)
                    {
                        canReview = true;
                        reviewOrderId = eligibleOrder;
                    }
                    else
                    {
                        alreadyReviewed = true;
                    }
                }
            }

            return new SellerRatingSummaryDto
            {
                AverageRating = allRatings.Count == 0 ? 0 : Math.Round(allRatings.Average(), 1),
                ReviewCount = allRatings.Count,
                RecentReviews = mapped,
                Page = page,
                PageSize = pageSize,
                TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                CanReview = canReview,
                ReviewOrderId = reviewOrderId,
                AlreadyReviewed = alreadyReviewed,
            };
        }

        public async Task<AdvertisementReviewDto?> CreateAdvertisementReviewAsync(int userId, CreateAdvertisementReviewDto dto)
        {
            if (dto.Rating is < 1 or > 5) return null;

            var order = await _db.MarketplaceOrders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == dto.MarketplaceOrderId && o.BuyerUserId == userId);
            if (order == null || order.Status != MarketplaceOrderStatus.Completed) return null;
            if (order.AdvertisementId != dto.AdvertisementId) return null;

            var exists = await _db.AdvertisementReviews.AnyAsync(r => r.MarketplaceOrderId == dto.MarketplaceOrderId);
            if (exists) return null;

            var review = new AdvertisementReview
            {
                AdvertisementId = dto.AdvertisementId,
                UserId = userId,
                MarketplaceOrderId = dto.MarketplaceOrderId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                CreatedTime = DateTime.UtcNow,
            };
            _db.AdvertisementReviews.Add(review);
            await _db.SaveChangesAsync();

            var ad = await _db.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == dto.AdvertisementId);
            if (ad != null)
            {
                await _realtime.PushToUserAsync(ad.UserId, "notification", new
                {
                    type = "new_ad_review",
                    advertisementId = dto.AdvertisementId,
                    rating = dto.Rating,
                });
            }

            return await MapAdvertisementReviewAsync(review);
        }

        public async Task<AdvertisementRatingSummaryDto> GetAdvertisementRatingAsync(
            int advertisementId, int? viewerUserId = null, int page = 1, int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var baseQuery = _db.AdvertisementReviews.AsNoTracking()
                .Where(r => r.AdvertisementId == advertisementId && !r.IsHidden);

            var total = await baseQuery.CountAsync();
            var reviews = await baseQuery
                .OrderByDescending(r => r.CreatedTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allRatings = await _db.AdvertisementReviews.AsNoTracking()
                .Where(r => r.AdvertisementId == advertisementId && !r.IsHidden)
                .Select(r => r.Rating)
                .ToListAsync();

            var mapped = new List<AdvertisementReviewDto>();
            foreach (var r in reviews)
                mapped.Add(await MapAdvertisementReviewAsync(r));

            int? reviewOrderId = null;
            var canReview = false;
            var alreadyReviewed = false;
            if (viewerUserId > 0)
            {
                var eligibleOrder = await _db.MarketplaceOrders.AsNoTracking()
                    .Where(o => o.AdvertisementId == advertisementId
                        && o.BuyerUserId == viewerUserId
                        && o.Status == MarketplaceOrderStatus.Completed)
                    .OrderByDescending(o => o.CompletedAt)
                    .Select(o => o.Id)
                    .FirstOrDefaultAsync();

                if (eligibleOrder > 0)
                {
                    var reviewed = await _db.AdvertisementReviews.AsNoTracking()
                        .AnyAsync(r => r.MarketplaceOrderId == eligibleOrder);
                    if (!reviewed)
                    {
                        canReview = true;
                        reviewOrderId = eligibleOrder;
                    }
                    else
                    {
                        alreadyReviewed = true;
                    }
                }
            }

            return new AdvertisementRatingSummaryDto
            {
                AverageRating = allRatings.Count == 0 ? 0 : Math.Round(allRatings.Average(), 1),
                ReviewCount = allRatings.Count,
                Reviews = mapped,
                Page = page,
                PageSize = pageSize,
                TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                CanReview = canReview,
                ReviewOrderId = reviewOrderId,
                AlreadyReviewed = alreadyReviewed,
            };
        }

        public async Task<AdvertisementReviewDto?> UpdateAdvertisementReviewAsync(int userId, int reviewId, UpdateReviewDto dto)
        {
            if (dto.Rating is < 1 or > 5) return null;
            var review = await _db.AdvertisementReviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);
            if (review == null) return null;

            review.Rating = dto.Rating;
            review.Comment = dto.Comment?.Trim();
            review.UpdatedTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return await MapAdvertisementReviewAsync(review);
        }

        public async Task<bool> DeleteAdvertisementReviewAsync(int userId, int reviewId, bool isAdmin = false)
        {
            var review = await _db.AdvertisementReviews.FirstOrDefaultAsync(r =>
                r.Id == reviewId && (isAdmin || r.UserId == userId));
            if (review == null) return false;
            _db.AdvertisementReviews.Remove(review);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<BuyerReviewDto?> CreateBuyerReviewAsync(int sellerUserId, CreateBuyerReviewDto dto)
        {
            if (dto.Rating is < 1 or > 5) return null;

            var order = await _db.MarketplaceOrders.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == dto.MarketplaceOrderId && o.SellerUserId == sellerUserId);
            if (order == null || order.Status != MarketplaceOrderStatus.Completed) return null;
            if (order.BuyerUserId != dto.BuyerUserId) return null;

            var exists = await _db.BuyerReviews.AnyAsync(r => r.MarketplaceOrderId == dto.MarketplaceOrderId);
            if (exists) return null;

            var review = new BuyerReview
            {
                BuyerUserId = dto.BuyerUserId,
                SellerUserId = sellerUserId,
                MarketplaceOrderId = dto.MarketplaceOrderId,
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                CreatedTime = DateTime.UtcNow,
            };
            _db.BuyerReviews.Add(review);
            await _db.SaveChangesAsync();

            await _realtime.PushToUserAsync(dto.BuyerUserId, "notification", new
            {
                type = "new_buyer_review",
                rating = dto.Rating,
            });

            return await MapBuyerReviewAsync(review);
        }

        public async Task<SellerReviewDto?> UpdateSellerReviewAsync(int userId, int reviewId, UpdateReviewDto dto)
        {
            if (dto.Rating is < 1 or > 5) return null;
            var review = await _db.SellerReviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.BuyerUserId == userId);
            if (review == null) return null;

            review.Rating = dto.Rating;
            review.Comment = dto.Comment?.Trim();
            review.UpdatedTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return await MapSellerReviewAsync(review);
        }

        public async Task<bool> DeleteSellerReviewAsync(int userId, int reviewId, bool isAdmin = false)
        {
            var review = await _db.SellerReviews.FirstOrDefaultAsync(r =>
                r.Id == reviewId && (isAdmin || r.BuyerUserId == userId));
            if (review == null) return false;
            _db.SellerReviews.Remove(review);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<AdminReviewItemDto>> GetAdminReviewsAsync(string? type = null, int take = 50)
        {
            take = Math.Clamp(take, 1, 200);
            var result = new List<AdminReviewItemDto>();

            if (type is null or "seller")
            {
                var sellerReviews = await _db.SellerReviews.AsNoTracking()
                    .OrderByDescending(r => r.CreatedTime)
                    .Take(take)
                    .ToListAsync();
                foreach (var r in sellerReviews)
                {
                    var buyer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == r.BuyerUserId);
                    var seller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == r.SellerUserId);
                    result.Add(new AdminReviewItemDto
                    {
                        Id = r.Id,
                        ReviewType = "seller",
                        Rating = r.Rating,
                        Comment = r.Comment,
                        IsHidden = r.IsHidden,
                        CreatedTime = r.CreatedTime,
                        AuthorName = buyer != null ? $"{buyer.FirstName} {buyer.LastName}".Trim() : "Alıcı",
                        TargetName = seller != null ? $"{seller.FirstName} {seller.LastName}".Trim() : "Satıcı",
                    });
                }
            }

            if (type is null or "advertisement")
            {
                var adReviews = await _db.AdvertisementReviews.AsNoTracking()
                    .OrderByDescending(r => r.CreatedTime)
                    .Take(take)
                    .ToListAsync();
                foreach (var r in adReviews)
                {
                    var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == r.UserId);
                    var ad = await _db.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == r.AdvertisementId);
                    result.Add(new AdminReviewItemDto
                    {
                        Id = r.Id,
                        ReviewType = "advertisement",
                        Rating = r.Rating,
                        Comment = r.Comment,
                        IsHidden = r.IsHidden,
                        CreatedTime = r.CreatedTime,
                        AuthorName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Kullanıcı",
                        AdvertisementId = r.AdvertisementId,
                        AdvertisementTitle = ad?.Title,
                        TargetName = ad?.Title,
                    });
                }
            }

            if (type is null or "buyer")
            {
                var buyerReviews = await _db.BuyerReviews.AsNoTracking()
                    .OrderByDescending(r => r.CreatedTime)
                    .Take(take)
                    .ToListAsync();
                foreach (var r in buyerReviews)
                {
                    var seller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == r.SellerUserId);
                    var buyer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == r.BuyerUserId);
                    result.Add(new AdminReviewItemDto
                    {
                        Id = r.Id,
                        ReviewType = "buyer",
                        Rating = r.Rating,
                        Comment = r.Comment,
                        IsHidden = r.IsHidden,
                        CreatedTime = r.CreatedTime,
                        AuthorName = seller != null ? $"{seller.FirstName} {seller.LastName}".Trim() : "Satıcı",
                        TargetName = buyer != null ? $"{buyer.FirstName} {buyer.LastName}".Trim() : "Alıcı",
                    });
                }
            }

            return result.OrderByDescending(r => r.CreatedTime).Take(take).ToList();
        }

        public async Task<bool> SetReviewHiddenAsync(string reviewType, int reviewId, bool hidden)
        {
            switch (reviewType.ToLowerInvariant())
            {
                case "seller":
                    var sr = await _db.SellerReviews.FirstOrDefaultAsync(r => r.Id == reviewId);
                    if (sr == null) return false;
                    sr.IsHidden = hidden;
                    break;
                case "advertisement":
                    var ar = await _db.AdvertisementReviews.FirstOrDefaultAsync(r => r.Id == reviewId);
                    if (ar == null) return false;
                    ar.IsHidden = hidden;
                    break;
                case "buyer":
                    var br = await _db.BuyerReviews.FirstOrDefaultAsync(r => r.Id == reviewId);
                    if (br == null) return false;
                    br.IsHidden = hidden;
                    break;
                default:
                    return false;
            }
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReviewAdminAsync(string reviewType, int reviewId)
        {
            switch (reviewType.ToLowerInvariant())
            {
                case "seller":
                    var sr = await _db.SellerReviews.FirstOrDefaultAsync(r => r.Id == reviewId);
                    if (sr == null) return false;
                    _db.SellerReviews.Remove(sr);
                    break;
                case "advertisement":
                    var ar = await _db.AdvertisementReviews.FirstOrDefaultAsync(r => r.Id == reviewId);
                    if (ar == null) return false;
                    _db.AdvertisementReviews.Remove(ar);
                    break;
                case "buyer":
                    var br = await _db.BuyerReviews.FirstOrDefaultAsync(r => r.Id == reviewId);
                    if (br == null) return false;
                    _db.BuyerReviews.Remove(br);
                    break;
                default:
                    return false;
            }
            await _db.SaveChangesAsync();
            return true;
        }

        private async Task<SellerReviewDto> MapSellerReviewAsync(SellerReview review)
        {
            var buyer = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == review.BuyerUserId);
            return new SellerReviewDto
            {
                Id = review.Id,
                SellerUserId = review.SellerUserId,
                BuyerName = buyer != null ? $"{buyer.FirstName} {buyer.LastName}".Trim() : "Alıcı",
                MarketplaceOrderId = review.MarketplaceOrderId,
                Rating = review.Rating,
                Comment = review.Comment,
                IsVerifiedPurchase = review.MarketplaceOrderId.HasValue,
                CreatedTime = review.CreatedTime,
            };
        }

        private async Task<AdvertisementReviewDto> MapAdvertisementReviewAsync(AdvertisementReview review)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == review.UserId);
            return new AdvertisementReviewDto
            {
                Id = review.Id,
                AdvertisementId = review.AdvertisementId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Kullanıcı",
                MarketplaceOrderId = review.MarketplaceOrderId,
                Rating = review.Rating,
                Comment = review.Comment,
                IsVerifiedPurchase = review.MarketplaceOrderId.HasValue,
                CreatedTime = review.CreatedTime,
            };
        }

        private async Task<BuyerReviewDto> MapBuyerReviewAsync(BuyerReview review)
        {
            var seller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == review.SellerUserId);
            return new BuyerReviewDto
            {
                Id = review.Id,
                BuyerUserId = review.BuyerUserId,
                SellerName = seller != null ? $"{seller.FirstName} {seller.LastName}".Trim() : "Satıcı",
                MarketplaceOrderId = review.MarketplaceOrderId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedTime = review.CreatedTime,
            };
        }
    }
}
