using AdvertisementApp.Business.Helpers;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Helpers;
using AdvertisementApp.Common.Models;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.Dtos.Admin;
using AdvertisementApp.Dtos.Platform;
using AdvertisementApp.Dtos.AdminDtos;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AdvertisementApp.Business.Service
{
    public class AdminService : IAdminService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IAdvertisementService _ads;
        private readonly UserManager<AppUser> _users;
        private readonly RoleManager<IdentityRole<int>> _roles;
        private readonly IEmailService _email;
        private readonly IEmailTemplateService _emailTemplates;
        private readonly IConfiguration _config;
        private readonly IRealtimeNotifier _realtime;
        private readonly IGrowthService _growth;

        public AdminService(
            AdvertisementAppDbContext db,
            IAdvertisementService ads,
            UserManager<AppUser> users,
            RoleManager<IdentityRole<int>> roles,
            IEmailService email,
            IEmailTemplateService emailTemplates,
            IConfiguration config,
            IRealtimeNotifier realtime,
            IGrowthService growth)
        {
            _db = db;
            _ads = ads;
            _users = users;
            _roles = roles;
            _email = email;
            _emailTemplates = emailTemplates;
            _config = config;
            _realtime = realtime;
            _growth = growth;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var categoryDistribution = await _db.Advertisements
                .AsNoTracking()
                .GroupBy(a => new { a.CategoryId, a.Category.Name })
                .Select(g => new CategoryAdCountDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    Count = g.Count(),
                })
                .OrderByDescending(x => x.Count)
                .Take(12)
                .ToListAsync();

            return new AdminDashboardDto
            {
                TotalAds = await _db.Advertisements.CountAsync(),
                PendingAds = await _db.Advertisements.CountAsync(a => a.Status == AdvertisementStatus.Pending),
                TotalUsers = await _db.Users.CountAsync(),
                OpenReports = await _db.ListingReports.CountAsync(r => r.Status == "open"),
                FeaturedAds = await _db.Advertisements.CountAsync(a => a.IsFeatured && (a.FeaturedUntil == null || a.FeaturedUntil > now)),
                ExpiredAds = await _db.Advertisements.CountAsync(a => a.ExpiresAt != null && a.ExpiresAt < now && a.ArchivedAt == null),
                CategoryDistribution = categoryDistribution,
            };
        }

        public async Task<PagedResult<AdvertisementListDto>> GetAdsAsync(AdvertisementFilterDto filter)
        {
            filter.AdminMode = true;
            var result = await _ads.GetPagedAsync(filter);
            return result.Data ?? new PagedResult<AdvertisementListDto>();
        }

        public async Task<bool> ApproveAdAsync(int id, int actorId, string actorEmail)
        {
            var adMeta = await _db.Advertisements.AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new { a.UserId, a.Title })
                .FirstOrDefaultAsync();
            var r = await _ads.ApproveAsync(id);
            if (r.Success)
            {
                await AddAuditAsync(id, actorId, actorEmail, "approve", "İlan onaylandı");
                if (adMeta != null)
                {
                    await NotifyAdOwnerAsync(adMeta.UserId, "ad_approved", "İlanınız onaylandı",
                        $"\"{adMeta.Title}\" yayına alındı.", "/ilanlarim");
                    await _growth.NotifyFollowersOnNewListingAsync(adMeta.UserId, id, adMeta.Title);
                }
            }
            return r.Success;
        }

        public async Task<bool> RejectAdAsync(int id, int actorId, string actorEmail, string? reason)
        {
            var ad = await _db.Advertisements.FirstOrDefaultAsync(a => a.Id == id);
            if (ad == null) return false;
            var title = ad.Title;
            var ownerId = ad.UserId;
            ad.Status = AdvertisementStatus.Rejected;
            ad.IsActive = false;
            ad.RejectReason = reason;
            ad.UpdatedTime = DateTime.Now;
            await _db.SaveChangesAsync();
            await AddAuditAsync(id, actorId, actorEmail, "reject", reason ?? "Reddedildi");
            var body = string.IsNullOrWhiteSpace(reason)
                ? $"\"{title}\" ilanınız reddedildi."
                : $"\"{title}\" reddedildi: {reason}";
            await NotifyAdOwnerAsync(ownerId, "ad_rejected", "İlanınız reddedildi", body, "/ilanlarim");
            return true;
        }

        public async Task<bool> DeleteAdAsync(int id, int actorId, string actorEmail)
        {
            var ad = await _db.Advertisements.AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new { a.Title })
                .FirstOrDefaultAsync();
            if (ad == null) return false;

            var ok = await HardDeleteAdvertisementAsync(id);
            if (ok)
                await LogActivityAsync(actorId, "ad_delete", $"İlan silindi #{id}: {ad.Title} ({actorEmail})");
            return ok;
        }

        public async Task<bool> ExtendAdAsync(int id, int days, int actorId, string actorEmail)
        {
            var ad = await _db.Advertisements.FirstOrDefaultAsync(a => a.Id == id);
            if (ad == null) return false;
            var baseDate = ad.ExpiresAt > DateTime.UtcNow ? ad.ExpiresAt.Value : DateTime.UtcNow;
            ad.ExpiresAt = baseDate.AddDays(days);
            ad.ArchivedAt = null;
            ad.UpdatedTime = DateTime.Now;
            await _db.SaveChangesAsync();
            await AddAuditAsync(id, actorId, actorEmail, "extend", $"{days} gün uzatıldı");
            return true;
        }

        public async Task<bool> ArchiveAdAsync(int id, int actorId, string actorEmail)
        {
            var ad = await _db.Advertisements.FirstOrDefaultAsync(a => a.Id == id);
            if (ad == null) return false;
            ad.ArchivedAt = DateTime.UtcNow;
            ad.IsActive = false;
            ad.UpdatedTime = DateTime.Now;
            await _db.SaveChangesAsync();
            await AddAuditAsync(id, actorId, actorEmail, "archive", "Arşivlendi");
            return true;
        }

        public async Task<bool> SetFeaturedAsync(int id, bool featured, int? days, int actorId, string actorEmail)
        {
            var ad = await _db.Advertisements.FirstOrDefaultAsync(a => a.Id == id);
            if (ad == null) return false;
            ad.IsFeatured = featured;
            ad.FeaturedUntil = featured && days.HasValue ? DateTime.UtcNow.AddDays(days.Value) : null;
            ad.UpdatedTime = DateTime.Now;
            await _db.SaveChangesAsync();
            await AddAuditAsync(id, actorId, actorEmail, "featured", featured ? $"Öne çıkarıldı {days} gün" : "Öne çıkarma kaldırıldı");
            return true;
        }

        public async Task<int> BulkAdsAsync(AdminBulkActionDto dto, int actorId, string actorEmail)
        {
            if (dto.Ids == null || dto.Ids.Count == 0) return 0;

            var count = 0;
            foreach (var id in dto.Ids.Distinct())
            {
                var ok = dto.Action switch
                {
                    "approve" => await ApproveAdAsync(id, actorId, actorEmail),
                    "reject" => await RejectAdAsync(id, actorId, actorEmail, dto.Reason),
                    "delete" => await DeleteAdAsync(id, actorId, actorEmail),
                    "archive" => await ArchiveAdAsync(id, actorId, actorEmail),
                    _ => false,
                };
                if (ok) count++;
            }
            return count;
        }

        public async Task<List<AuditLogDto>> GetAdHistoryAsync(int adId) =>
            await _db.AdvertisementAuditLogs.AsNoTracking()
                .Where(l => l.AdvertisementId == adId)
                .OrderByDescending(l => l.CreatedTime)
                .Select(l => new AuditLogDto
                {
                    Id = l.Id,
                    ActorEmail = l.ActorEmail,
                    Action = l.Action,
                    Details = l.Details,
                    CreatedTime = l.CreatedTime,
                })
                .ToListAsync();

        public async Task<List<ReportAdminDto>> GetReportsAsync(string? status = "open")
        {
            var q = _db.ListingReports.AsNoTracking().Include(r => r.Advertisement).AsQueryable();
            if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);
            return await q.OrderByDescending(r => r.CreatedTime)
                .Select(r => new ReportAdminDto
                {
                    Id = r.Id,
                    AdvertisementId = r.AdvertisementId,
                    AdTitle = r.Advertisement.Title,
                    ReporterUserId = r.ReporterUserId,
                    Reason = r.Reason,
                    Details = r.Details,
                    Status = r.Status,
                    CreatedTime = r.CreatedTime,
                })
                .ToListAsync();
        }

        public async Task<bool> ResolveReportAsync(int reportId, ResolveReportDto dto, int actorId, string actorEmail)
        {
            var report = await _db.ListingReports.FirstOrDefaultAsync(r => r.Id == reportId);
            if (report == null) return false;
            report.Status = "resolved";
            report.AdminAction = dto.Note ?? dto.Action;
            report.ResolvedAt = DateTime.UtcNow;
            if (dto.RejectAd)
                await RejectAdAsync(report.AdvertisementId, actorId, actorEmail, dto.Note ?? "Şikayet sonrası red");
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<AdminUserDto>> GetUsersAsync(string? search, string? role)
        {
            var users = await _db.Users.AsNoTracking().OrderByDescending(u => u.Id).ToListAsync();
            var list = new List<AdminUserDto>();
            foreach (var u in users)
            {
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim().ToLowerInvariant();
                    if (!(u.Email?.ToLower().Contains(term) == true || u.FirstName.ToLower().Contains(term) || u.LastName.ToLower().Contains(term)))
                        continue;
                }
                var roles = await _users.GetRolesAsync(u);
                if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role)) continue;
                var adCount = await _db.Advertisements.CountAsync(a => a.UserId == u.Id);
                list.Add(new AdminUserDto
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Roles = roles,
                    IsVerified = u.IsVerified,
                    IsBanned = u.IsBanned,
                    IsFrozen = u.IsFrozen,
                    WarningCount = u.WarningCount,
                    PhoneVerified = u.PhoneVerified,
                    PhoneNumber = u.PhoneNumber,
                    AdCount = adCount,
                });
            }
            return list;
        }

        public async Task<AdminUserDetailDto?> GetUserDetailAsync(int userId)
        {
            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (u == null) return null;
            var roles = await _users.GetRolesAsync(u);
            var filter = new AdvertisementFilterDto { UserId = userId, AdminMode = true, PageSize = 20 };
            var ads = await GetAdsAsync(filter);
            var activity = await _db.ActivityLogs.AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedTime)
                .Take(30)
                .Select(a => new ActivityLogDto { Id = a.Id, Type = a.Type, Message = a.Message, CreatedTime = a.CreatedTime })
                .ToListAsync();
            return new AdminUserDetailDto
            {
                Id = u.Id,
                Email = u.Email ?? "",
                FirstName = u.FirstName,
                LastName = u.LastName,
                Roles = roles,
                IsVerified = u.IsVerified,
                IsBanned = u.IsBanned,
                IsFrozen = u.IsFrozen,
                WarningCount = u.WarningCount,
                PhoneVerified = u.PhoneVerified,
                PhoneNumber = u.PhoneNumber,
                BanReason = u.BanReason,
                FrozenUntil = u.FrozenUntil,
                AdCount = ads.TotalCount,
                RecentAds = ads.Items,
                Activity = activity,
            };
        }

        public async Task<bool> ModerateUserAsync(int userId, ModerateUserDto dto, int actorId, string actorEmail)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (u == null) return false;

            var actions = new List<string>();

            if (dto.IsBanned.HasValue)
            {
                u.IsBanned = dto.IsBanned.Value;
                u.BanReason = dto.IsBanned.Value ? dto.BanReason : null;
                actions.Add(dto.IsBanned.Value ? "ban" : "unban");
            }
            if (dto.IsFrozen.HasValue)
            {
                u.IsFrozen = dto.IsFrozen.Value;
                u.FrozenUntil = dto.IsFrozen.Value && dto.FrozenDays.HasValue
                    ? DateTime.UtcNow.AddDays(dto.FrozenDays.Value) : null;
                actions.Add(dto.IsFrozen.Value ? "freeze" : "unfreeze");
            }
            if (dto.PhoneVerified.HasValue)
            {
                u.PhoneVerified = dto.PhoneVerified.Value;
                actions.Add("phone_verified");
            }
            if (dto.IsVerified.HasValue)
            {
                u.IsVerified = dto.IsVerified.Value;
                actions.Add("verified");
            }
            if (dto.AddWarning)
            {
                u.WarningCount++;
                actions.Add("warning");
            }

            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                var identityUser = await _users.FindByIdAsync(userId.ToString());
                if (identityUser != null)
                {
                    var current = await _users.GetRolesAsync(identityUser);
                    await _users.RemoveFromRolesAsync(identityUser, current);
                    await _users.AddToRoleAsync(identityUser, dto.Role);
                    actions.Add($"role:{dto.Role}");
                }
            }

            var summary = actions.Count > 0 ? string.Join(", ", actions) : "güncelleme";
            await LogActivityAsync(userId, "moderation", $"Admin {actorEmail}: {summary}");
            return true;
        }

        public async Task LogActivityAsync(int? userId, string type, string message, string? ip = null)
        {
            _db.ActivityLogs.Add(new ActivityLog
            {
                UserId = userId,
                Type = type,
                Message = message,
                IpAddress = ip,
                CreatedTime = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
        }

        public async Task<List<AdminCategoryDto>> GetAdminCategoriesAsync()
        {
            var rows = await _db.Categories.AsNoTracking()
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .Select(c => new AdminCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ParentId = c.ParentId,
                    Slug = c.Slug,
                    SortOrder = c.SortOrder,
                    FieldSchemaJson = c.FieldSchemaJson,
                    IsActive = c.IsActive,
                    AdCount = c.Advertisements.Count,
                })
                .ToListAsync();

            var childCounts = await _db.Categories.AsNoTracking()
                .Where(c => c.ParentId != null)
                .GroupBy(c => c.ParentId!.Value)
                .Select(g => new { ParentId = g.Key, Count = g.Count() })
                .ToListAsync();

            var childMap = childCounts.ToDictionary(x => x.ParentId, x => x.Count);
            var nameById = rows.ToDictionary(c => c.Id, c => c.Name);

            foreach (var row in rows)
            {
                if (childMap.TryGetValue(row.Id, out var count))
                    row.ChildCount = count;
                if (row.ParentId.HasValue && nameById.TryGetValue(row.ParentId.Value, out var parentName))
                    row.ParentName = parentName;
            }
            return rows;
        }

        public async Task<bool> SaveCategoryAsync(AdminCategoryDto dto, int? id, int actorId, string actorEmail)
        {
            var name = dto.Name.Trim();
            await ValidateCategoryPlacement(name, dto.ParentId);

            Category entity;
            if (id.HasValue)
            {
                entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id) ?? new Category();
            }
            else
            {
                entity = new Category { CreatedTime = DateTime.Now };
                _db.Categories.Add(entity);
            }
            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description;
            entity.ParentId = dto.ParentId;
            entity.SortOrder = dto.SortOrder;
            entity.FieldSchemaJson = dto.FieldSchemaJson;
            entity.IsActive = dto.IsActive;
            entity.Slug = string.IsNullOrWhiteSpace(dto.Slug)
                ? SlugHelper.ToSlug(entity.Name)
                : SlugHelper.ToSlug(dto.Slug);
            entity.UpdatedTime = DateTime.Now;
            await _db.SaveChangesAsync();
            await LogActivityAsync(actorId, "category", $"Kategori kaydedildi: {entity.Name}");
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id, int actorId, string actorEmail)
        {
            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return false;
            _db.Categories.Remove(c);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<StaticPageDto>> GetStaticPagesAsync() =>
            await _db.StaticPages.AsNoTracking().OrderBy(p => p.Slug)
                .Select(p => new StaticPageDto { Id = p.Id, Slug = p.Slug, Title = p.Title, Content = p.Content, IsActive = p.IsActive })
                .ToListAsync();

        public async Task<bool> SaveStaticPageAsync(StaticPageDto dto)
        {
            StaticPage entity;
            if (dto.Id > 0)
                entity = await _db.StaticPages.FirstOrDefaultAsync(p => p.Id == dto.Id) ?? new StaticPage();
            else
            {
                entity = new StaticPage();
                _db.StaticPages.Add(entity);
            }
            entity.Slug = dto.Slug.Trim().ToLowerInvariant();
            entity.Title = dto.Title;
            entity.Content = dto.Content;
            entity.IsActive = dto.IsActive;
            entity.UpdatedTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<CityLocationDto>> GetCitiesAsync() =>
            await _db.CityLocations.AsNoTracking().OrderBy(c => c.SortOrder).ThenBy(c => c.City)
                .Select(c => new CityLocationDto { Id = c.Id, City = c.City, District = c.District, IsActive = c.IsActive, SortOrder = c.SortOrder })
                .ToListAsync();

        public async Task<bool> SaveCityAsync(CityLocationDto dto, int? id)
        {
            CityLocation entity;
            if (id.HasValue)
                entity = await _db.CityLocations.FirstOrDefaultAsync(c => c.Id == id) ?? new CityLocation();
            else
            {
                entity = new CityLocation();
                _db.CityLocations.Add(entity);
            }
            entity.City = dto.City.Trim();
            entity.District = dto.District?.Trim();
            entity.IsActive = dto.IsActive;
            entity.SortOrder = dto.SortOrder;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCityAsync(int id)
        {
            var c = await _db.CityLocations.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return false;
            _db.CityLocations.Remove(c);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<BlogPostDto>> GetBlogPostsAsync() =>
            await _db.BlogPosts.AsNoTracking().OrderByDescending(b => b.CreatedTime)
                .Select(b => new BlogPostDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Slug = b.Slug,
                    Summary = b.Summary,
                    Content = b.Content,
                    IsPublished = b.IsPublished,
                    CreatedTime = b.CreatedTime,
                })
                .ToListAsync();

        public async Task<bool> SaveBlogPostAsync(BlogPostDto dto, int? id)
        {
            BlogPost entity;
            if (id.HasValue)
                entity = await _db.BlogPosts.FirstOrDefaultAsync(b => b.Id == id) ?? new BlogPost();
            else
            {
                entity = new BlogPost { CreatedTime = DateTime.UtcNow };
                _db.BlogPosts.Add(entity);
            }
            entity.Title = dto.Title;
            entity.Slug = dto.Slug;
            entity.Summary = dto.Summary;
            entity.Content = dto.Content;
            entity.IsPublished = dto.IsPublished;
            if (dto.IsPublished && entity.PublishedTime == null)
                entity.PublishedTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBlogPostAsync(int id)
        {
            var b = await _db.BlogPosts.FirstOrDefaultAsync(x => x.Id == id);
            if (b == null) return false;
            _db.BlogPosts.Remove(b);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<AdminAdPackageDto>> GetAdPackagesAsync() =>
            await _db.AdPackages.AsNoTracking()
                .OrderBy(p => p.Price)
                .Select(p => new AdminAdPackageDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Price = p.Price,
                    FeaturedDays = p.FeaturedDays,
                    IsActive = p.IsActive,
                })
                .ToListAsync();

        public async Task<bool> SaveAdPackageAsync(AdminAdPackageDto dto, int? id)
        {
            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name)) return false;
            if (dto.Price < 0 || dto.FeaturedDays < 1) return false;

            AdPackage entity;
            if (id.HasValue)
            {
                entity = await _db.AdPackages.FirstOrDefaultAsync(p => p.Id == id.Value)
                    ?? new AdPackage();
                if (entity.Id == 0) return false;
            }
            else
            {
                entity = new AdPackage();
                _db.AdPackages.Add(entity);
            }

            entity.Code = dto.Code.Trim().ToLowerInvariant();
            entity.Name = dto.Name.Trim();
            entity.Price = dto.Price;
            entity.FeaturedDays = dto.FeaturedDays;
            entity.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAdPackageAsync(int id)
        {
            var pkg = await _db.AdPackages.FirstOrDefaultAsync(p => p.Id == id);
            if (pkg == null) return false;
            pkg.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<BulkImportResultDto> ImportAdvertisementsCsvAsync(string csv, int actorUserId)
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
                if (parts.Length < 5)
                {
                    result.Failed++;
                    result.Errors.Add($"Satır {i + 1}: yetersiz sütun");
                    continue;
                }

                if (!int.TryParse(parts[0].Trim(), out var userId) || !int.TryParse(parts[1].Trim(), out var categoryId))
                {
                    result.Failed++;
                    result.Errors.Add($"Satır {i + 1}: userId/categoryId geçersiz");
                    continue;
                }

                var title = parts[2].Trim();
                var description = parts[3].Trim();
                if (!decimal.TryParse(parts[4].Trim(), out var price))
                    price = 0;

                var city = parts.Length > 5 ? parts[5].Trim() : "";
                var details = System.Text.Json.JsonSerializer.Serialize(new { price, city });

                var ad = new Advertisement
                {
                    UserId = userId,
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

        private async Task AddAuditAsync(int adId, int actorId, string actorEmail, string action, string? details)
        {
            var exists = await _db.Advertisements.AnyAsync(a => a.Id == adId);
            if (!exists) return;

            _db.AdvertisementAuditLogs.Add(new AdvertisementAuditLog
            {
                AdvertisementId = adId,
                ActorUserId = actorId,
                ActorEmail = actorEmail,
                Action = action,
                Details = details,
                CreatedTime = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
        }

        private async Task NotifyAdOwnerAsync(int userId, string type, string title, string body, string? link)
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
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            await _realtime.PushToUserAsync(userId, "notification", new
            {
                id = notification.Id,
                type,
                title,
                body,
                link,
            });

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (string.IsNullOrWhiteSpace(user?.Email)) return;

            if (type == "ad_approved" && !string.IsNullOrWhiteSpace(link))
            {
                await _emailTemplates.SendAdApprovedAsync(user.Email, title, link);
                return;
            }

            var frontend = _config["App:FrontendUrl"] ?? "http://localhost:3000";
            var url = string.IsNullOrWhiteSpace(link) ? frontend : $"{frontend}{link}";
            await _email.SendAsync(user.Email, $"İlanMarket — {title}",
                $"<p>{body}</p><p><a href=\"{url}\">Görüntüle</a></p>");
        }

        private async Task<bool> HardDeleteAdvertisementAsync(int id)
        {
            var ad = await _db.Advertisements.FirstOrDefaultAsync(a => a.Id == id);
            if (ad == null) return false;

            var threadIds = await _db.MessageThreads
                .Where(t => t.AdvertisementId == id)
                .Select(t => t.Id)
                .ToListAsync();

            if (threadIds.Count > 0)
            {
                var messages = await _db.Messages.Where(m => threadIds.Contains(m.ThreadId)).ToListAsync();
                _db.Messages.RemoveRange(messages);
                var threads = await _db.MessageThreads.Where(t => t.AdvertisementId == id).ToListAsync();
                _db.MessageThreads.RemoveRange(threads);
            }

            _db.Offers.RemoveRange(await _db.Offers.Where(o => o.AdvertisementId == id).ToListAsync());
            _db.ListingReports.RemoveRange(await _db.ListingReports.Where(r => r.AdvertisementId == id).ToListAsync());
            _db.Favorites.RemoveRange(await _db.Favorites.Where(f => f.AdvertisementId == id).ToListAsync());
            _db.AdvertisementAuditLogs.RemoveRange(await _db.AdvertisementAuditLogs.Where(l => l.AdvertisementId == id).ToListAsync());

            var purchases = await _db.UserPurchases.Where(p => p.AdvertisementId == id).ToListAsync();
            foreach (var purchase in purchases)
                purchase.AdvertisementId = null;

            _db.Advertisements.Remove(ad);
            await _db.SaveChangesAsync();
            return true;
        }

        private static readonly HashSet<string> ReservedElektronikNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Telefon", "Cep Telefonu", "Bilgisayar", "Televizyon", "Tablet",
            "Oyun Konsolu", "Oyun & Konsol", "Beyaz Eşya", "Tv",
        };

        private async Task ValidateCategoryPlacement(string name, int? parentId)
        {
            if (parentId != null) return;

            if (!ReservedElektronikNames.Contains(name)) return;

            var elektronikId = await _db.Categories.AsNoTracking()
                .Where(c => c.ParentId == null && c.Name == "Elektronik")
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();

            if (elektronikId == null) return;

            throw new InvalidOperationException(
                $"\"{name}\" ana kategori olamaz — Elektronik altında zaten tanımlı. " +
                "Alt Kategoriler sekmesinden Elektronik ağacını kullanın.");
        }
    }
}
