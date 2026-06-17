using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("write")]
    public class OffersController : ControllerBase
    {
        private readonly IPlatformService _platform;

        public OffersController(IPlatformService platform) => _platform = platform;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOfferDto dto)
        {
            var offer = await _platform.CreateOfferAsync(GetUserId(), dto);
            if (offer == null)
                return BadRequest(ApiResponse.Fail("Teklif oluşturulamadı."));
            return Ok(ApiResponse<OfferDto>.Ok(offer, "Teklif gönderildi."));
        }

        [HttpGet("ad/{advertisementId:int}")]
        [DisableRateLimiting]
        public async Task<IActionResult> ForAd(int advertisementId, [FromQuery] bool asOwner = false)
        {
            var items = await _platform.GetOffersForAdAsync(GetUserId(), advertisementId, asOwner);
            return Ok(ApiResponse<List<OfferDto>>.Ok(items));
        }

        [HttpGet("incoming")]
        [DisableRateLimiting]
        public async Task<IActionResult> Incoming()
        {
            var items = await _platform.GetIncomingOffersForSellerAsync(GetUserId());
            return Ok(ApiResponse<List<OfferDto>>.Ok(items));
        }

        [HttpPost("{id:int}/accept")]
        public async Task<IActionResult> Accept(int id)
        {
            var offer = await _platform.RespondToOfferAsync(GetUserId(), id, accept: true);
            if (offer == null)
                return BadRequest(ApiResponse.Fail("Teklif kabul edilemedi."));
            return Ok(ApiResponse<OfferDto>.Ok(offer, "Teklif kabul edildi."));
        }

        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var offer = await _platform.RespondToOfferAsync(GetUserId(), id, accept: false);
            if (offer == null)
                return BadRequest(ApiResponse.Fail("Teklif reddedilemedi."));
            return Ok(ApiResponse<OfferDto>.Ok(offer, "Teklif reddedildi."));
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
