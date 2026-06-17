using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ContentController : ControllerBase
    {
        private readonly IPlatformService _platform;

        public ContentController(IPlatformService platform) => _platform = platform;

        [HttpGet("blog")]
        public async Task<IActionResult> BlogList()
        {
            var items = await _platform.GetPublishedBlogPostsAsync();
            return Ok(ApiResponse.Ok(items));
        }

        [HttpGet("blog/{slug}")]
        public async Task<IActionResult> BlogDetail(string slug)
        {
            var post = await _platform.GetPublishedBlogPostBySlugAsync(slug);
            if (post == null) return NotFound(ApiResponse.Fail("Yazı bulunamadı."));
            return Ok(ApiResponse.Ok(post));
        }

        [HttpGet("pages")]
        public async Task<IActionResult> PageList()
        {
            var items = await _platform.GetActiveStaticPagesAsync();
            return Ok(ApiResponse.Ok(items));
        }

        [HttpGet("pages/{slug}")]
        public async Task<IActionResult> PageDetail(string slug)
        {
            var page = await _platform.GetActiveStaticPageBySlugAsync(slug);
            if (page == null) return NotFound(ApiResponse.Fail("Sayfa bulunamadı."));
            return Ok(ApiResponse.Ok(page));
        }
    }
}
