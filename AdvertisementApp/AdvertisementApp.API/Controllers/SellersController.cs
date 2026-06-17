using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class SellersController : ControllerBase
    {
        private readonly IPlatformService _platform;

        public SellersController(IPlatformService platform) => _platform = platform;

        [HttpGet("by-slug/{slug}")]
        public async Task<IActionResult> GetProfileBySlug(string slug)
        {
            var profile = await _platform.GetSellerPublicProfileBySlugAsync(slug);
            if (profile == null)
                return NotFound(ApiResponse.Fail("Mağaza bulunamadı."));
            return Ok(ApiResponse<object>.Ok(profile));
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var profile = await _platform.GetSellerPublicProfileAsync(userId);
            if (profile == null)
                return NotFound(ApiResponse.Fail("Satıcı bulunamadı."));
            return Ok(ApiResponse<object>.Ok(profile));
        }

        [HttpGet("{userId:int}/analytics")]
        public async Task<IActionResult> GetAnalytics(int userId)
        {
            var stats = await _platform.GetSellerAnalyticsAsync(userId);
            if (stats == null)
                return NotFound(ApiResponse.Fail("Satıcı bulunamadı."));
            return Ok(ApiResponse<object>.Ok(stats));
        }
    }
}
