using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Constants;
using AdvertisementApp.DataAccess.Extension;
using AdvertisementApp.Dtos.CategoryDtos;
using AdvertisementApp.UI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AdvertisementApp.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = DependencyExtension.AdminScheme, Roles = AppRoles.Admin)]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<NotificationHub> _hub;

        public CategoriesController(ICategoryService categoryService, IHubContext<NotificationHub> hub)
        {
            _categoryService = categoryService;
            _hub = hub;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _categoryService.GetAllAsync();
            return View(result.Success && result.Data != null ? result.Data : new List<CategoryListDto>());
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var result = await _categoryService.CreateAsync(dto);
            if (result.Success)
                await _hub.Clients.All.SendAsync("OnChange", "category");

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return result.Success ? RedirectToAction(nameof(Index)) : View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();

            return View(new CategoryUpdateDto
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                Description = result.Data.Description,
                IsActive = result.Data.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryUpdateDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var result = await _categoryService.UpdateAsync(dto);
            if (result.Success)
                await _hub.Clients.All.SendAsync("OnChange", "category");

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return result.Success ? RedirectToAction(nameof(Index)) : View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (result.Success)
                await _hub.Clients.All.SendAsync("OnChange", "category");

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
