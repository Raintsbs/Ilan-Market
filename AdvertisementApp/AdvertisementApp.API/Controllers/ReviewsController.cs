using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.Marketplace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviews;

        public ReviewsController(IReviewService reviews) => _reviews = reviews;

        [HttpGet("seller/{sellerUserId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> SellerRating(int sellerUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            int? viewerId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
            return Ok(ApiResponse<SellerRatingSummaryDto>.Ok(
                await _reviews.GetSellerRatingAsync(sellerUserId, page, pageSize, viewerId)));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateSellerReview([FromBody] CreateSellerReviewDto dto)
        {
            var review = await _reviews.CreateSellerReviewAsync(GetUserId(), dto);
            if (review == null) return BadRequest(ApiResponse.Fail("Yorum kaydedilemedi."));
            return Ok(ApiResponse<SellerReviewDto>.Ok(review, "Yorumunuz kaydedildi."));
        }

        [HttpPut("seller/{reviewId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateSellerReview(int reviewId, [FromBody] UpdateReviewDto dto)
        {
            var review = await _reviews.UpdateSellerReviewAsync(GetUserId(), reviewId, dto);
            if (review == null) return BadRequest(ApiResponse.Fail("Güncellenemedi."));
            return Ok(ApiResponse<SellerReviewDto>.Ok(review, "Güncellendi."));
        }

        [HttpDelete("seller/{reviewId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteSellerReview(int reviewId)
        {
            var ok = await _reviews.DeleteSellerReviewAsync(GetUserId(), reviewId);
            return ok ? Ok(ApiResponse.Ok("Silindi.")) : BadRequest(ApiResponse.Fail("Silinemedi."));
        }

        [HttpGet("advertisement/{advertisementId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> AdvertisementRating(int advertisementId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            int? viewerId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
            var data = await _reviews.GetAdvertisementRatingAsync(advertisementId, viewerId, page, pageSize);
            return Ok(ApiResponse<AdvertisementRatingSummaryDto>.Ok(data));
        }

        [HttpPost("advertisement")]
        [Authorize]
        public async Task<IActionResult> CreateAdvertisementReview([FromBody] CreateAdvertisementReviewDto dto)
        {
            var review = await _reviews.CreateAdvertisementReviewAsync(GetUserId(), dto);
            if (review == null) return BadRequest(ApiResponse.Fail("Yorum kaydedilemedi."));
            return Ok(ApiResponse<AdvertisementReviewDto>.Ok(review, "Yorumunuz kaydedildi."));
        }

        [HttpPut("advertisement/{reviewId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateAdvertisementReview(int reviewId, [FromBody] UpdateReviewDto dto)
        {
            var review = await _reviews.UpdateAdvertisementReviewAsync(GetUserId(), reviewId, dto);
            if (review == null) return BadRequest(ApiResponse.Fail("Güncellenemedi."));
            return Ok(ApiResponse<AdvertisementReviewDto>.Ok(review, "Güncellendi."));
        }

        [HttpDelete("advertisement/{reviewId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteAdvertisementReview(int reviewId)
        {
            var ok = await _reviews.DeleteAdvertisementReviewAsync(GetUserId(), reviewId);
            return ok ? Ok(ApiResponse.Ok("Silindi.")) : BadRequest(ApiResponse.Fail("Silinemedi."));
        }

        [HttpPost("buyer")]
        [Authorize]
        public async Task<IActionResult> CreateBuyerReview([FromBody] CreateBuyerReviewDto dto)
        {
            var review = await _reviews.CreateBuyerReviewAsync(GetUserId(), dto);
            if (review == null) return BadRequest(ApiResponse.Fail("Yorum kaydedilemedi."));
            return Ok(ApiResponse<BuyerReviewDto>.Ok(review, "Yorumunuz kaydedildi."));
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
