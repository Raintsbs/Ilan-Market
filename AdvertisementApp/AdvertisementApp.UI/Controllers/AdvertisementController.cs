using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Constants;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Entities;
using AdvertisementApp.UI.Hubs;
using AdvertisementApp.UI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AdvertisementApp.UI.Controllers
{
    public class AdvertisementController : Controller
    {
        private readonly IAdvertisementService _advertisementService;
        private readonly ICategoryService _categoryService;
        private readonly IImageStorageService _imageStorageService;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IFavoriteService _favoriteService;

        public AdvertisementController(
            IAdvertisementService advertisementService,
            ICategoryService categoryService,
            IImageStorageService imageStorageService,
            IHubContext<NotificationHub> hub,
            IFavoriteService favoriteService)
        {
            _advertisementService = advertisementService;
            _categoryService = categoryService;
            _imageStorageService = imageStorageService;
            _hub = hub;
            _favoriteService = favoriteService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(AdvertisementFilterDto filter)
        {
            // Genel listede sadece onaylanmış ilanlar
            filter.Status = AdvertisementStatus.Approved;
            return await IndexCoreAsync(filter, isMyAds: false);
        }

        [Authorize]
        public async Task<IActionResult> MyAds(AdvertisementFilterDto filter)
        {
            filter.UserId = GetCurrentUserId();
            return await IndexCoreAsync(filter, isMyAds: true);
        }

        private async Task<IActionResult> IndexCoreAsync(AdvertisementFilterDto filter, bool isMyAds)
        {
            var result = await _advertisementService.GetPagedAsync(filter);
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Message;
                return View(isMyAds ? "MyAds" : "Index", new PagedResult<AdvertisementListDto>());
            }

            await LoadCategoriesAsync(filter.CategoryId);
            ViewBag.Filter = filter;
            ViewBag.IsMyAds = isMyAds;
            return View(isMyAds ? "MyAds" : "Index", result.Data);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _advertisementService.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return NotFound();

            var similar = await _advertisementService.GetSimilarAsync(id);
            ViewBag.SimilarAds = similar.Success ? similar.Data : new List<AdvertisementListDto>();

            // Giriş yapmış kullanıcı için favori durumu
            ViewBag.IsFavorite = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
                ViewBag.IsFavorite = await _favoriteService.IsFavoriteAsync(userId, id);
            }

            return View(result.Data);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View(new AdvertisementCreateViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdvertisementCreateViewModel model)
        {
            await LoadCategoriesAsync(model.CategoryId);

            if (!ModelState.IsValid)
                return View(model);

            var imageResult = await _imageStorageService.SaveImageAsync(model.ImageFile);
            if (!imageResult.Success)
            {
                ModelState.AddModelError(nameof(model.ImageFile), imageResult.Message);
                return View(model);
            }

            var dto = new AdvertisementCreateDto
            {
                UserId = GetCurrentUserId(),
                CategoryId = model.CategoryId,
                Title = model.Title,
                Description = model.Description,
                Content = model.Content,
                ImagePath = string.IsNullOrEmpty(imageResult.Data) ? model.ImagePath : imageResult.Data
            };

            var result = await _advertisementService.CreateAsync(dto);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(model);
            }

            await _hub.Clients.All.SendAsync("OnChange", "advertisement");
            TempData["Success"] = "İlanınız başarıyla gönderildi. Admin onayından sonra yayına alınacaktır.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _advertisementService.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return NotFound();

            if (!IsOwnerOrAdmin(result.Data.UserId))
                return Forbid();

            await LoadCategoriesAsync(result.Data.CategoryId);
            var model = new AdvertisementEditViewModel
            {
                Id = result.Data.Id,
                UserId = result.Data.UserId,
                CategoryId = result.Data.CategoryId,
                Title = result.Data.Title,
                Description = result.Data.Description,
                Content = result.Data.Content,
                ImagePath = result.Data.ImagePath,
                IsActive = result.Data.IsActive
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdvertisementEditViewModel model)
        {
            var existing = await _advertisementService.GetByIdAsync(model.Id);
            if (!existing.Success || existing.Data == null)
                return NotFound();

            if (!IsOwnerOrAdmin(existing.Data.UserId))
                return Forbid();

            await LoadCategoriesAsync(model.CategoryId);

            if (!ModelState.IsValid)
                return View(model);

            var imageResult = await _imageStorageService.SaveImageAsync(model.ImageFile);
            if (!imageResult.Success)
            {
                ModelState.AddModelError(nameof(model.ImageFile), imageResult.Message);
                return View(model);
            }

            var dto = new AdvertisementUpdateDto
            {
                Id = model.Id,
                UserId = existing.Data.UserId,
                CategoryId = model.CategoryId,
                Title = model.Title,
                Description = model.Description,
                Content = model.Content,
                ImagePath = string.IsNullOrEmpty(imageResult.Data) ? model.ImagePath : imageResult.Data,
                IsActive = model.IsActive
            };

            var result = await _advertisementService.UpdateAsync(dto);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return View(model);
            }

            await _hub.Clients.All.SendAsync("OnChange", "advertisement");
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(MyAds));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _advertisementService.GetByIdAsync(id);
            if (!existing.Success || existing.Data == null)
                return NotFound();

            if (!IsOwnerOrAdmin(existing.Data.UserId))
                return Forbid();

            var result = await _advertisementService.DeleteAsync(id);
            if (result.Success)
                await _hub.Clients.All.SendAsync("OnChange", "advertisement");
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(MyAds));
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private bool IsOwnerOrAdmin(int userId) =>
            User.IsInRole(AppRoles.Admin) || GetCurrentUserId() == userId;

        private async Task LoadCategoriesAsync(int? selectedId = null)
        {
            var categories = await _categoryService.GetAllAsync();
            var items = categories.Success && categories.Data != null
                ? categories.Data.Where(c => c.IsActive).ToList()
                : new List<AdvertisementApp.Dtos.CategoryDtos.CategoryListDto>();
            ViewBag.Categories = new SelectList(items, "Id", "Name", selectedId);
        }
    }
}
