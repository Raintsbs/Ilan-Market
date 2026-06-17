using AdvertisementApp.Business.Helpers;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Helpers;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Dtos.Platform;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.Business.Service
{
    public class GrowthService : IGrowthService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IPlatformService _platform;
        private readonly IEmailTemplateService _emailTemplates;

        public GrowthService(
            AdvertisementAppDbContext db,
            IPlatformService platform,
            IEmailTemplateService emailTemplates)
        {
            _db = db;
            _platform = platform;
            _emailTemplates = emailTemplates;
        }

        public async Task<VerificationRequestDto?> GetMyVerificationAsync(int userId)
        {
            var id = await _db.VerificationRequests.AsNoTracking()
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => v.Id)
                .FirstOrDefaultAsync();
            if (id == 0) return null;

            var row = await _db.VerificationRequests.AsNoTracking().FirstAsync(v => v.Id == id);
            var email = await _db.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.Email).FirstOrDefaultAsync();
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

        public async Task<List<ListingQuestionDto>> GetListingQuestionsAsync(int advertisementId)
        {
            var rows = await _db.ListingQuestions.AsNoTracking()
                .Where(q => q.AdvertisementId == advertisementId && !q.IsHidden)
                .OrderByDescending(q => q.CreatedTime)
                .Take(50)
                .ToListAsync();

            var adOwnerId = await _db.Advertisements.AsNoTracking()
                .Where(a => a.Id == advertisementId)
                .Select(a => a.UserId)
                .FirstOrDefaultAsync();

            var userIds = rows.Select(r => r.UserId).Distinct().ToList();
            var users = await _db.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim());

            return rows.Select(r => new ListingQuestionDto
            {
                Id = r.Id,
                AdvertisementId = r.AdvertisementId,
                UserId = r.UserId,
                UserName = users.GetValueOrDefault(r.UserId, "Kullanıcı"),
                Question = r.Question,
                Answer = r.Answer,
                CreatedTime = r.CreatedTime,
                AnsweredTime = r.AnsweredTime,
                IsOwnerAnswer = r.AnsweredByUserId == adOwnerId,
            }).ToList();
        }

        public async Task<ListingQuestionDto?> AskQuestionAsync(int userId, CreateListingQuestionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Question) || dto.Question.Length > 1000) return null;
            var ad = await _db.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == dto.AdvertisementId);
            if (ad == null || ad.UserId == userId) return null;

            var row = new ListingQuestion
            {
                AdvertisementId = dto.AdvertisementId,
                UserId = userId,
                Question = dto.Question.Trim(),
                CreatedTime = DateTime.UtcNow,
            };
            _db.ListingQuestions.Add(row);
            await _db.SaveChangesAsync();

            await _platform.SendUserNotificationAsync(
                ad.UserId, "listing_question", "Yeni soru",
                $"İlanınıza soru geldi: {dto.Question.Trim()[..Math.Min(80, dto.Question.Trim().Length)]}",
                $"/ilan/{dto.AdvertisementId}", "İlanınıza yeni soru");

            var seller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == ad.UserId);
            if (!string.IsNullOrWhiteSpace(seller?.Email))
            {
                await _emailTemplates.SendListingQuestionEmailAsync(
                    seller.Email,
                    ad.Title,
                    dto.Question.Trim(),
                    $"/ilan/{dto.AdvertisementId}");
            }

            return (await GetListingQuestionsAsync(dto.AdvertisementId)).FirstOrDefault(q => q.Id == row.Id);
        }

        public async Task<ListingQuestionDto?> AnswerQuestionAsync(int userId, int questionId, AnswerListingQuestionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Answer)) return null;
            var q = await _db.ListingQuestions.FirstOrDefaultAsync(x => x.Id == questionId);
            if (q == null) return null;

            var ad = await _db.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == q.AdvertisementId);
            if (ad == null || ad.UserId != userId) return null;

            q.Answer = dto.Answer.Trim();
            q.AnsweredByUserId = userId;
            q.AnsweredTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _platform.SendUserNotificationAsync(
                q.UserId, "question_answered", "Sorunuz yanıtlandı",
                $"\"{ad.Title}\" ilanındaki sorunuza yanıt verildi.",
                $"/ilan/{q.AdvertisementId}", "Sorunuza yanıt");

            var asker = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == q.UserId);
            if (!string.IsNullOrWhiteSpace(asker?.Email))
            {
                await _emailTemplates.SendQuestionAnsweredEmailAsync(
                    asker.Email,
                    ad.Title,
                    dto.Answer.Trim(),
                    $"/ilan/{q.AdvertisementId}");
            }

            return (await GetListingQuestionsAsync(q.AdvertisementId)).FirstOrDefault(x => x.Id == questionId);
        }

        public async Task<bool> FollowSellerAsync(int followerUserId, int sellerUserId)
        {
            if (followerUserId == sellerUserId) return false;
            if (!await _db.Users.AnyAsync(u => u.Id == sellerUserId)) return false;
            if (await _db.SellerFollows.AnyAsync(f => f.FollowerUserId == followerUserId && f.SellerUserId == sellerUserId))
                return true;

            _db.SellerFollows.Add(new SellerFollow
            {
                FollowerUserId = followerUserId,
                SellerUserId = sellerUserId,
                CreatedTime = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowSellerAsync(int followerUserId, int sellerUserId)
        {
            var row = await _db.SellerFollows.FirstOrDefaultAsync(f =>
                f.FollowerUserId == followerUserId && f.SellerUserId == sellerUserId);
            if (row == null) return false;
            _db.SellerFollows.Remove(row);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingSellerAsync(int followerUserId, int sellerUserId) =>
            await _db.SellerFollows.AsNoTracking()
                .AnyAsync(f => f.FollowerUserId == followerUserId && f.SellerUserId == sellerUserId);

        public async Task<List<SellerFollowDto>> GetFollowedSellersAsync(int followerUserId)
        {
            var follows = await _db.SellerFollows.AsNoTracking()
                .Where(f => f.FollowerUserId == followerUserId)
                .OrderByDescending(f => f.CreatedTime)
                .ToListAsync();

            var result = new List<SellerFollowDto>();
            foreach (var f in follows)
            {
                var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == f.SellerUserId);
                if (user == null) continue;
                var count = await _db.Advertisements.CountAsync(a =>
                    a.UserId == f.SellerUserId && a.IsActive && a.Status == AdvertisementStatus.Approved);
                result.Add(new SellerFollowDto
                {
                    SellerUserId = f.SellerUserId,
                    DisplayName = user.IsCorporateStore && !string.IsNullOrWhiteSpace(user.CompanyName)
                        ? user.CompanyName.Trim()
                        : $"{user.FirstName} {user.LastName}".Trim(),
                    StoreSlug = user.StoreSlug,
                    ActiveListingCount = count,
                    FollowedAt = f.CreatedTime,
                });
            }
            return result;
        }

        public async Task<bool> UpdateStoreSettingsAsync(int userId, UpdateStoreSettingsDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.StoreSlug))
            {
                var slug = dto.StoreSlug.Trim().ToLowerInvariant();
                if (!System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[a-z0-9-]{3,64}$")) return false;
                var taken = await _db.Users.AnyAsync(u => u.Id != userId && u.StoreSlug != null && u.StoreSlug.ToLower() == slug);
                if (taken) return false;
                user.StoreSlug = slug;
            }

            if (dto.CompanyName != null) user.CompanyName = dto.CompanyName.Trim();
            if (dto.StoreDescription != null) user.StoreDescription = dto.StoreDescription.Trim();
            if (dto.IsCorporateStore.HasValue) user.IsCorporateStore = dto.IsCorporateStore.Value;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<UpdateStoreSettingsDto?> GetStoreSettingsAsync(int userId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;
            return new UpdateStoreSettingsDto
            {
                StoreSlug = user.StoreSlug,
                CompanyName = user.CompanyName,
                StoreDescription = user.StoreDescription,
                IsCorporateStore = user.IsCorporateStore,
            };
        }

        public async Task<SellerEarningsDto> GetSellerEarningsAsync(int sellerUserId)
        {
            var orders = await _db.MarketplaceOrders.AsNoTracking()
                .Where(o => o.SellerUserId == sellerUserId && o.Status == MarketplaceOrderStatus.Completed)
                .ToListAsync();

            var paid = orders.Where(o => o.SellerPaidOutAt != null).Sum(o => o.Amount);
            var pending = orders.Where(o => o.SellerPaidOutAt == null).Sum(o => o.Amount);

            return new SellerEarningsDto
            {
                TotalCompletedAmount = orders.Sum(o => o.Amount),
                PendingPayoutAmount = pending,
                PaidOutAmount = paid,
                CompletedOrderCount = orders.Count,
            };
        }

        public async Task<ReferralStatsDto?> GetReferralStatsAsync(int userId, string siteBaseUrl)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            if (string.IsNullOrWhiteSpace(user.ReferralCode))
            {
                user.ReferralCode = await GenerateUniqueReferralCodeAsync();
                await _db.SaveChangesAsync();
            }

            var count = await _db.Users.CountAsync(u => u.ReferredByUserId == userId);
            var baseUrl = siteBaseUrl.TrimEnd('/');
            return new ReferralStatsDto
            {
                ReferralCode = user.ReferralCode,
                ReferredUserCount = count,
                ShareUrl = $"{baseUrl}/kayit?ref={user.ReferralCode}",
            };
        }

        private async Task<string> GenerateUniqueReferralCodeAsync()
        {
            for (var attempt = 0; attempt < 20; attempt++)
            {
                var code = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("+", "").Replace("/", "").Replace("=", "")[..8]
                    .ToUpperInvariant();
                if (!await _db.Users.AnyAsync(u => u.ReferralCode == code))
                    return code;
            }
            return Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        }

        public async Task<CouponValidationResultDto> ValidateCouponAsync(ValidateCouponDto dto)
        {
            var code = dto.Code.Trim().ToUpperInvariant();
            var coupon = await _db.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
            if (coupon == null)
                return new CouponValidationResultDto { Valid = false, Message = "Kupon geçersiz." };
            if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < DateTime.UtcNow)
                return new CouponValidationResultDto { Valid = false, Message = "Kupon süresi dolmuş." };
            if (coupon.UsedCount >= coupon.MaxUses)
                return new CouponValidationResultDto { Valid = false, Message = "Kupon kullanım limiti dolmuş." };

            decimal discount = coupon.DiscountAmount;
            if (coupon.DiscountPercent.HasValue)
                discount = Math.Round(dto.OrderAmount * coupon.DiscountPercent.Value / 100m, 2);
            if (discount > dto.OrderAmount) discount = dto.OrderAmount;

            return new CouponValidationResultDto
            {
                Valid = true,
                DiscountAmount = discount,
                Message = $"{discount:N0} TL indirim uygulanır.",
            };
        }

        public async Task<List<CouponDto>> GetCouponsAdminAsync() =>
            await _db.Coupons.AsNoTracking()
                .OrderByDescending(c => c.CreatedTime)
                .Select(c => new CouponDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Description = c.Description,
                    DiscountAmount = c.DiscountAmount,
                    DiscountPercent = c.DiscountPercent,
                    MaxUses = c.MaxUses,
                    UsedCount = c.UsedCount,
                    ExpiresAt = c.ExpiresAt,
                    IsActive = c.IsActive,
                }).ToListAsync();

        public async Task<bool> SaveCouponAsync(CouponDto dto, int? id)
        {
            if (string.IsNullOrWhiteSpace(dto.Code)) return false;
            var code = dto.Code.Trim().ToUpperInvariant();
            Coupon entity;
            if (id.HasValue)
            {
                entity = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == id) ?? new Coupon();
                if (entity.Id == 0) return false;
            }
            else
            {
                if (await _db.Coupons.AnyAsync(c => c.Code == code)) return false;
                entity = new Coupon { CreatedTime = DateTime.UtcNow };
                _db.Coupons.Add(entity);
            }

            entity.Code = code;
            entity.Description = dto.Description?.Trim();
            entity.DiscountAmount = dto.DiscountAmount;
            entity.DiscountPercent = dto.DiscountPercent;
            entity.MaxUses = dto.MaxUses > 0 ? dto.MaxUses : 100;
            entity.ExpiresAt = dto.ExpiresAt;
            entity.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCouponAsync(int id)
        {
            var c = await _db.Coupons.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return false;
            c.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RedeemCouponAsync(string code)
        {
            var normalized = code.Trim().ToUpperInvariant();
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == normalized && c.IsActive);
            if (coupon == null || coupon.UsedCount >= coupon.MaxUses) return false;
            coupon.UsedCount++;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<BulkImportResultDto> ImportSellerAdvertisementsCsvAsync(string csv, int sellerUserId)
        {
            var result = new BulkImportResultDto();
            var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (lines.Length < 2)
            {
                result.Errors.Add("CSV en az başlık + 1 satır içermeli.");
                return result;
            }

            for (var i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length < 4)
                {
                    result.Failed++;
                    result.Errors.Add($"Satır {i + 1}: categoryId,title,description,price gerekli");
                    continue;
                }

                if (!int.TryParse(parts[0].Trim(), out var categoryId))
                {
                    result.Failed++;
                    result.Errors.Add($"Satır {i + 1}: categoryId geçersiz");
                    continue;
                }

                var title = parts[1].Trim();
                var description = parts[2].Trim();
                if (!decimal.TryParse(parts[3].Trim(), out var price)) price = 0;
                var city = parts.Length > 4 ? parts[4].Trim() : "";
                var details = System.Text.Json.JsonSerializer.Serialize(new { price, city });

                var ad = new Advertisement
                {
                    UserId = sellerUserId,
                    CategoryId = categoryId,
                    Title = title,
                    Description = description.Length > 500 ? description[..500] : description,
                    Content = description,
                    ListingDetailsJson = details,
                    Status = AdvertisementStatus.Pending,
                    IsActive = false,
                    CreatedTime = DateTime.UtcNow,
                };
                ListingIndexSync.Apply(ad);
                _db.Advertisements.Add(ad);
                try
                {
                    await _db.SaveChangesAsync();
                    result.Created++;
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"Satır {i + 1}: {ex.Message}");
                }
            }

            return result;
        }

        public async Task NotifyFollowersOnNewListingAsync(int sellerUserId, int advertisementId, string title)
        {
            var seller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == sellerUserId);
            var sellerName = seller != null
                ? (seller.IsCorporateStore && !string.IsNullOrWhiteSpace(seller.CompanyName)
                    ? seller.CompanyName.Trim()
                    : $"{seller.FirstName} {seller.LastName}".Trim())
                : "Satıcı";

            var followerIds = await _db.SellerFollows.AsNoTracking()
                .Where(f => f.SellerUserId == sellerUserId)
                .Select(f => f.FollowerUserId)
                .ToListAsync();

            foreach (var followerId in followerIds)
            {
                await _platform.SendUserNotificationAsync(
                    followerId,
                    "seller_new_listing",
                    "Takip ettiğiniz satıcı yeni ilan verdi",
                    title,
                    $"/ilan/{advertisementId}",
                    $"Yeni ilan: {title}");

                var follower = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == followerId);
                if (!string.IsNullOrWhiteSpace(follower?.Email))
                {
                    await _emailTemplates.SendFollowedSellerListingEmailAsync(
                        follower.Email,
                        sellerName,
                        title,
                        $"/ilan/{advertisementId}");
                }
            }
        }
    }
}
