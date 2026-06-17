using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Dtos.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IPlatformService _platform;

        public AnalyticsController(IPlatformService platform) => _platform = platform;

        [HttpGet("overview")]
        [AllowAnonymous]
        public async Task<IActionResult> Overview()
        {
            var data = await _platform.GetAnalyticsOverviewAsync();
            return Ok(ApiResponse<AnalyticsOverviewDto>.Ok(data));
        }

        [HttpPost("search-log")]
        [AllowAnonymous]
        public async Task<IActionResult> LogSearch([FromQuery] int? categoryId, [FromQuery] string? search)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
                userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _platform.LogSearchAsync(userId, categoryId, search);
            return Ok(ApiResponse.Ok("Logged."));
        }

        [HttpGet("ad/{advertisementId:int}")]
        [Authorize]
        public async Task<IActionResult> AdStats(int advertisementId)
        {
            var data = await _platform.GetAdAnalyticsAsync(GetUserId(), advertisementId);
            if (data == null)
                return NotFound(ApiResponse.Fail("İlan bulunamadı veya yetkiniz yok."));
            return Ok(ApiResponse<AdAnalyticsDto>.Ok(data));
        }

        [HttpGet("map")]
        [AllowAnonymous]
        public async Task<IActionResult> Map([FromQuery] AdvertisementFilterDto filter)
        {
            var items = await _platform.GetMapListingsAsync(filter);
            return Ok(ApiResponse<List<MapListingDto>>.Ok(items));
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
