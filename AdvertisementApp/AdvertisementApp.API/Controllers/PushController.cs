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
    public class PushController : ControllerBase
    {
        private readonly IPlatformService _platform;

        public PushController(IPlatformService platform) => _platform = platform;

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionDto dto)
        {
            await _platform.SavePushSubscriptionAsync(GetUserId(), dto);
            return Ok(ApiResponse.Ok("Push aboneliği kaydedildi."));
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
