using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdvertisementApp.UI.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly AdvertisementAppDbContext _context;

        public FavoriteController(AdvertisementAppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            try
            {
                var favorites = await _context.Favorites
                    .Include(f => f.Advertisement)
                        .ThenInclude(a => a.Category)
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedTime)
                    .ToListAsync();

                var dtos = favorites.Select(f => new AdvertisementListDto
                {
                    Id = f.Advertisement.Id,
                    UserId = f.Advertisement.UserId,
                    CategoryId = f.Advertisement.CategoryId,
                    CategoryName = f.Advertisement.Category?.Name ?? string.Empty,
                    Title = f.Advertisement.Title,
                    Description = f.Advertisement.Description,
                    Content = f.Advertisement.Content,
                    ImagePath = f.Advertisement.ImagePath,
                    IsActive = f.Advertisement.IsActive,
                    CreatedTime = f.Advertisement.CreatedTime
                }).ToList();

                return View(dtos);
            }
            catch
            {
                TempData["Error"] = "Favoriler yüklenemedi. Lütfen migration'ı çalıştırın.";
                return View(new List<AdvertisementListDto>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int advertisementId, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.AdvertisementId == advertisementId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                TempData["Success"] = "Favorilerden kaldırıldı.";
            }
            else
            {
                _context.Favorites.Add(new Favorite
                {
                    UserId = userId,
                    AdvertisementId = advertisementId
                });
                TempData["Success"] = "Favorilere eklendi.";
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Details", "Advertisement", new { id = advertisementId });
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
