using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Helpers;
using AdvertisementApp.Common.Result;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.Business.Service
{
    public class AuctionService : IAuctionService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IRealtimeNotifier _realtime;

        public AuctionService(AdvertisementAppDbContext db, IRealtimeNotifier realtime)
        {
            _db = db;
            _realtime = realtime;
        }

        public async Task<IDataResult<AuctionDto>> CreateAsync(CreateAuctionDto dto, int userId)
        {
            var ad = await _db.Advertisements.FirstOrDefaultAsync(a => a.Id == dto.AdvertisementId);
            if (ad == null) return DataResult<AuctionDto>.Fail("İlan bulunamadı.");
            if (ad.UserId != userId) return DataResult<AuctionDto>.Fail("Bu ilan size ait değil.");
            if (await _db.Auctions.AnyAsync(a => a.AdvertisementId == dto.AdvertisementId))
                return DataResult<AuctionDto>.Fail("Bu ilan için müzayede zaten tanımlı.");

            if (dto.EndsAt <= dto.StartsAt)
                return DataResult<AuctionDto>.Fail("Bitiş tarihi başlangıçtan sonra olmalı.");
            if (dto.MinIncrement <= 0 || dto.StartingBid <= 0)
                return DataResult<AuctionDto>.Fail("Başlangıç fiyatı ve artış tutarı pozitif olmalı.");

            var auction = new Auction
            {
                AdvertisementId = dto.AdvertisementId,
                StartsAt = dto.StartsAt.ToUniversalTime(),
                EndsAt = dto.EndsAt.ToUniversalTime(),
                StartingBid = dto.StartingBid,
                MinIncrement = dto.MinIncrement,
                CurrentBid = null,
                Status = DateTime.UtcNow >= dto.StartsAt.ToUniversalTime() ? "active" : "scheduled",
                CreatedTime = DateTime.UtcNow,
            };

            ad.ListingType = ListingType.Auction;
            _db.Auctions.Add(auction);
            await _db.SaveChangesAsync();

            return DataResult<AuctionDto>.Ok(await MapAsync(auction.Id), "Müzayede oluşturuldu.");
        }

        public async Task<IDataResult<AuctionDto>> GetByAdvertisementAsync(int advertisementId)
        {
            var auction = await _db.Auctions.AsNoTracking().FirstOrDefaultAsync(a => a.AdvertisementId == advertisementId);
            if (auction == null) return DataResult<AuctionDto>.Fail("Müzayede bulunamadı.");
            return DataResult<AuctionDto>.Ok(await MapAsync(auction.Id));
        }

        public async Task<IDataResult<AuctionDto>> PlaceBidAsync(int auctionId, int userId, decimal amount)
        {
            var auction = await _db.Auctions.Include(a => a.Advertisement).FirstOrDefaultAsync(a => a.Id == auctionId);
            if (auction == null) return DataResult<AuctionDto>.Fail("Müzayede bulunamadı.");
            if (auction.Advertisement.UserId == userId)
                return DataResult<AuctionDto>.Fail("Kendi ilanınıza teklif veremezsiniz.");

            var now = DateTime.UtcNow;
            if (auction.Status == "ended" || now > auction.EndsAt)
                return DataResult<AuctionDto>.Fail("Müzayede sona ermiş.");
            if (now < auction.StartsAt)
                return DataResult<AuctionDto>.Fail("Müzayede henüz başlamadı.");

            auction.Status = "active";
            var minRequired = (auction.CurrentBid ?? auction.StartingBid) + auction.MinIncrement;
            if (amount < minRequired)
                return DataResult<AuctionDto>.Fail($"Minimum teklif: {minRequired:N0} TL");

            auction.CurrentBid = amount;
            auction.HighBidderUserId = userId;
            _db.AuctionBids.Add(new AuctionBid
            {
                AuctionId = auctionId,
                UserId = userId,
                Amount = amount,
                CreatedTime = now,
            });
            await _db.SaveChangesAsync();

            await _realtime.PushToUserAsync(auction.Advertisement.UserId, "auction", new
            {
                auctionId,
                advertisementId = auction.AdvertisementId,
                amount,
                message = $"{amount:N0} TL teklif geldi",
            });

            return DataResult<AuctionDto>.Ok(await MapAsync(auctionId), "Teklifiniz alındı.");
        }

        public async Task CloseExpiredAsync()
        {
            var expired = await _db.Auctions
                .Where(a => a.Status != "ended" && a.EndsAt < DateTime.UtcNow)
                .ToListAsync();
            foreach (var a in expired)
                a.Status = "ended";
            if (expired.Count > 0)
                await _db.SaveChangesAsync();
        }

        private async Task<AuctionDto> MapAsync(int auctionId)
        {
            var auction = await _db.Auctions.AsNoTracking().FirstAsync(a => a.Id == auctionId);
            var bids = await _db.AuctionBids.AsNoTracking()
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.CreatedTime)
                .Take(10)
                .ToListAsync();

            var userIds = bids.Select(b => b.UserId).Distinct().ToList();
            var users = await _db.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            return new AuctionDto
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
                BidCount = await _db.AuctionBids.CountAsync(b => b.AuctionId == auctionId),
                RecentBids = bids.Select(b => new AuctionBidDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    UserDisplayName = users.GetValueOrDefault(b.UserId),
                    Amount = b.Amount,
                    CreatedTime = b.CreatedTime,
                }).ToList(),
            };
        }
    }
}
