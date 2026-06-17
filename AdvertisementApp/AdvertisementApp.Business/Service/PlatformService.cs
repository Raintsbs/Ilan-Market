using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Helpers;
using Microsoft.Extensions.Configuration;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Dtos.Platform;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.Business.Service
{
    public class PlatformService : IPlatformService
    {
        private readonly AdvertisementAppDbContext _context;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly IRealtimeNotifier _realtime;
        private readonly IWebPushNotificationService _webPush;

        private static readonly Dictionary<string, (double Lat, double Lng)> CityCoords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["İstanbul"] = (41.0082, 28.9784),
            ["Ankara"] = (39.9334, 32.8597),
            ["İzmir"] = (38.4192, 27.1287),
            ["Bursa"] = (40.1885, 29.0610),
            ["Antalya"] = (36.8969, 30.7133),
            ["Adana"] = (37.0000, 35.3213),
            ["Konya"] = (37.8746, 32.4932),
            ["Gaziantep"] = (37.0662, 37.3833),
            ["Mersin"] = (36.8121, 34.6415),
            ["Kocaeli"] = (40.7656, 29.9406),
        };

        public PlatformService(
            AdvertisementAppDbContext context,
            IEmailService email,
            IConfiguration config,
            IRealtimeNotifier realtime,
            IWebPushNotificationService webPush)
        {
            _context = context;
            _email = email;
            _config = config;
            _realtime = realtime;
            _webPush = webPush;
        }

        public async Task<List<MessageThreadDto>> GetThreadsAsync(int userId)
        {
            var threads = await _context.MessageThreads
                .AsNoTracking()
                .Include(t => t.Advertisement)
                .Include(t => t.Messages)
                .Where(t => t.BuyerUserId == userId || t.SellerUserId == userId)
                .OrderByDescending(t => t.UpdatedTime ?? t.CreatedTime)
                .ToListAsync();

            var otherIds = threads
                .Select(t => t.BuyerUserId == userId ? t.SellerUserId : t.BuyerUserId)
                .Distinct()
                .ToList();
            var users = await _context.Users.AsNoTracking()
                .Where(u => otherIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            return threads.Select(t =>
            {
                var otherId = t.BuyerUserId == userId ? t.SellerUserId : t.BuyerUserId;
                users.TryGetValue(otherId, out var other);
                var last = t.Messages.OrderByDescending(m => m.CreatedTime).FirstOrDefault();
                return new MessageThreadDto
                {
                    Id = t.Id,
                    AdvertisementId = t.AdvertisementId,
                    AdvertisementTitle = t.Advertisement.Title,
                    OtherUserId = otherId,
                    OtherUserName = other != null ? $"{other.FirstName} {other.LastName}".Trim() : "Kullanıcı",
                    LastMessage = last?.Body,
                    LastMessageTime = last?.CreatedTime,
                    UnreadCount = t.Messages.Count(m => !m.IsRead && m.SenderUserId != userId),
                };
            }).ToList();
        }

        public async Task<List<MessageDto>> GetMessagesAsync(int userId, int threadId)
        {
            var thread = await _context.MessageThreads.FirstOrDefaultAsync(t => t.Id == threadId);
            if (thread == null || (thread.BuyerUserId != userId && thread.SellerUserId != userId))
                return new List<MessageDto>();

            var toMark = await _context.Messages
                .Where(m => m.ThreadId == threadId && !m.IsRead && m.SenderUserId != userId)
                .ToListAsync();
            foreach (var m in toMark) m.IsRead = true;
            if (toMark.Count > 0) await _context.SaveChangesAsync();

            return await _context.Messages.AsNoTracking()
                .Where(m => m.ThreadId == threadId)
                .OrderBy(m => m.CreatedTime)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    ThreadId = m.ThreadId,
                    SenderUserId = m.SenderUserId,
                    IsMine = m.SenderUserId == userId,
                    Body = m.Body,
                    CreatedTime = m.CreatedTime,
                })
                .ToListAsync();
        }

        public async Task<MessageDto?> SendMessageAsync(int userId, SendMessageDto dto)
        {
            var body = dto.Body.Trim();
            if (string.IsNullOrEmpty(body)) return null;

            if (dto.ThreadId is > 0)
                return await SendThreadMessageAsync(userId, dto.ThreadId.Value, body);

            var ad = await _context.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == dto.AdvertisementId);
            if (ad == null) return null;

            if (ad.UserId == userId)
            {
                var sellerThreads = await _context.MessageThreads.AsNoTracking()
                    .Where(t => t.AdvertisementId == dto.AdvertisementId && t.SellerUserId == userId)
                    .Select(t => t.Id)
                    .ToListAsync();
                if (sellerThreads.Count == 1)
                    return await SendThreadMessageAsync(userId, sellerThreads[0], body);
                return null;
            }

            var existingThread = await _context.MessageThreads.AsNoTracking()
                .FirstOrDefaultAsync(t => t.AdvertisementId == dto.AdvertisementId && t.BuyerUserId == userId);
            if (existingThread != null)
                return await SendThreadMessageAsync(userId, existingThread.Id, body);

            var added = await AddMessageToAdThreadAsync(dto.AdvertisementId, userId, ad.UserId, userId, body);
            if (added == null) return null;

            var (thread, msg) = added.Value;
            var preview = body.Length > 80 ? body[..80] + "…" : body;
            await NotifyUserAsync(ad.UserId, "message", "Yeni mesaj", preview, $"/mesajlar/{thread.Id}", "Yeni mesajınız var");
            await PushMessagesRealtimeAsync(ad.UserId, thread.Id);

            return new MessageDto
            {
                Id = msg.Id,
                ThreadId = thread.Id,
                SenderUserId = userId,
                IsMine = true,
                Body = body,
                CreatedTime = msg.CreatedTime,
            };
        }

        public async Task<MessageDto?> SendThreadMessageAsync(int userId, int threadId, string body)
        {
            var text = body.Trim();
            if (string.IsNullOrEmpty(text)) return null;

            var thread = await _context.MessageThreads
                .Include(t => t.Advertisement)
                .FirstOrDefaultAsync(t => t.Id == threadId);
            if (thread == null) return null;

            var sellerId = thread.SellerUserId > 0 ? thread.SellerUserId : thread.Advertisement.UserId;
            if (thread.SellerUserId != sellerId)
            {
                thread.SellerUserId = sellerId;
                await _context.SaveChangesAsync();
            }

            if (thread.BuyerUserId != userId && sellerId != userId)
                return null;

            var msg = new Message
            {
                ThreadId = thread.Id,
                SenderUserId = userId,
                Body = text,
                CreatedTime = DateTime.UtcNow,
            };
            _context.Messages.Add(msg);
            thread.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var recipientId = thread.BuyerUserId == userId ? sellerId : thread.BuyerUserId;
            var preview = text.Length > 80 ? text[..80] + "…" : text;
            await NotifyUserAsync(recipientId, "message", "Yeni mesaj", preview, $"/mesajlar/{thread.Id}", "Yeni mesajınız var");
            await PushMessagesRealtimeAsync(recipientId, thread.Id);

            return new MessageDto
            {
                Id = msg.Id,
                ThreadId = thread.Id,
                SenderUserId = userId,
                IsMine = true,
                Body = text,
                CreatedTime = msg.CreatedTime,
            };
        }

        public async Task<OfferDto?> CreateOfferAsync(int userId, CreateOfferDto dto)
        {
            var ad = await _context.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == dto.AdvertisementId);
            if (ad == null || ad.UserId == userId || dto.Amount <= 0) return null;

            var offer = new Offer
            {
                AdvertisementId = dto.AdvertisementId,
                BuyerUserId = userId,
                Amount = dto.Amount,
                Message = dto.Message?.Trim(),
                CreatedTime = DateTime.UtcNow,
            };
            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();

            var buyer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            var buyerName = buyer != null ? $"{buyer.FirstName} {buyer.LastName}".Trim() : "Alıcı";
            var threadBody = string.IsNullOrWhiteSpace(dto.Message)
                ? $"💰 {buyerName}: {dto.Amount:N0} TL teklif verdi."
                : $"💰 {buyerName}: {dto.Amount:N0} TL teklif — {dto.Message.Trim()}";

            var threadMsg = await AddMessageToAdThreadAsync(ad.Id, userId, ad.UserId, userId, threadBody);

            await NotifyUserAsync(
                ad.UserId,
                "offer",
                "Yeni teklif",
                $"{buyerName}: {dto.Amount:N0} TL",
                "/teklifler",
                "Yeni teklif");
            await _realtime.PushToUserAsync(ad.UserId, "offers", new { advertisementId = ad.Id, offerId = offer.Id });
            if (threadMsg != null)
                await PushMessagesRealtimeAsync(ad.UserId, threadMsg.Value.thread.Id);

            return new OfferDto
            {
                Id = offer.Id,
                AdvertisementId = offer.AdvertisementId,
                BuyerUserId = userId,
                BuyerName = buyer != null ? $"{buyer.FirstName} {buyer.LastName}".Trim() : null,
                Amount = offer.Amount,
                Message = offer.Message,
                Status = (int)offer.Status,
                CreatedTime = offer.CreatedTime,
            };
        }

        public async Task<List<OfferDto>> GetOffersForAdAsync(int userId, int advertisementId, bool isOwner)
        {
            var ad = await _context.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == advertisementId);
            if (ad == null) return new List<OfferDto>();
            if (isOwner)
            {
                if (ad.UserId != userId) return new List<OfferDto>();
            }

            var query = _context.Offers.AsNoTracking().Where(o => o.AdvertisementId == advertisementId);
            if (!isOwner)
                query = query.Where(o => o.BuyerUserId == userId);

            var offers = await query.OrderByDescending(o => o.CreatedTime).ToListAsync();
            var buyerIds = offers.Select(o => o.BuyerUserId).Distinct().ToList();
            var buyers = await _context.Users.AsNoTracking().Where(u => buyerIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

            return offers.Select(o =>
            {
                buyers.TryGetValue(o.BuyerUserId, out var b);
                return new OfferDto
                {
                    Id = o.Id,
                    AdvertisementId = o.AdvertisementId,
                    AdvertisementTitle = ad.Title,
                    BuyerUserId = o.BuyerUserId,
                    BuyerName = b != null ? $"{b.FirstName} {b.LastName}".Trim() : null,
                    Amount = o.Amount,
                    Message = o.Message,
                    Status = (int)o.Status,
                    CreatedTime = o.CreatedTime,
                };
            }).ToList();
        }

        public async Task<List<OfferDto>> GetIncomingOffersForSellerAsync(int userId)
        {
            var offers = await (
                from o in _context.Offers.AsNoTracking()
                join a in _context.Advertisements.AsNoTracking() on o.AdvertisementId equals a.Id
                where a.UserId == userId
                orderby o.CreatedTime descending
                select new
                {
                    o.Id,
                    o.AdvertisementId,
                    AdTitle = a.Title,
                    o.BuyerUserId,
                    o.Amount,
                    o.Message,
                    o.Status,
                    o.CreatedTime,
                })
                .Take(100)
                .ToListAsync();

            var buyerIds = offers.Select(o => o.BuyerUserId).Distinct().ToList();
            var buyers = await _context.Users.AsNoTracking()
                .Where(u => buyerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var adIds = offers.Select(o => o.AdvertisementId).Distinct().ToList();
            var threads = await _context.MessageThreads.AsNoTracking()
                .Where(t => adIds.Contains(t.AdvertisementId) && buyerIds.Contains(t.BuyerUserId))
                .ToListAsync();
            var threadLookup = threads.ToDictionary(t => (t.AdvertisementId, t.BuyerUserId), t => t.Id);

            return offers.Select(o =>
            {
                buyers.TryGetValue(o.BuyerUserId, out var b);
                threadLookup.TryGetValue((o.AdvertisementId, o.BuyerUserId), out var threadId);
                return new OfferDto
                {
                    Id = o.Id,
                    AdvertisementId = o.AdvertisementId,
                    AdvertisementTitle = o.AdTitle,
                    BuyerUserId = o.BuyerUserId,
                    BuyerName = b != null ? $"{b.FirstName} {b.LastName}".Trim() : null,
                    Amount = o.Amount,
                    Message = o.Message,
                    Status = (int)o.Status,
                    CreatedTime = o.CreatedTime,
                    MessageThreadId = threadId > 0 ? threadId : null,
                };
            }).ToList();
        }

        public async Task<OfferDto?> RespondToOfferAsync(int sellerUserId, int offerId, bool accept)
        {
            var row = await (
                from o in _context.Offers
                join a in _context.Advertisements on o.AdvertisementId equals a.Id
                where o.Id == offerId
                select new { Offer = o, Ad = a }
            ).FirstOrDefaultAsync();

            if (row == null || row.Ad.UserId != sellerUserId || row.Offer.Status != OfferStatus.Pending)
                return null;

            var offer = row.Offer;
            var ad = row.Ad;
            offer.Status = accept ? OfferStatus.Accepted : OfferStatus.Rejected;
            await _context.SaveChangesAsync();

            var threadBody = accept
                ? $"✅ {offer.Amount:N0} TL teklif kabul edildi."
                : $"❌ {offer.Amount:N0} TL teklif reddedildi.";

            var threadMsg = await AddMessageToAdThreadAsync(
                ad.Id, offer.BuyerUserId, ad.UserId, sellerUserId, threadBody);

            var title = accept ? "Teklifiniz kabul edildi" : "Teklifiniz reddedildi";
            var body = accept
                ? $"\"{ad.Title}\" için {offer.Amount:N0} TL teklifiniz kabul edildi."
                : $"\"{ad.Title}\" için {offer.Amount:N0} TL teklifiniz reddedildi.";

            await NotifyUserAsync(
                offer.BuyerUserId,
                accept ? "offer_accepted" : "offer_rejected",
                title,
                body,
                $"/ilan/{ad.Id}",
                title);

            await _realtime.PushToUserAsync(offer.BuyerUserId, "offers", new { advertisementId = ad.Id, offerId = offer.Id });
            if (threadMsg != null)
                await PushMessagesRealtimeAsync(offer.BuyerUserId, threadMsg.Value.thread.Id);

            var buyer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == offer.BuyerUserId);
            var threadId = await _context.MessageThreads.AsNoTracking()
                .Where(t => t.AdvertisementId == ad.Id && t.BuyerUserId == offer.BuyerUserId)
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync();

            return new OfferDto
            {
                Id = offer.Id,
                AdvertisementId = offer.AdvertisementId,
                AdvertisementTitle = ad.Title,
                BuyerUserId = offer.BuyerUserId,
                BuyerName = buyer != null ? $"{buyer.FirstName} {buyer.LastName}".Trim() : null,
                Amount = offer.Amount,
                Message = offer.Message,
                Status = (int)offer.Status,
                CreatedTime = offer.CreatedTime,
                MessageThreadId = threadId,
            };
        }

        public async Task<List<PublicBlogListDto>> GetPublishedBlogPostsAsync() =>
            await _context.BlogPosts.AsNoTracking()
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishedTime ?? b.CreatedTime)
                .Select(b => new PublicBlogListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Slug = b.Slug,
                    Summary = b.Summary,
                    PublishedTime = b.PublishedTime,
                    CreatedTime = b.CreatedTime,
                })
                .ToListAsync();

        public async Task<PublicBlogDetailDto?> GetPublishedBlogPostBySlugAsync(string slug)
        {
            var normalized = slug.Trim().ToLowerInvariant();
            return await _context.BlogPosts.AsNoTracking()
                .Where(b => b.IsPublished && b.Slug.ToLower() == normalized)
                .Select(b => new PublicBlogDetailDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Slug = b.Slug,
                    Summary = b.Summary,
                    Content = b.Content,
                    PublishedTime = b.PublishedTime,
                    CreatedTime = b.CreatedTime,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<PublicStaticPageListDto>> GetActiveStaticPagesAsync() =>
            await _context.StaticPages.AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Title)
                .Select(p => new PublicStaticPageListDto { Slug = p.Slug, Title = p.Title })
                .ToListAsync();

        public async Task<PublicStaticPageDto?> GetActiveStaticPageBySlugAsync(string slug)
        {
            var normalized = slug.Trim().ToLowerInvariant();
            return await _context.StaticPages.AsNoTracking()
                .Where(p => p.IsActive && p.Slug.ToLower() == normalized)
                .Select(p => new PublicStaticPageDto
                {
                    Slug = p.Slug,
                    Title = p.Title,
                    Content = p.Content,
                    UpdatedTime = p.UpdatedTime,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ReportListingAsync(int userId, ReportListingDto dto)
        {
            var spam = ContentModerationHelper.RejectReasonIfSpam($"{dto.Reason} {dto.Details}");
            if (spam != null) return false;

            var exists = await _context.ListingReports.AnyAsync(r =>
                r.AdvertisementId == dto.AdvertisementId && r.ReporterUserId == userId);
            if (exists) return false;

            _context.ListingReports.Add(new ListingReport
            {
                AdvertisementId = dto.AdvertisementId,
                ReporterUserId = userId,
                Reason = dto.Reason.Trim(),
                Details = dto.Details?.Trim(),
                CreatedTime = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SellerPublicProfileDto?> GetSellerPublicProfileAsync(int sellerUserId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == sellerUserId);
            if (user == null) return null;

            var ads = await _context.Advertisements.AsNoTracking()
                .Where(a => a.UserId == sellerUserId && a.IsActive && a.Status == AdvertisementStatus.Approved)
                .ToListAsync();

            var memberSince = ads.Count > 0
                ? ads.Min(a => a.CreatedTime)
                : DateTime.UtcNow;

            var sellerRating = await _context.SellerReviews.AsNoTracking()
                .Where(r => r.SellerUserId == sellerUserId && !r.IsHidden)
                .Select(r => r.Rating)
                .ToListAsync();

            return new SellerPublicProfileDto
            {
                UserId = sellerUserId,
                DisplayName = user.IsCorporateStore && !string.IsNullOrWhiteSpace(user.CompanyName)
                    ? user.CompanyName.Trim()
                    : $"{user.FirstName} {user.LastName}".Trim(),
                IsVerified = user.IsVerified,
                ActiveListingCount = ads.Count,
                TotalViews = ads.Sum(a => a.ViewCount),
                MemberSince = memberSince,
                ProfileImagePath = user.ProfileImagePath,
                StoreSlug = user.StoreSlug,
                CompanyName = user.CompanyName,
                StoreDescription = user.StoreDescription,
                StoreBannerPath = user.StoreBannerPath,
                IsCorporateStore = user.IsCorporateStore,
                CompletedOrderCount = await _context.MarketplaceOrders.CountAsync(o =>
                    o.SellerUserId == sellerUserId && o.Status == MarketplaceOrderStatus.Completed),
                AverageRating = sellerRating.Count == 0 ? 0 : Math.Round(sellerRating.Average(), 1),
                ReviewCount = sellerRating.Count,
            };
        }

        public async Task<SellerPublicProfileDto?> GetSellerPublicProfileBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;
            var normalized = slug.Trim().ToLowerInvariant();
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.StoreSlug != null && u.StoreSlug.ToLower() == normalized);
            if (user == null) return null;
            return await GetSellerPublicProfileAsync(user.Id);
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(int userId) =>
            await _context.Notifications.AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedTime)
                .Take(50)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Body = n.Body,
                    Link = n.Link,
                    IsRead = n.IsRead,
                    CreatedTime = n.CreatedTime,
                })
                .ToListAsync();

        public async Task<int> GetUnreadNotificationCountAsync(int userId) =>
            await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task MarkNotificationReadAsync(int userId, int id)
        {
            var n = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (n == null) return;
            n.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task LogSearchAsync(int? userId, int? categoryId, string? searchTerm)
        {
            _context.SearchLogs.Add(new SearchLog
            {
                UserId = userId,
                CategoryId = categoryId,
                SearchTerm = searchTerm?.Trim(),
                CreatedTime = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync();
        }

        public async Task<AnalyticsOverviewDto> GetAnalyticsOverviewAsync()
        {
            var since = DateTime.UtcNow.AddDays(-7);
            var logs = await _context.SearchLogs.AsNoTracking()
                .Where(l => l.CreatedTime >= since)
                .ToListAsync();

            var cats = await _context.Categories.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Name);

            var top = logs
                .GroupBy(l => l.CategoryId)
                .Select(g => new CategorySearchStatDto
                {
                    CategoryId = g.Key,
                    CategoryName = g.Key.HasValue && cats.TryGetValue(g.Key.Value, out var n) ? n : "Genel",
                    SearchCount = g.Count(),
                })
                .OrderByDescending(x => x.SearchCount)
                .Take(8)
                .ToList();

            return new AnalyticsOverviewDto
            {
                TopCategories = top,
                TotalSearchesLast7Days = logs.Count,
            };
        }

        public async Task<AdAnalyticsDto?> GetAdAnalyticsAsync(int userId, int advertisementId)
        {
            var ad = await _context.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == advertisementId);
            if (ad == null || ad.UserId != userId) return null;

            var offers = await _context.Offers.CountAsync(o => o.AdvertisementId == advertisementId);
            var threads = await _context.MessageThreads.CountAsync(t => t.AdvertisementId == advertisementId);

            return new AdAnalyticsDto
            {
                AdvertisementId = advertisementId,
                ViewCount = ad.ViewCount,
                OfferCount = offers,
                MessageThreadCount = threads,
            };
        }

        public async Task RecordViewAsync(int advertisementId)
        {
            var ad = await _context.Advertisements.FirstOrDefaultAsync(a => a.Id == advertisementId);
            if (ad == null) return;
            ad.ViewCount++;
            await _context.SaveChangesAsync();
        }

        public async Task<List<MapListingDto>> GetMapListingsAsync(AdvertisementFilterDto filter)
        {
            var query = _context.Advertisements.AsNoTracking()
                .Include(a => a.Category)
                .Where(a => a.IsActive && a.Status == AdvertisementStatus.Approved);

            if (filter.CategoryId.HasValue)
                query = query.Where(a => a.CategoryId == filter.CategoryId.Value);
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim();
                query = query.Where(a => a.Title.Contains(term) || a.Description.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                var city = filter.City.Trim();
                query = query.Where(a => a.ListingDetailsJson != null && a.ListingDetailsJson.Contains(city));
            }

            var items = await query.OrderByDescending(a => a.IsFeatured).Take(100).ToListAsync();
            var result = new List<MapListingDto>();
            foreach (var a in items)
            {
                var details = ListingDetailsHelper.Parse(a.ListingDetailsJson);
                var city = details?.City ?? "";
                if (string.IsNullOrWhiteSpace(city)) continue;
                if (!CityCoords.TryGetValue(city, out var coord))
                    coord = (39.0, 35.0);
                var paths = AdvertisementImagePathsHelper.Parse(a.ImagePath, a.ImagePathsJson);
                result.Add(new MapListingDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Price = details?.Price,
                    City = city,
                    District = details?.District,
                    Lat = coord.Lat + Random.Shared.NextDouble() * 0.02 - 0.01,
                    Lng = coord.Lng + Random.Shared.NextDouble() * 0.02 - 0.01,
                    ImagePath = paths.FirstOrDefault(),
                });
            }
            return result;
        }

        public async Task<SellerAnalyticsDto?> GetSellerAnalyticsAsync(int sellerUserId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == sellerUserId);
            if (user == null) return null;

            var ads = await _context.Advertisements.AsNoTracking()
                .Where(a => a.UserId == sellerUserId && a.IsActive && a.Status == AdvertisementStatus.Approved)
                .ToListAsync();

            var adIds = ads.Select(a => a.Id).ToList();
            var offerCounts = await _context.Offers.AsNoTracking()
                .Where(o => adIds.Contains(o.AdvertisementId))
                .GroupBy(o => o.AdvertisementId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();
            var threadCounts = await _context.MessageThreads.AsNoTracking()
                .Where(t => adIds.Contains(t.AdvertisementId))
                .GroupBy(t => t.AdvertisementId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            var offerMap = offerCounts.ToDictionary(x => x.Key, x => x.Count);
            var threadMap = threadCounts.ToDictionary(x => x.Key, x => x.Count);

            return new SellerAnalyticsDto
            {
                UserId = sellerUserId,
                DisplayName = $"{user.FirstName} {user.LastName}".Trim(),
                ActiveListingCount = ads.Count,
                TotalViews = ads.Sum(a => a.ViewCount),
                TotalOffers = offerCounts.Sum(x => x.Count),
                TotalMessageThreads = threadCounts.Sum(x => x.Count),
                TopAds = ads
                    .OrderByDescending(a => a.ViewCount)
                    .Take(10)
                    .Select(a => new SellerAdStatDto
                    {
                        AdvertisementId = a.Id,
                        Title = a.Title,
                        ViewCount = a.ViewCount,
                        OfferCount = offerMap.GetValueOrDefault(a.Id),
                        MessageThreadCount = threadMap.GetValueOrDefault(a.Id),
                    })
                    .ToList(),
            };
        }

        public async Task<List<AdPackageDto>> GetPackagesAsync() =>
            await _context.AdPackages.AsNoTracking()
                .Where(p => p.IsActive)
                .Select(p => new AdPackageDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Price = p.Price,
                    FeaturedDays = p.FeaturedDays,
                })
                .ToListAsync();

        public async Task SetFavoritePriceAlertAsync(int userId, int advertisementId, FavoritePriceAlertDto dto)
        {
            var fav = await _context.Favorites.FirstOrDefaultAsync(f =>
                f.UserId == userId && f.AdvertisementId == advertisementId);
            if (fav == null) return;
            fav.PriceAlertEnabled = dto.PriceAlertEnabled;
            fav.AlertPrice = dto.AlertPrice;
            if (dto.PriceAlertEnabled)
            {
                var ad = await _context.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == advertisementId);
                fav.LastKnownPrice = ad?.ListingPrice;
            }
            await _context.SaveChangesAsync();

            if (dto.PriceAlertEnabled)
            {
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user?.Email != null)
                {
                    await _email.SendAsync(user.Email, "İlanMarket — Fiyat alarmı",
                        $"<p>Fiyat alarmınız kaydedildi. Hedef: {dto.AlertPrice?.ToString() ?? "—"} TL</p>");
                }
            }
        }

        public async Task SavePushSubscriptionAsync(int userId, PushSubscriptionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Endpoint) || string.IsNullOrWhiteSpace(dto.P256dh) || string.IsNullOrWhiteSpace(dto.Auth))
                return;

            var existing = await _context.WebPushSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == dto.Endpoint);
            if (existing != null)
            {
                existing.P256dh = dto.P256dh;
                existing.Auth = dto.Auth;
            }
            else
            {
                _context.WebPushSubscriptions.Add(new WebPushSubscription
                {
                    UserId = userId,
                    Endpoint = dto.Endpoint.Trim(),
                    P256dh = dto.P256dh,
                    Auth = dto.Auth,
                    CreatedTime = DateTime.UtcNow,
                });
            }
            await _context.SaveChangesAsync();
        }

        private async Task NotifyUserAsync(int userId, string type, string title, string body, string? link, string emailSubject)
        {
            await SendUserNotificationAsync(userId, type, title, body, link, emailSubject);
        }

        public async Task SendUserNotificationAsync(int userId, string type, string title, string body, string? link, string emailSubject)
        {
            var notification = new AppNotification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                Link = link,
                CreatedTime = DateTime.UtcNow,
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _realtime.PushToUserAsync(userId, "notification", new
            {
                id = notification.Id,
                type,
                title,
                body,
                link,
            });

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

            await _webPush.SendToUserAsync(userId, title, body, link);

            if (string.IsNullOrWhiteSpace(user?.Email)) return;

            var frontend = _config["App:FrontendUrl"] ?? "http://localhost:3000";
            var url = string.IsNullOrWhiteSpace(link) ? frontend : $"{frontend}{link}";
            await _email.SendAsync(user.Email, $"İlanMarket — {emailSubject}",
                $"<p>{body}</p><p><a href=\"{url}\">Görüntüle</a></p>");
        }

        private Task PushMessagesRealtimeAsync(int userId, int threadId) =>
            _realtime.PushToUserAsync(userId, "messages", new { threadId });

        private async Task<(MessageThread thread, Message message)?> AddMessageToAdThreadAsync(
            int advertisementId,
            int buyerUserId,
            int sellerUserId,
            int senderUserId,
            string body)
        {
            var text = body.Trim();
            if (string.IsNullOrEmpty(text)) return null;

            var thread = await _context.MessageThreads
                .FirstOrDefaultAsync(t => t.AdvertisementId == advertisementId && t.BuyerUserId == buyerUserId);

            if (thread == null)
            {
                thread = new MessageThread
                {
                    AdvertisementId = advertisementId,
                    BuyerUserId = buyerUserId,
                    SellerUserId = sellerUserId,
                    CreatedTime = DateTime.UtcNow,
                };
                _context.MessageThreads.Add(thread);
                await _context.SaveChangesAsync();
            }

            var msg = new Message
            {
                ThreadId = thread.Id,
                SenderUserId = senderUserId,
                Body = text,
                CreatedTime = DateTime.UtcNow,
            };
            _context.Messages.Add(msg);
            thread.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (thread, msg);
        }

        public async Task<PackageExperimentDto> GetPackageExperimentAsync(string? variantHint)
        {
            var packages = await GetPackagesAsync();
            var variant = string.Equals(variantHint, "B", StringComparison.OrdinalIgnoreCase) ? "B" : "A";
            if (variant == "B")
                packages = packages.OrderByDescending(p => p.Price).ToList();
            return new PackageExperimentDto { Variant = variant, Packages = packages };
        }

        public async Task LogPackageExperimentAsync(int? userId, LogPackageExperimentDto dto)
        {
            _context.SearchLogs.Add(new SearchLog
            {
                UserId = userId,
                SearchTerm = $"ab:{dto.Variant}:{dto.Event}:{dto.PackageId}",
                CreatedTime = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync();
        }

        public async Task<VerificationRequestDto?> SubmitVerificationAsync(int userId, string documentType, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;
            var pending = await _context.VerificationRequests
                .AnyAsync(v => v.UserId == userId && v.Status == "pending");
            if (pending) return null;

            var row = new VerificationRequest
            {
                UserId = userId,
                DocumentType = string.IsNullOrWhiteSpace(documentType) ? "identity" : documentType.Trim(),
                FilePath = filePath.Trim(),
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
            };
            _context.VerificationRequests.Add(row);
            await _context.SaveChangesAsync();
            return await MapVerificationAsync(row.Id);
        }

        public async Task<List<VerificationRequestDto>> GetPendingVerificationsAsync()
        {
            var ids = await _context.VerificationRequests.AsNoTracking()
                .Where(v => v.Status == "pending")
                .OrderBy(v => v.CreatedAt)
                .Select(v => v.Id)
                .Take(100)
                .ToListAsync();
            var result = new List<VerificationRequestDto>();
            foreach (var id in ids)
            {
                var mapped = await MapVerificationAsync(id);
                if (mapped != null) result.Add(mapped);
            }
            return result;
        }

        public async Task<bool> ReviewVerificationAsync(int requestId, ReviewVerificationDto dto, int adminUserId)
        {
            var row = await _context.VerificationRequests.FirstOrDefaultAsync(v => v.Id == requestId);
            if (row == null || row.Status != "pending") return false;

            row.Status = dto.Approve ? "approved" : "rejected";
            row.AdminNote = dto.AdminNote?.Trim();
            row.ReviewedAt = DateTime.UtcNow;
            row.ReviewedByUserId = adminUserId;

            if (dto.Approve)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == row.UserId);
                if (user != null) user.IsVerified = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<VerificationRequestDto?> MapVerificationAsync(int id)
        {
            var row = await _context.VerificationRequests.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (row == null) return null;
            var email = await _context.Users.AsNoTracking()
                .Where(u => u.Id == row.UserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
            return new VerificationRequestDto
            {
                Id = row.Id,
                UserId = row.UserId,
                UserEmail = email,
                DocumentType = row.DocumentType,
                FilePath = row.FilePath,
                Status = row.Status,
                AdminNote = row.AdminNote,
                CreatedAt = row.CreatedAt,
            };
        }

    }
}
