using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Constants;
using AdvertisementApp.DataAccess.Extension;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Entities;
using AdvertisementApp.UI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;

namespace AdvertisementApp.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = DependencyExtension.AdminScheme, Roles = AppRoles.Admin)]
    public class AdvertisementsController : Controller
    {
        private readonly IAdvertisementService _advertisementService;
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<NotificationHub> _hub;

        public AdvertisementsController(
            IAdvertisementService advertisementService,
            ICategoryService categoryService,
            IHubContext<NotificationHub> hub)
        {
            _advertisementService = advertisementService;
            _categoryService = categoryService;
            _hub = hub;
        }

        public async Task<IActionResult> Index(AdvertisementFilterDto filter)
        {
            var result = await _advertisementService.GetPagedAsync(filter);
            await LoadCategoriesAsync(filter.CategoryId);
            ViewBag.Filter = filter;
            ViewBag.PendingCount = await GetPendingCountAsync();
            return View(result.Success && result.Data != null
                ? result.Data
                : new AdvertisementApp.Common.Models.PagedResult<AdvertisementListDto>());
        }

        [HttpGet]
        public async Task<IActionResult> Pending()
        {
            var filter = new AdvertisementFilterDto { Status = AdvertisementStatus.Pending, PageSize = 50 };
            var result = await _advertisementService.GetPagedAsync(filter);
            return View(result.Success && result.Data != null
                ? result.Data.Items
                : new List<AdvertisementListDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _advertisementService.ApproveAsync(id);
            if (result.Success)
                await _hub.Clients.All.SendAsync("OnChange", "advertisement");

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Pending));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _advertisementService.RejectAsync(id);
            if (result.Success)
                await _hub.Clients.All.SendAsync("OnChange", "advertisement");

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Pending));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _advertisementService.DeleteAsync(id);
            if (result.Success)
                await _hub.Clients.All.SendAsync("OnChange", "advertisement");

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var existing = await _advertisementService.GetByIdAsync(id);
            if (!existing.Success || existing.Data == null)
                return NotFound();

            var dto = new AdvertisementUpdateDto
            {
                Id = existing.Data.Id,
                UserId = existing.Data.UserId,
                CategoryId = existing.Data.CategoryId,
                Title = existing.Data.Title,
                Description = existing.Data.Description,
                Content = existing.Data.Content,
                ImagePath = existing.Data.ImagePath,
                IsActive = !existing.Data.IsActive
            };

            var result = await _advertisementService.UpdateAsync(dto);
            if (result.Success)
                await _hub.Clients.All.SendAsync("OnChange", "advertisement");

            TempData[result.Success ? "Success" : "Error"] = result.Success
                ? $"İlan {(dto.IsActive ? "aktif" : "pasif")} yapıldı."
                : result.Message;

            return RedirectToAction(nameof(Index));
        }

        private async Task<int> GetPendingCountAsync()
        {
            var filter = new AdvertisementFilterDto { Status = AdvertisementStatus.Pending, PageSize = 1000 };
            var result = await _advertisementService.GetPagedAsync(filter);
            return result.Success && result.Data != null ? result.Data.TotalCount : 0;
        }

        private async Task LoadCategoriesAsync(int? selectedId = null)
        {
            var categories = await _categoryService.GetAllAsync();
            var items = categories.Success && categories.Data != null
                ? categories.Data.ToList()
                : new List<AdvertisementApp.Dtos.CategoryDtos.CategoryListDto>();
            ViewBag.Categories = new SelectList(items, "Id", "Name", selectedId);
        }
    }
}
