using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrowthController : ControllerBase
    {
        private readonly IGrowthService _growth;

        public GrowthController(IGrowthService growth) => _growth = growth;

        [HttpGet("listing-questions/{advertisementId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuestions(int advertisementId) =>
            Ok(ApiResponse<List<ListingQuestionDto>>.Ok(await _growth.GetListingQuestionsAsync(advertisementId)));

        [HttpPost("listing-questions")]
        [Authorize]
        public async Task<IActionResult> Ask([FromBody] CreateListingQuestionDto dto)
        {
            var q = await _growth.AskQuestionAsync(GetUserId(), dto);
            if (q == null) return BadRequest(ApiResponse.Fail("Soru gönderilemedi."));
            return Ok(ApiResponse<ListingQuestionDto>.Ok(q, "Sorunuz gönderildi."));
        }

        [HttpPost("listing-questions/{id:int}/answer")]
        [Authorize]
        public async Task<IActionResult> Answer(int id, [FromBody] AnswerListingQuestionDto dto)
        {
            var q = await _growth.AnswerQuestionAsync(GetUserId(), id, dto);
            if (q == null) return BadRequest(ApiResponse.Fail("Yanıt kaydedilemedi."));
            return Ok(ApiResponse<ListingQuestionDto>.Ok(q, "Yanıt kaydedildi."));
        }

        [HttpPost("follow/{sellerUserId:int}")]
        [Authorize]
        public async Task<IActionResult> Follow(int sellerUserId)
        {
            var ok = await _growth.FollowSellerAsync(GetUserId(), sellerUserId);
            return ok ? Ok(ApiResponse.Ok("Takip edildi.")) : BadRequest(ApiResponse.Fail("Takip edilemedi."));
        }

        [HttpDelete("follow/{sellerUserId:int}")]
        [Authorize]
        public async Task<IActionResult> Unfollow(int sellerUserId)
        {
            var ok = await _growth.UnfollowSellerAsync(GetUserId(), sellerUserId);
            return ok ? Ok(ApiResponse.Ok("Takipten çıkıldı.")) : BadRequest(ApiResponse.Fail("İşlem başarısız."));
        }

        [HttpGet("follow/{sellerUserId:int}/status")]
        [Authorize]
        public async Task<IActionResult> FollowStatus(int sellerUserId) =>
            Ok(ApiResponse<bool>.Ok(await _growth.IsFollowingSellerAsync(GetUserId(), sellerUserId)));

        [HttpGet("followed-sellers")]
        [Authorize]
        public async Task<IActionResult> FollowedSellers() =>
            Ok(ApiResponse<List<SellerFollowDto>>.Ok(await _growth.GetFollowedSellersAsync(GetUserId())));

        [HttpGet("store")]
        [Authorize]
        public async Task<IActionResult> GetStore()
        {
            var data = await _growth.GetStoreSettingsAsync(GetUserId());
            if (data == null) return NotFound(ApiResponse.Fail("Mağaza bulunamadı."));
            return Ok(ApiResponse<UpdateStoreSettingsDto>.Ok(data));
        }

        [HttpPut("store")]
        [Authorize]
        public async Task<IActionResult> UpdateStore([FromBody] UpdateStoreSettingsDto dto)
        {
            var ok = await _growth.UpdateStoreSettingsAsync(GetUserId(), dto);
            return ok ? Ok(ApiResponse.Ok("Mağaza ayarları kaydedildi.")) : BadRequest(ApiResponse.Fail("Kaydedilemedi."));
        }

        [HttpGet("earnings")]
        [Authorize]
        public async Task<IActionResult> Earnings() =>
            Ok(ApiResponse<SellerEarningsDto>.Ok(await _growth.GetSellerEarningsAsync(GetUserId())));

        [HttpGet("referral")]
        [Authorize]
        public async Task<IActionResult> Referral([FromServices] IConfiguration config)
        {
            var baseUrl = config["App:SiteUrl"] ?? "http://localhost:3000";
            var stats = await _growth.GetReferralStatsAsync(GetUserId(), baseUrl);
            if (stats == null) return NotFound(ApiResponse.Fail("Referans kodu bulunamadı."));
            return Ok(ApiResponse<ReferralStatsDto>.Ok(stats));
        }

        [HttpPost("coupons/validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponDto dto) =>
            Ok(ApiResponse<CouponValidationResultDto>.Ok(await _growth.ValidateCouponAsync(dto)));

        [HttpPost("import/csv")]
        [Authorize]
        public async Task<IActionResult> ImportCsv([FromBody] string csv)
        {
            var result = await _growth.ImportSellerAdvertisementsCsvAsync(csv, GetUserId());
            return Ok(ApiResponse<BulkImportResultDto>.Ok(result, $"{result.Created} ilan içe aktarıldı."));
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
