using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.Seo;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/seo")]
    public class SeoController : ControllerBase
    {
        private readonly ISeoService _seo;

        public SeoController(ISeoService seo) => _seo = seo;

        [HttpGet("landing")]
        public async Task<IActionResult> Landing([FromQuery] string city, [FromQuery] string? categoryPath)
        {
            var result = await _seo.ResolveLandingAsync(city, categoryPath);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail("Sayfa bulunamadı."));
            return Ok(ApiResponse<SeoLandingDto>.Ok(result));
        }

        [HttpGet("sitemap-entries")]
        public async Task<IActionResult> SitemapEntries([FromQuery] int max = 500)
        {
            var entries = await _seo.GetSitemapEntriesAsync(max);
            return Ok(ApiResponse<List<SeoSitemapEntryDto>>.Ok(entries));
        }
    }
}
