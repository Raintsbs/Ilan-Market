using AdvertisementApp.API.Extensions;
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
    public class AuctionsController : ControllerBase
    {
        private readonly IAuctionService _auctions;

        public AuctionsController(IAuctionService auctions) => _auctions = auctions;

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("by-ad/{advertisementId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByAd(int advertisementId)
        {
            await _auctions.CloseExpiredAsync();
            var result = await _auctions.GetByAdvertisementAsync(advertisementId);
            return result.Success
                ? Ok(ApiResponse<AuctionDto>.Ok(result.Data!))
                : NotFound(ApiResponse.Fail(result.Message));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateAuctionDto dto)
        {
            var result = await _auctions.CreateAsync(dto, GetUserId());
            return this.ToActionResult(result);
        }

        [HttpPost("{id:int}/bid")]
        [Authorize]
        public async Task<IActionResult> PlaceBid(int id, [FromBody] PlaceBidDto dto)
        {
            var result = await _auctions.PlaceBidAsync(id, GetUserId(), dto.Amount);
            return this.ToActionResult(result);
        }
    }
}
