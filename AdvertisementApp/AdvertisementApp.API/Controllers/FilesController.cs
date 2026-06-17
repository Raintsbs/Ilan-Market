using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IImageStorageService _imageStorageService;

        public FilesController(IImageStorageService imageStorageService)
        {
            _imageStorageService = imageStorageService;
        }

        /// <summary>Görsel yükle; dönen path ilan create/update body’sine ImagePath olarak eklenir.</summary>
        [HttpPost("upload")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var result = await _imageStorageService.SaveImageAsync(file);
            if (!result.Success)
                return BadRequest(ApiResponse<string>.Fail(result.Message));

            return Ok(ApiResponse<string>.Ok(result.Data ?? string.Empty, "Dosya yüklendi."));
        }
    }
}
