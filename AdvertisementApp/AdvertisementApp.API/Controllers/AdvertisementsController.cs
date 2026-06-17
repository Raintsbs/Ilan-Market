using AdvertisementApp.API.Extensions;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Business.Service;
using AdvertisementApp.Common.Constants;
using AdvertisementApp.Common.Helpers;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.AdvertisementDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("write")]
    public class AdvertisementsController : ControllerBase
    {
        private readonly IAdvertisementService _advertisementService;
        private readonly ICategoryService _categoryService;
        private readonly IMediaStorageService _mediaStorage;
        private readonly IPlatformService _platformService;
        private readonly ICaptchaService _captcha;

        public AdvertisementsController(
            IAdvertisementService advertisementService,
            ICategoryService categoryService,
            IMediaStorageService mediaStorage,
            IPlatformService platformService,
            ICaptchaService captcha)
        {
            _advertisementService = advertisementService;
            _categoryService = categoryService;
            _mediaStorage = mediaStorage;
            _platformService = platformService;
            _captcha = captcha;
        }

        [HttpGet]
        [AllowAnonymous]
        [DisableRateLimiting]
        public async Task<IActionResult> GetPaged(
            [FromQuery] AdvertisementFilterDto filter,
            [FromQuery] bool mine = false)
        {
            if (mine)
            {
                if (User.Identity?.IsAuthenticated != true)
                    return Unauthorized(ApiResponse.Fail("Bu liste için giriş gerekli."));
                filter.UserId = GetUserId();
            }

            var result = await _advertisementService.GetPagedAsync(filter);
            return this.ToActionResult(result, notFoundStatus: 400);
        }

        /// <summary>Eski istemciler için; tercih: GET /api/advertisements?mine=true</summary>
        [HttpGet("my")]
        [Authorize]
        [DisableRateLimiting]
        public Task<IActionResult> GetMyAds([FromQuery] AdvertisementFilterDto filter) =>
            GetPaged(filter, mine: true);

        [HttpGet("my/counts")]
        [Authorize]
        [DisableRateLimiting]
        public async Task<IActionResult> GetMyAdCounts()
        {
            var result = await _advertisementService.GetMyAdCountsAsync(GetUserId());
            return this.ToActionResult(result);
        }

        [HttpGet("{id:int}/phone")]
        [Authorize]
        [DisableRateLimiting]
        public async Task<IActionResult> RevealPhone(int id)
        {
            var result = await _advertisementService.RevealPhoneAsync(id, GetUserId());
            return this.ToActionResult(result, notFoundStatus: 400);
        }

        [HttpPost("{id:int}/bump")]
        [Authorize]
        public async Task<IActionResult> Bump(int id)
        {
            var result = await _advertisementService.BumpAdAsync(id, GetUserId());
            return this.ToActionResult(result, notFoundStatus: 400);
        }

        [HttpPost("{id:int}/extend")]
        [Authorize]
        public async Task<IActionResult> Extend(int id, [FromQuery] int days = 30)
        {
            var result = await _advertisementService.ExtendAdAsync(id, GetUserId(), days);
            return this.ToActionResult(result, notFoundStatus: 400);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [DisableRateLimiting]
        public async Task<IActionResult> GetById(int id)
        {
            int? viewerId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
            var result = await _advertisementService.GetPublicByIdAsync(id, viewerId);
            return this.ToActionResult(result);
        }

        [HttpGet("{id:int}/similar")]
        [AllowAnonymous]
        [DisableRateLimiting]
        public async Task<IActionResult> GetSimilar(int id, [FromQuery] int count = 4)
        {
            var ad = await _advertisementService.GetByIdAsync(id);
            if (!ad.Success || ad.Data == null)
                return this.ToActionResult(ad);

            count = count is < 1 or > 12 ? 4 : count;
            var similar = await _advertisementService.GetSimilarAsync(id, count);
            return this.ToActionResult(similar);
        }

        [HttpGet("{id:int}/price-history")]
        [AllowAnonymous]
        [DisableRateLimiting]
        public async Task<IActionResult> GetPriceHistory(int id)
        {
            var result = await _advertisementService.GetPriceHistoryAsync(id);
            return this.ToActionResult(result);
        }

        [HttpGet("batch")]
        [AllowAnonymous]
        [DisableRateLimiting]
        public async Task<IActionResult> GetBatch([FromQuery] string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return Ok(ApiResponse.Ok(new List<AdvertisementListDto>()));

            var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var n) ? n : 0)
                .Where(n => n > 0)
                .ToList();
            var result = await _advertisementService.GetByIdsAsync(idList);
            return this.ToActionResult(result);
        }

        [HttpPost("{id:int}/view")]
        [AllowAnonymous]
        public async Task<IActionResult> RecordView(int id)
        {
            await _platformService.RecordViewAsync(id);
            return Ok(ApiResponse.Ok("View recorded."));
        }

        /// <summary>Next.js için JSON body ile ilan oluşturma.</summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] AdvertisementCreateDto dto)
        {
            if (!await _captcha.VerifyAsync(dto.CaptchaToken, HttpContext.Connection.RemoteIpAddress?.ToString()))
                return BadRequest(ApiResponse.Fail("CAPTCHA doğrulaması başarısız."));

            dto.UserId = GetUserId();
            var cat = await _categoryService.GetByIdAsync(dto.CategoryId);
            if (cat.Success && cat.Data != null)
            {
                var warn = CategoryTitleValidationService.Validate(dto.Title, cat.Data.Name);
                if (warn != null)
                    return BadRequest(ApiResponse.Fail(warn));
            }
            var result = await _advertisementService.CreateReturningAsync(dto);
            return this.ToActionResult(result, notFoundStatus: 400);
        }

        /// <summary>Tek görsel (geriye dönük uyumluluk).</summary>
        [HttpPost("with-image")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateWithImage([FromForm] AdvertisementCreateDto dto, IFormFile? image)
        {
            var images = image != null ? new List<IFormFile> { image } : null;
            return await CreateWithImages(dto, images, null, null);
        }

        /// <summary>Çoklu görsel ile ilan oluşturma (form alanı: images).</summary>
        [HttpPost("with-images")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateWithImages(
            [FromForm] AdvertisementCreateDto dto,
            List<IFormFile>? images,
            IFormFile? video,
            IFormFile? panorama)
        {
            if (!await _captcha.VerifyAsync(dto.CaptchaToken, HttpContext.Connection.RemoteIpAddress?.ToString()))
                return BadRequest(ApiResponse.Fail("CAPTCHA doğrulaması başarısız."));

            dto.UserId = GetUserId();

            var cat = await _categoryService.GetByIdAsync(dto.CategoryId);
            if (cat.Success && cat.Data != null)
            {
                var warn = CategoryTitleValidationService.Validate(dto.Title, cat.Data.Name);
                if (warn != null)
                    return BadRequest(ApiResponse.Fail(warn));
            }

            var mediaError = await ApplyMediaAsync(dto, images, video, panorama);
            if (mediaError != null) return BadRequest(ApiResponse.Fail(mediaError));

            var result = await _advertisementService.CreateReturningAsync(dto);
            return this.ToActionResult(result, notFoundStatus: 400);
        }

        [HttpPost("validate-title")]
        [AllowAnonymous]
        public IActionResult ValidateTitle([FromBody] ValidateTitleRequest request)
        {
            var message = CategoryTitleValidationService.Validate(request.Title, request.CategoryName);
            return Ok(ApiResponse<string?>.Ok(message, message ?? ""));
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] AdvertisementUpdateDto dto)
        {
            var forbidden = await EnsureOwnerOrAdminAsync(id, dto);
            if (forbidden != null) return forbidden;

            dto.Id = id;
            var result = await _advertisementService.UpdateReturningAsync(dto);
            return this.ToActionResult(result, notFoundStatus: 400);
        }

        [HttpPut("{id:int}/with-image")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public Task<IActionResult> UpdateWithImage(int id, [FromForm] AdvertisementUpdateDto dto, IFormFile? image) =>
            UpdateWithImages(id, dto, image != null ? new List<IFormFile> { image } : null, null, null);

        [HttpPut("{id:int}/with-images")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateWithImages(
            int id,
            [FromForm] AdvertisementUpdateDto dto,
            List<IFormFile>? images,
            IFormFile? video,
            IFormFile? panorama)
        {
            var forbidden = await EnsureOwnerOrAdminAsync(id, dto);
            if (forbidden != null) return forbidden;

            dto.Id = id;
            var cat = await _categoryService.GetByIdAsync(dto.CategoryId);
            if (cat.Success && cat.Data != null)
            {
                var warn = CategoryTitleValidationService.Validate(dto.Title, cat.Data.Name);
                if (warn != null)
                    return BadRequest(ApiResponse.Fail(warn));
            }

            var paths = AdvertisementImagePathsHelper.Parse(dto.ImagePath, dto.ImagePathsJson);

            if (images != null && images.Count > 0)
            {
                foreach (var file in images.Take(Math.Max(0, 10 - paths.Count)))
                {
                    var img = await _mediaStorage.SaveImageAsync(file);
                    if (!img.Success)
                        return BadRequest(ApiResponse.Fail(img.Message));
                    if (!string.IsNullOrEmpty(img.Data))
                        paths.Add(img.Data);
                }
            }

            if (paths.Count > 0)
            {
                var (primary, json) = AdvertisementImagePathsHelper.ToStorage(paths);
                dto.ImagePath = primary;
                dto.ImagePathsJson = json;
            }
            else if (!string.IsNullOrWhiteSpace(dto.ImagePathsJson))
            {
                dto.ImagePath = null;
                dto.ImagePathsJson = "[]";
            }

            if (video != null)
            {
                var v = await _mediaStorage.SaveVideoAsync(video);
                if (!v.Success) return BadRequest(ApiResponse.Fail(v.Message));
                if (!string.IsNullOrEmpty(v.Data)) dto.VideoPath = v.Data;
            }
            if (panorama != null)
            {
                var p = await _mediaStorage.SavePanoramaAsync(panorama);
                if (!p.Success) return BadRequest(ApiResponse.Fail(p.Message));
                if (!string.IsNullOrEmpty(p.Data)) dto.PanoramaPath = p.Data;
            }

            var result = await _advertisementService.UpdateReturningAsync(dto);
            return this.ToActionResult(result, notFoundStatus: 400);
        }

        private async Task<string?> ApplyMediaAsync(
            AdvertisementCreateDto dto,
            List<IFormFile>? images,
            IFormFile? video,
            IFormFile? panorama)
        {
            var paths = new List<string>();
            if (images != null)
            {
                foreach (var file in images.Take(10))
                {
                    var img = await _mediaStorage.SaveImageAsync(file);
                    if (!img.Success) return img.Message;
                    if (!string.IsNullOrEmpty(img.Data)) paths.Add(img.Data);
                }
            }
            if (paths.Count > 0)
            {
                var (primary, json) = AdvertisementImagePathsHelper.ToStorage(paths);
                dto.ImagePath = primary;
                dto.ImagePathsJson = json;
            }
            if (video != null)
            {
                var v = await _mediaStorage.SaveVideoAsync(video);
                if (!v.Success) return v.Message;
                if (!string.IsNullOrEmpty(v.Data)) dto.VideoPath = v.Data;
            }
            if (panorama != null)
            {
                var p = await _mediaStorage.SavePanoramaAsync(panorama);
                if (!p.Success) return p.Message;
                if (!string.IsNullOrEmpty(p.Data)) dto.PanoramaPath = p.Data;
            }
            return null;
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _advertisementService.GetByIdAsync(id);
            if (!existing.Success || existing.Data == null)
                return this.ToActionResult(existing);

            if (!CanManage(existing.Data.UserId))
                return Forbid();

            var result = await _advertisementService.DeleteAsync(id);
            return this.ToActionResult(result);
        }

        private async Task<IActionResult?> EnsureOwnerOrAdminAsync(int id, AdvertisementUpdateDto dto)
        {
            var existing = await _advertisementService.GetByIdAsync(id);
            if (!existing.Success || existing.Data == null)
                return NotFound(ApiResponse.Fail(existing.Message));

            if (!CanManage(existing.Data.UserId))
                return Forbid();

            dto.UserId = existing.Data.UserId;
            return null;
        }

        private bool CanManage(int ownerUserId) =>
            GetUserId() == ownerUserId || User.IsInRole(AppRoles.Admin);

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
