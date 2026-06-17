using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IPlatformService _platform;

        public NotificationsController(IPlatformService platform) => _platform = platform;

        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var items = await _platform.GetNotificationsAsync(GetUserId());
            return Ok(ApiResponse<List<NotificationDto>>.Ok(items));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var count = await _platform.GetUnreadNotificationCountAsync(GetUserId());
            return Ok(ApiResponse<int>.Ok(count));
        }

        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _platform.MarkNotificationReadAsync(GetUserId(), id);
            return Ok(ApiResponse.Ok("Okundu."));
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
