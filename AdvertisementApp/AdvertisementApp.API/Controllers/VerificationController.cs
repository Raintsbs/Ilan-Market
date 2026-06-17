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
    public class VerificationController : ControllerBase
    {
        private readonly IPlatformService _platform;
        private readonly IMediaStorageService _storage;

        public VerificationController(IPlatformService platform, IMediaStorageService storage)
        {
            _platform = platform;
            _storage = storage;
        }

        [HttpPost("submit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Submit([FromForm] string documentType, IFormFile document)
        {
            if (document == null || document.Length == 0)
                return BadRequest(ApiResponse.Fail("Belge dosyası gerekli."));

            var saved = await _storage.SaveImageAsync(document);
            if (!saved.Success || string.IsNullOrWhiteSpace(saved.Data))
                return BadRequest(ApiResponse.Fail(saved.Message ?? "Belge kaydedilemedi."));

            var row = await _platform.SubmitVerificationAsync(GetUserId(), documentType, saved.Data);
            if (row == null) return BadRequest(ApiResponse.Fail("Başvuru oluşturulamadı (bekleyen başvuru olabilir)."));
            return Ok(ApiResponse<VerificationRequestDto>.Ok(row, "Kimlik doğrulama başvurunuz alındı."));
        }

        [HttpGet("mine")]
        public async Task<IActionResult> Mine([FromServices] IGrowthService growth)
        {
            var row = await growth.GetMyVerificationAsync(GetUserId());
            if (row == null) return Ok(ApiResponse<VerificationRequestDto?>.Ok(null, "Başvuru yok."));
            return Ok(ApiResponse<VerificationRequestDto>.Ok(row));
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
