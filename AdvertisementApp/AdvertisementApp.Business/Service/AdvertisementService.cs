using AdvertisementApp.Business.Helpers;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Helpers;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Common.Result;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.DataAccess.Interface;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Entities;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AdvertisementApp.Business.Service
{
    public class AdvertisementService : GenericService<Advertisement, AdvertisementListDto, AdvertisementCreateDto, AdvertisementUpdateDto>, IAdvertisementService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly AdvertisementAppDbContext _db;
        private readonly ICategoryService _categories;
        private readonly IFullTextSearchService _fullText;
        private readonly IAppCacheService _cache;

        public AdvertisementService(
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<AdvertisementCreateDto> createValidator,
            IValidator<AdvertisementUpdateDto> updateValidator,
            AdvertisementAppDbContext db,
            ICategoryService categories,
            IFullTextSearchService fullText,
            IAppCacheService cache) : base(uow, mapper, createValidator, updateValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _db = db;
            _categories = categories;
            _fullText = fullText;
            _cache = cache;
        }

        public override async Task<IDataResult<List<AdvertisementListDto>>> GetAllAsync()
        {
            var items = await GetPagedAsync(new AdvertisementFilterDto { Page = 1, PageSize = 1000 });
            return items.Success && items.Data != null
                ? DataResult<List<AdvertisementListDto>>.Ok(items.Data.Items)
                : DataResult<List<AdvertisementListDto>>.Fail(items.Message);
        }

        public override async Task<IDataResult<AdvertisementListDto>> GetByIdAsync(int id) =>
            await GetPublicByIdInternalAsync(id, viewerUserId: null, allowUnpublished: true);

        public Task<IDataResult<AdvertisementListDto>> GetPublicByIdAsync(int id, int? viewerUserId = null) =>
            GetPublicByIdInternalAsync(id, viewerUserId, allowUnpublished: false);

        private async Task<IDataResult<AdvertisementListDto>> GetPublicByIdInternalAsync(
            int id,
            int? viewerUserId,
            bool allowUnpublished)
        {
            var query = _uow.GetRepository<Advertisement>().GetQuery()
                .Include(a => a.Category)
                .AsNoTracking();

            var entity = await query.FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null)
                return DataResult<AdvertisementListDto>.Fail("İlan bulunamadı.");

            var isOwner = viewerUserId.HasValue && entity.UserId == viewerUserId.Value;
            if (!allowUnpublished && !isOwner &&
                (entity.Status != AdvertisementStatus.Approved || !entity.IsActive || entity.ArchivedAt != null))
                return DataResult<AdvertisementListDto>.Fail("İlan bulunamadı.");

            var verified = await _db.Users.AsNoTracking()
                .Where(u => u.Id == entity.UserId)
                .Select(u => u.IsVerified)
                .FirstOrDefaultAsync();
            var dto = MapToListDto(entity, verified);
            await EnrichDetailAsync(dto, entity);
            await EnrichRatingsAsync(new List<AdvertisementListDto> { dto });
            return DataResult<AdvertisementListDto>.Ok(dto);
        }

        public async Task<IDataResult<PagedResult<AdvertisementListDto>>> GetPagedAsync(AdvertisementFilterDto filter)
        {
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize is < 1 or > 100 ? 10 : filter.PageSize;

            var query = _uow.GetRepository<Advertisement>().GetQuery()
                .Include(a => a.Category)
                .AsNoTracking();

            if (filter.SellerUserId.HasValue)
            {
                query = query.Where(a =>
                    a.UserId == filter.SellerUserId.Value
                    && a.IsActive
                    && a.Status == AdvertisementStatus.Approved);
            }
            else if (filter.UserId.HasValue)
                query = query.Where(a => a.UserId == filter.UserId.Value);

            if (filter.CategoryId.HasValue)
            {
                var categoryIds = await _categories.GetDescendantCategoryIdsAsync(filter.CategoryId.Value);
                query = query.Where(a => categoryIds.Contains(a.CategoryId));
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(a => a.Status == filter.Status.Value);
                // İlanlarım "Yayında" sekmesi = ana sayfada görünen ilanlar
                if (filter.UserId.HasValue && filter.Status.Value == AdvertisementStatus.Approved)
                    query = query.Where(a => a.IsActive && a.ArchivedAt == null);
            }

            if (filter.ListingId.HasValue)
                query = query.Where(a => a.Id == filter.ListingId.Value);
            else if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim();
                if (int.TryParse(term, out var numericId))
                {
                    query = query.Where(a =>
                        a.Id == numericId ||
                        a.Title.Contains(term) ||
                        a.Description.Contains(term) ||
                        a.Content.Contains(term));
                }
                else if (_fullText.IsAvailable)
                {
                    var ftIds = await _fullText.SearchAdvertisementIdsAsync(term);
                    if (ftIds.Count > 0)
                        query = query.Where(a => ftIds.Contains(a.Id));
                    else
                        query = query.Where(a =>
                            a.Title.Contains(term) ||
                            a.Description.Contains(term) ||
                            a.Content.Contains(term));
                }
                else
                {
                    query = query.Where(a =>
                        a.Title.Contains(term) ||
                        a.Description.Contains(term) ||
                        a.Content.Contains(term));
                }
            }

            if (filter.CreatedAfter.HasValue)
                query = query.Where(a => a.CreatedTime > filter.CreatedAfter.Value);

            if (!filter.UserId.HasValue && !filter.SellerUserId.HasValue && !filter.AdminMode)
                query = query.Where(a => a.IsActive && a.Status == AdvertisementStatus.Approved);

            if (filter.AdminMode && filter.ExpiredOnly)
                query = query.Where(a => a.ExpiresAt != null && a.ExpiresAt < DateTime.UtcNow && a.ArchivedAt == null);

            if (filter.AdminMode && filter.ArchivedOnly)
                query = query.Where(a => a.ArchivedAt != null);

            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                var city = filter.City.Trim();
                query = query.Where(a => a.ListingDetailsJson != null && a.ListingDetailsJson.Contains(city));
            }

            if (filter.FeaturedOnly == true)
                query = query.Where(a => a.IsFeatured && (a.FeaturedUntil == null || a.FeaturedUntil > DateTime.UtcNow));

            var needsMemoryFilter = !string.IsNullOrWhiteSpace(filter.Brand)
                || !string.IsNullOrWhiteSpace(filter.Model);

            if (filter.MinPrice.HasValue)
                query = query.Where(a => a.ListingPrice >= filter.MinPrice);
            if (filter.MaxPrice.HasValue)
                query = query.Where(a => a.ListingPrice <= filter.MaxPrice);
            if (filter.MinYear.HasValue)
                query = query.Where(a => a.ListingYear >= filter.MinYear);
            if (filter.MaxYear.HasValue)
                query = query.Where(a => a.ListingYear <= filter.MaxYear);
            if (filter.MinMileage.HasValue)
                query = query.Where(a => a.ListingMileageKm >= filter.MinMileage);
            if (filter.MaxMileage.HasValue)
                query = query.Where(a => a.ListingMileageKm <= filter.MaxMileage);

            if (needsMemoryFilter)
            {
                query = ApplyDetailFiltersQuery(query, filter);
                var list = await query.ToListAsync();
                list = ApplyBrandModelFilters(list, filter);
                var totalMem = list.Count;
                var entitiesMem = list
                    .OrderByDescending(a => a.IsFeatured && (a.FeaturedUntil == null || a.FeaturedUntil > DateTime.UtcNow))
                    .ThenByDescending(a => a.LastBumpedAt ?? a.CreatedTime)
                    .ThenByDescending(a => a.CreatedTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                return await FinishPagedAsync(entitiesMem, totalMem, page, pageSize);
            }

            var total = await query.CountAsync();
            var entities = await query
                .OrderByDescending(a => a.IsFeatured && (a.FeaturedUntil == null || a.FeaturedUntil > DateTime.UtcNow))
                .ThenByDescending(a => a.LastBumpedAt ?? a.CreatedTime)
                .ThenByDescending(a => a.CreatedTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return await FinishPagedAsync(entities, total, page, pageSize);
        }

        private async Task<IDataResult<PagedResult<AdvertisementListDto>>> FinishPagedAsync(
            List<Advertisement> entities,
            int total,
            int page,
            int pageSize,
            Dictionary<int, bool>? verifiedMap = null)
        {
            if (verifiedMap == null && entities.Count > 0)
            {
                var sellerIds = entities.Select(e => e.UserId).Distinct().ToList();
                verifiedMap = await _db.Users.AsNoTracking()
                    .Where(u => sellerIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.IsVerified);
            }

            verifiedMap ??= new Dictionary<int, bool>();

            var items = entities.Select(e => MapToListDto(e, verifiedMap.GetValueOrDefault(e.UserId))).ToList();
            await EnrichRatingsAsync(items);

            return DataResult<PagedResult<AdvertisementListDto>>.Ok(new PagedResult<AdvertisementListDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Items = items,
            });
        }

        private async Task EnrichRatingsAsync(List<AdvertisementListDto> items)
        {
            if (items.Count == 0) return;

            var sellerIds = items.Select(i => i.UserId).Distinct().ToList();
            var adIds = items.Select(i => i.Id).ToList();

            var sellerRatings = await _db.SellerReviews.AsNoTracking()
                .Where(r => sellerIds.Contains(r.SellerUserId) && !r.IsHidden)
                .GroupBy(r => r.SellerUserId)
                .Select(g => new { SellerId = g.Key, Avg = g.Average(r => r.Rating), Count = g.Count() })
                .ToListAsync();

            var adRatings = await _db.AdvertisementReviews.AsNoTracking()
                .Where(r => adIds.Contains(r.AdvertisementId) && !r.IsHidden)
                .GroupBy(r => r.AdvertisementId)
                .Select(g => new { AdId = g.Key, Avg = g.Average(r => r.Rating), Count = g.Count() })
                .ToListAsync();

            var sellerMap = sellerRatings.ToDictionary(x => x.SellerId);
            var adMap = adRatings.ToDictionary(x => x.AdId);

            foreach (var item in items)
            {
                if (sellerMap.TryGetValue(item.UserId, out var sr))
                {
                    item.SellerAverageRating = Math.Round(sr.Avg, 1);
                    item.SellerReviewCount = sr.Count;
                }
                if (adMap.TryGetValue(item.Id, out var ar))
                {
                    item.AverageRating = Math.Round(ar.Avg, 1);
                    item.ReviewCount = ar.Count;
                }
            }
        }

        private AdvertisementListDto MapToListDto(Advertisement entity, bool sellerVerified = false)
        {
            var dto = _mapper.Map<AdvertisementListDto>(entity);
            dto.CategoryName = entity.Category?.Name ?? string.Empty;
            var paths = AdvertisementImagePathsHelper.Parse(entity.ImagePath, entity.ImagePathsJson);
            dto.ImagePaths = paths;
            dto.ImagePath = paths.FirstOrDefault();
            dto.ListingDetails = ListingDetailsHelper.Parse(entity.ListingDetailsJson);
            dto.ViewCount = entity.ViewCount;
            dto.IsFeatured = entity.IsFeatured && (entity.FeaturedUntil == null || entity.FeaturedUntil > DateTime.UtcNow);
            dto.SellerIsVerified = sellerVerified;
            dto.RejectReason = entity.RejectReason;
            dto.VideoPath = entity.VideoPath;
            dto.PanoramaPath = entity.PanoramaPath;
            dto.ListingType = entity.ListingType;
            if (!string.IsNullOrWhiteSpace(entity.TramerResultJson))
            {
                try
                {
                    dto.TramerResult = JsonSerializer.Deserialize<TramerQueryResult>(entity.TramerResultJson);
                }
                catch { /* ignore */ }
            }
            return dto;
        }

        private async Task EnrichDetailAsync(AdvertisementListDto dto, Advertisement entity)
        {
            var auction = await _db.Auctions.AsNoTracking().FirstOrDefaultAsync(a => a.AdvertisementId == entity.Id);
            if (auction == null) return;

            var bids = await _db.AuctionBids.AsNoTracking()
                .Where(b => b.AuctionId == auction.Id)
                .OrderByDescending(b => b.CreatedTime)
                .Take(10)
                .ToListAsync();

            dto.Auction = new AuctionDto
            {
                Id = auction.Id,
                AdvertisementId = auction.AdvertisementId,
                StartsAt = auction.StartsAt,
                EndsAt = auction.EndsAt,
                StartingBid = auction.StartingBid,
                MinIncrement = auction.MinIncrement,
                CurrentBid = auction.CurrentBid,
                HighBidderUserId = auction.HighBidderUserId,
                Status = auction.Status,
                BidCount = await _db.AuctionBids.CountAsync(b => b.AuctionId == auction.Id),
                RecentBids = bids.Select(b => new AuctionBidDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    Amount = b.Amount,
                    CreatedTime = b.CreatedTime,
                }).ToList(),
            };
        }

        public async Task<IDataResult<AdvertisementListDto>> CreateReturningAsync(AdvertisementCreateDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return DataResult<AdvertisementListDto>.Fail(string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var spam = ContentModerationHelper.RejectReasonIfSpam($"{dto.Title} {dto.Description} {dto.Content}");
            if (spam != null)
                return DataResult<AdvertisementListDto>.Fail(spam);

            var entity = _mapper.Map<Advertisement>(dto);
            entity.CreatedTime = DateTime.Now;
            entity.IsActive = false;
            entity.Status = AdvertisementStatus.Pending;
            var paths = AdvertisementImagePathsHelper.Parse(dto.ImagePath, dto.ImagePathsJson);
            var (primary, json) = AdvertisementImagePathsHelper.ToStorage(paths);
            entity.ImagePath = primary;
            entity.ImagePathsJson = json;
            ListingIndexSync.Apply(entity);

            await _uow.GetRepository<Advertisement>().CreateAsync(entity);
            try
            {
                await _uow.SaveChanges();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                if (inner.Contains("String or binary data would be truncated", StringComparison.OrdinalIgnoreCase)
                    || inner.Contains("too long", StringComparison.OrdinalIgnoreCase))
                    return DataResult<AdvertisementListDto>.Fail("Bir alan çok uzun. Kısa açıklama en fazla 500 karakter olmalıdır.");
                return DataResult<AdvertisementListDto>.Fail($"Kayıt kaydedilemedi: {inner}");
            }

            var created = await _uow.GetRepository<Advertisement>().GetQuery()
                .Include(a => a.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == entity.Id);

            if (created == null)
                return DataResult<AdvertisementListDto>.Fail("İlan oluşturuldu ancak yüklenemedi.");

            await RecordPriceHistoryAsync(created.Id, created.ListingDetailsJson, null, created.CreatedTime);

            return DataResult<AdvertisementListDto>.Ok(MapToListDto(created), "İlan oluşturuldu.");
        }

        public async Task<IDataResult<AdvertisementListDto>> UpdateReturningAsync(AdvertisementUpdateDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return DataResult<AdvertisementListDto>.Fail(string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var unchangedEntity = await _uow.GetRepository<Advertisement>().FindAsync(dto.Id);
            if (unchangedEntity == null)
                return DataResult<AdvertisementListDto>.Fail("Kayıt bulunamadı.");

            var entity = _mapper.Map<Advertisement>(dto);
            entity.CreatedTime = unchangedEntity.CreatedTime;
            entity.UpdatedTime = DateTime.Now;
            // Durum / yayın bilgisi kullanıcı düzenlemesiyle değişmez (yalnızca admin onayı)
            entity.Status = unchangedEntity.Status;
            entity.ViewCount = unchangedEntity.ViewCount;
            entity.IsFeatured = unchangedEntity.IsFeatured;
            entity.FeaturedUntil = unchangedEntity.FeaturedUntil;
            entity.ExpiresAt = unchangedEntity.ExpiresAt;
            entity.LastBumpedAt = unchangedEntity.LastBumpedAt;
            entity.ArchivedAt = unchangedEntity.ArchivedAt;
            entity.RejectReason = unchangedEntity.RejectReason;
            entity.AdminNote = unchangedEntity.AdminNote;
            entity.IsActive = unchangedEntity.Status == AdvertisementStatus.Approved
                && unchangedEntity.ArchivedAt == null
                && unchangedEntity.IsActive;
            var paths = AdvertisementImagePathsHelper.Parse(dto.ImagePath, dto.ImagePathsJson);
            if (paths.Count == 0 && string.IsNullOrWhiteSpace(dto.ImagePathsJson)
                && !string.IsNullOrEmpty(unchangedEntity.ImagePathsJson))
            {
                paths = AdvertisementImagePathsHelper.Parse(unchangedEntity.ImagePath, unchangedEntity.ImagePathsJson);
            }
            var (primary, json) = AdvertisementImagePathsHelper.ToStorage(paths);
            entity.ImagePath = primary;
            entity.ImagePathsJson = json;
            ListingIndexSync.Apply(entity);

            _uow.GetRepository<Advertisement>().Update(entity, unchangedEntity);
            await _uow.SaveChanges();

            await RecordPriceHistoryAsync(dto.Id, entity.ListingDetailsJson, unchangedEntity.ListingDetailsJson);

            return await GetByIdAsync(dto.Id);
        }

        public async Task<IDataResult<List<AdvertisementListDto>>> GetSimilarAsync(int excludeId, int count = 4)
        {
            var source = await _uow.GetRepository<Advertisement>().GetQuery()
                .Include(a => a.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == excludeId);
            if (source == null)
                return DataResult<List<AdvertisementListDto>>.Fail("İlan bulunamadı.");

            var sourceDetails = ListingDetailsHelper.Parse(source.ListingDetailsJson);
            var sourcePrice = sourceDetails?.Price;

            var candidates = await _uow.GetRepository<Advertisement>().GetQuery()
                .Include(a => a.Category)
                .AsNoTracking()
                .Where(a => a.CategoryId == source.CategoryId && a.Id != excludeId && a.IsActive
                    && a.Status == AdvertisementStatus.Approved)
                .OrderByDescending(a => a.IsFeatured)
                .ThenByDescending(a => a.CreatedTime)
                .Take(60)
                .ToListAsync();

            var ranked = candidates
                .Select(a =>
                {
                    var d = ListingDetailsHelper.Parse(a.ListingDetailsJson);
                    var score = 0;
                    if (!string.IsNullOrWhiteSpace(sourceDetails?.City) && !string.IsNullOrWhiteSpace(d?.City)
                        && string.Equals(sourceDetails.City, d.City, StringComparison.OrdinalIgnoreCase))
                        score += 3;
                    if (sourcePrice is > 0 && d?.Price is > 0)
                    {
                        var ratio = (double)d.Price.Value / (double)sourcePrice.Value;
                        if (ratio is >= 0.7 and <= 1.3) score += 2;
                    }
                    if (!string.IsNullOrWhiteSpace(sourceDetails?.Brand) && !string.IsNullOrWhiteSpace(d?.Brand)
                        && string.Equals(sourceDetails.Brand, d.Brand, StringComparison.OrdinalIgnoreCase))
                        score += 2;
                    if (a.IsFeatured) score += 1;
                    return (Ad: a, Score: score);
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Ad.CreatedTime)
                .Take(count)
                .Select(x => x.Ad)
                .ToList();

            var similarItems = ranked.Select(e => MapToListDto(e)).ToList();
            await EnrichRatingsAsync(similarItems);
            return DataResult<List<AdvertisementListDto>>.Ok(similarItems);
        }

        public async Task<IDataResult<List<PriceHistoryPointDto>>> GetPriceHistoryAsync(int advertisementId)
        {
            var ad = await _uow.GetRepository<Advertisement>().GetQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == advertisementId);
            if (ad == null)
                return DataResult<List<PriceHistoryPointDto>>.Fail("İlan bulunamadı.");

            var items = await _db.AdvertisementPriceHistories.AsNoTracking()
                .Where(h => h.AdvertisementId == advertisementId)
                .OrderBy(h => h.RecordedAt)
                .Select(h => new PriceHistoryPointDto { Price = h.Price, RecordedAt = h.RecordedAt })
                .ToListAsync();

            if (items.Count == 0)
            {
                var price = ListingDetailsHelper.Parse(ad.ListingDetailsJson)?.Price;
                if (price is > 0)
                {
                    await RecordPriceHistoryAsync(ad.Id, ad.ListingDetailsJson, null, ad.CreatedTime);
                    items.Add(new PriceHistoryPointDto { Price = price.Value, RecordedAt = ad.CreatedTime.ToUniversalTime() });
                }
            }

            return DataResult<List<PriceHistoryPointDto>>.Ok(items);
        }

        public async Task<IDataResult<List<AdvertisementListDto>>> GetByIdsAsync(IReadOnlyList<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return DataResult<List<AdvertisementListDto>>.Ok(new List<AdvertisementListDto>());

            var distinct = ids.Distinct().Take(4).ToList();
            var entities = await _uow.GetRepository<Advertisement>().GetQuery()
                .Include(a => a.Category)
                .AsNoTracking()
                .Where(a => distinct.Contains(a.Id) && a.IsActive && a.Status == AdvertisementStatus.Approved)
                .ToListAsync();

            var map = entities.ToDictionary(e => e.Id);
            var ordered = distinct.Where(map.ContainsKey).Select(id => map[id]).ToList();
            var compareItems = ordered.Select(e => MapToListDto(e)).ToList();
            await EnrichRatingsAsync(compareItems);
            return DataResult<List<AdvertisementListDto>>.Ok(compareItems);
        }

        private async Task RecordPriceHistoryAsync(
            int advertisementId,
            string? newJson,
            string? oldJson,
            DateTime? recordedAt = null)
        {
            var newPrice = ListingDetailsHelper.Parse(newJson)?.Price;
            if (newPrice is not > 0) return;

            if (oldJson != null)
            {
                var oldPrice = ListingDetailsHelper.Parse(oldJson)?.Price;
                if (oldPrice == newPrice) return;
            }

            var lastPrice = await _db.AdvertisementPriceHistories.AsNoTracking()
                .Where(h => h.AdvertisementId == advertisementId)
                .OrderByDescending(h => h.RecordedAt)
                .Select(h => h.Price)
                .FirstOrDefaultAsync();
            if (lastPrice == newPrice) return;

            _db.AdvertisementPriceHistories.Add(new AdvertisementPriceHistory
            {
                AdvertisementId = advertisementId,
                Price = newPrice.Value,
                RecordedAt = recordedAt?.ToUniversalTime() ?? DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
        }

        public async Task<IResult> ApproveAsync(int id)
        {
            var entity = await _uow.GetRepository<Advertisement>().FindAsync(id);
            if (entity == null)
                return Result.Fail("İlan bulunamadı.");

            entity.Status = AdvertisementStatus.Approved;
            entity.IsActive = true;
            entity.UpdatedTime = DateTime.Now;

            _uow.GetRepository<Advertisement>().Update(entity, entity);
            await _uow.SaveChanges();
            return Result.Ok("İlan onaylandı ve yayına alındı.");
        }

        public async Task<IResult> RejectAsync(int id)
        {
            var entity = await _uow.GetRepository<Advertisement>().FindAsync(id);
            if (entity == null)
                return Result.Fail("İlan bulunamadı.");

            entity.Status = AdvertisementStatus.Rejected;
            entity.IsActive = false;
            entity.UpdatedTime = DateTime.Now;

            _uow.GetRepository<Advertisement>().Update(entity, entity);
            await _uow.SaveChanges();
            return Result.Ok("İlan reddedildi.");
        }

        public async Task<IDataResult<MyAdCountsDto>> GetMyAdCountsAsync(int userId)
        {
            var q = _uow.GetRepository<Advertisement>().GetQuery().AsNoTracking().Where(a => a.UserId == userId);
            var counts = new MyAdCountsDto
            {
                All = await q.CountAsync(),
                Pending = await q.CountAsync(a => a.Status == AdvertisementStatus.Pending),
                Approved = await q.CountAsync(a =>
                    a.Status == AdvertisementStatus.Approved && a.IsActive && a.ArchivedAt == null),
                Rejected = await q.CountAsync(a => a.Status == AdvertisementStatus.Rejected),
            };
            return DataResult<MyAdCountsDto>.Ok(counts);
        }

        public async Task<IDataResult<PhoneRevealDto>> RevealPhoneAsync(int advertisementId, int viewerUserId)
        {
            const int dailyLimit = 30;
            var since = DateTime.UtcNow.AddDays(-1);
            var revealsToday = await _db.PhoneRevealLogs.CountAsync(l =>
                l.ViewerUserId == viewerUserId && l.CreatedTime >= since);
            if (revealsToday >= dailyLimit)
                return DataResult<PhoneRevealDto>.Fail("Günlük telefon görüntüleme limitine ulaştınız.");

            var ad = await _db.Advertisements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == advertisementId);
            if (ad == null || !ad.IsActive || ad.Status != AdvertisementStatus.Approved)
                return DataResult<PhoneRevealDto>.Fail("İlan bulunamadı.");

            var seller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == ad.UserId);
            if (seller == null || string.IsNullOrWhiteSpace(seller.PhoneNumber))
                return DataResult<PhoneRevealDto>.Fail("Satıcı telefon numarası kayıtlı değil.");

            _db.PhoneRevealLogs.Add(new PhoneRevealLog
            {
                AdvertisementId = advertisementId,
                ViewerUserId = viewerUserId,
                CreatedTime = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();

            return DataResult<PhoneRevealDto>.Ok(new PhoneRevealDto
            {
                PhoneNumber = seller.PhoneNumber!,
                DisplayName = $"{seller.FirstName} {seller.LastName}".Trim(),
            });
        }

        public async Task<IResult> BumpAdAsync(int advertisementId, int userId)
        {
            var entity = await _uow.GetRepository<Advertisement>().FindAsync(advertisementId);
            if (entity == null || entity.UserId != userId)
                return Result.Fail("İlan bulunamadı.");
            if (entity.Status != AdvertisementStatus.Approved)
                return Result.Fail("Yalnızca yayında olan ilanlar yukarı taşınabilir.");

            entity.LastBumpedAt = DateTime.UtcNow;
            entity.UpdatedTime = DateTime.Now;
            _uow.GetRepository<Advertisement>().Update(entity, entity);
            await _db.SaveChangesAsync();
            return Result.Ok("İlan listenin üstüne taşındı.");
        }

        public async Task<IResult> ExtendAdAsync(int advertisementId, int userId, int days)
        {
            if (days is < 1 or > 365)
                return Result.Fail("Geçersiz süre.");
            var entity = await _uow.GetRepository<Advertisement>().FindAsync(advertisementId);
            if (entity == null || entity.UserId != userId)
                return Result.Fail("İlan bulunamadı.");

            var baseDate = entity.ExpiresAt ?? DateTime.UtcNow;
            if (baseDate < DateTime.UtcNow) baseDate = DateTime.UtcNow;
            entity.ExpiresAt = baseDate.AddDays(days);
            entity.UpdatedTime = DateTime.Now;
            _uow.GetRepository<Advertisement>().Update(entity, entity);
            await _db.SaveChangesAsync();
            return Result.Ok($"İlan süresi {days} gün uzatıldı.");
        }

        private static IQueryable<Advertisement> ApplyDetailFiltersQuery(IQueryable<Advertisement> query, AdvertisementFilterDto filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.Brand))
            {
                var brand = filter.Brand.Trim();
                query = query.Where(a => a.ListingDetailsJson != null &&
                    (a.ListingDetailsJson.Contains($"\"brand\":\"{brand}\"") ||
                     a.ListingDetailsJson.Contains($"\"brand\": \"{brand}\"")));
            }

            if (!string.IsNullOrWhiteSpace(filter.Model))
            {
                var model = filter.Model.Trim();
                query = query.Where(a => a.ListingDetailsJson != null &&
                    a.ListingDetailsJson.Contains(model));
            }

            return query;
        }

        private static List<Advertisement> ApplyBrandModelFilters(List<Advertisement> list, AdvertisementFilterDto filter)
        {
            return list.Where(a =>
            {
                var d = ListingDetailsHelper.Parse(a.ListingDetailsJson);
                if (!string.IsNullOrWhiteSpace(filter.Brand) &&
                    !string.Equals(d?.Brand, filter.Brand.Trim(), StringComparison.OrdinalIgnoreCase)) return false;
                if (!string.IsNullOrWhiteSpace(filter.Model) &&
                    (d?.Model == null || !d.Model.Contains(filter.Model.Trim(), StringComparison.OrdinalIgnoreCase))) return false;
                return true;
            }).ToList();
        }
    }
}
