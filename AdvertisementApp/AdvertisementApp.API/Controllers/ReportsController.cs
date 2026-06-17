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
    public class ReportsController : ControllerBase
    {
        private readonly IPlatformService _platform;

        public ReportsController(IPlatformService platform) => _platform = platform;

        [HttpPost]
        public async Task<IActionResult> Report([FromBody] ReportListingDto dto)
        {
            var ok = await _platform.ReportListingAsync(GetUserId(), dto);
            if (!ok)
                return BadRequest(ApiResponse.Fail("Bu ilanı zaten şikayet ettiniz."));
            return Ok(ApiResponse.Ok("Şikayetiniz alındı."));
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
