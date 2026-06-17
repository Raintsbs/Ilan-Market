using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Constants;
using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.DataAccess.Extension;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AdvertisementApp.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = DependencyExtension.AdminScheme, Roles = AppRoles.Admin)]
    public class DashboardController : Controller
    {
        private readonly IAdvertisementService _advertisementService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(
            IAdvertisementService advertisementService,
            ICategoryService categoryService,
            UserManager<AppUser> userManager)
        {
            _advertisementService = advertisementService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var ads = await _advertisementService.GetAllAsync();
            var cats = await _categoryService.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();

            var adList = ads.Success && ads.Data != null ? ads.Data : new List<AdvertisementListDto>();
            var catList = cats.Success && cats.Data != null ? cats.Data : new List<AdvertisementApp.Dtos.CategoryDtos.CategoryListDto>();

            ViewBag.TotalAds = adList.Count;
            ViewBag.ActiveAds = adList.Count(a => a.IsActive);
            ViewBag.PendingAds = adList.Count(a => a.Status == AdvertisementStatus.Pending);
            ViewBag.TotalCategories = catList.Count;
            ViewBag.TotalUsers = users.Count;

            // Son 5 ilan
            ViewBag.RecentAds = adList.OrderByDescending(a => a.CreatedTime).Take(5).ToList();

            // Son 5 kullanıcı
            ViewBag.RecentUsers = users.OrderByDescending(u => u.Id).Take(5).ToList();

            // Kategori bazlı ilan dağılımı (grafik için)
            var chartData = catList
                .Select(c => new
                {
                    label = c.Name,
                    count = adList.Count(a => a.CategoryId == c.Id)
                })
                .Where(x => x.count > 0)
                .OrderByDescending(x => x.count)
                .ToList();

            ViewBag.ChartLabels = JsonSerializer.Serialize(chartData.Select(x => x.label).ToList());
            ViewBag.ChartData = JsonSerializer.Serialize(chartData.Select(x => x.count).ToList());

            return View();
        }
    }
}
