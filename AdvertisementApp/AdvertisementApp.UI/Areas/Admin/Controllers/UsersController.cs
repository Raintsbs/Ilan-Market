using AdvertisementApp.Common.Constants;
using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.DataAccess.Extension;
using AdvertisementApp.UI.Areas.Admin.Models;
using AdvertisementApp.UI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = DependencyExtension.AdminScheme, Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHubContext<NotificationHub> _hub;

        public UsersController(UserManager<AppUser> userManager, IHubContext<NotificationHub> hub)
        {
            _userManager = userManager;
            _hub = hub;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var userList = new List<AdminUserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList(),
                    CreatedDate = null
                });
            }

            return View(userList.OrderBy(u => u.Id).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRole(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Kendi rolünü değiştiremesin
            var currentAdminEmail = User.Identity?.Name;
            if (user.Email == currentAdminEmail)
            {
                TempData["Error"] = "Kendi rolünüzü değiştiremezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
            {
                // Admin → User
                await _userManager.RemoveFromRoleAsync(user, AppRoles.Admin);
                if (!await _userManager.IsInRoleAsync(user, AppRoles.User))
                    await _userManager.AddToRoleAsync(user, AppRoles.User);
                TempData["Success"] = $"{user.Email} kullanıcısının rolü User olarak güncellendi.";
            }
            else
            {
                // User → Admin
                await _userManager.RemoveFromRoleAsync(user, AppRoles.User);
                await _userManager.AddToRoleAsync(user, AppRoles.Admin);
                TempData["Success"] = $"{user.Email} kullanıcısına Admin rolü verildi.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
            {
                TempData["Error"] = "Admin kullanıcısı silinemez.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                await _hub.Clients.All.SendAsync("OnChange", "user");

            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Kullanıcı silindi."
                : string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Index));
        }
    }
}
