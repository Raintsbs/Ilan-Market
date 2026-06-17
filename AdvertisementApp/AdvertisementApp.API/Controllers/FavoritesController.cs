using AdvertisementApp.Common.Helpers;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.Dtos.Platform;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly AdvertisementAppDbContext _context;
        private readonly IPlatformService _platform;

        public FavoritesController(AdvertisementAppDbContext context, IPlatformService platform)
        {
            _context = context;
            _platform = platform;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var userId = GetUserId();
            var rows = await _context.Favorites
                .AsNoTracking()
                .Include(f => f.Advertisement)
                    .ThenInclude(a => a.Category)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedTime)
                .Select(f => f.Advertisement)
                .ToListAsync();

            var sellerIds = rows.Select(a => a.UserId).Distinct().ToList();
            var verifiedMap = await _context.Users.AsNoTracking()
                .Where(u => sellerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.IsVerified);
            var items = rows.Select(a => MapAd(a, verifiedMap.GetValueOrDefault(a.UserId))).ToList();
            return Ok(ApiResponse<List<AdvertisementListDto>>.Ok(items));
        }

        private static AdvertisementListDto MapAd(Advertisement a, bool sellerVerified = false)
        {
            var paths = AdvertisementImagePathsHelper.Parse(a.ImagePath, a.ImagePathsJson);
            return new AdvertisementListDto
            {
                Id = a.Id,
                UserId = a.UserId,
                CategoryId = a.CategoryId,
                CategoryName = a.Category?.Name ?? string.Empty,
                Title = a.Title,
                Description = a.Description,
                Content = a.Content,
                ListingDetails = ListingDetailsHelper.Parse(a.ListingDetailsJson),
                ImagePath = paths.FirstOrDefault(),
                ImagePaths = paths,
                IsActive = a.IsActive,
                Status = a.Status,
                CreatedTime = a.CreatedTime,
                UpdatedTime = a.UpdatedTime,
                ViewCount = a.ViewCount,
                IsFeatured = a.IsFeatured,
                SellerIsVerified = sellerVerified,
            };
        }

        [HttpPut("{advertisementId:int}/price-alert")]
        public async Task<IActionResult> SetPriceAlert(int advertisementId, [FromBody] FavoritePriceAlertDto dto)
        {
            await _platform.SetFavoritePriceAlertAsync(GetUserId(), advertisementId, dto);
            return Ok(ApiResponse.Ok("Fiyat alarmı güncellendi."));
        }

        [HttpGet("{advertisementId:int}/status")]
        public async Task<IActionResult> GetStatus(int advertisementId)
        {
            var userId = GetUserId();
            var exists = await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.AdvertisementId == advertisementId);
            return Ok(ApiResponse<bool>.Ok(exists));
        }

        [HttpPost("{advertisementId:int}/toggle")]
        public async Task<IActionResult> Toggle(int advertisementId)
        {
            var userId = GetUserId();
            var adExists = await _context.Advertisements.AnyAsync(a => a.Id == advertisementId);
            if (!adExists)
                return NotFound(ApiResponse<bool>.Fail("İlan bulunamadı."));

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.AdvertisementId == advertisementId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<bool>.Ok(false, "Favorilerden kaldırıldı."));
            }

            _context.Favorites.Add(new Favorite
            {
                UserId = userId,
                AdvertisementId = advertisementId,
                CreatedTime = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Favorilere eklendi."));
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
