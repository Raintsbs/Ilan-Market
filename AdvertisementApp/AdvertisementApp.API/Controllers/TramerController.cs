using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Dtos.Marketplace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TramerController : ControllerBase
    {
        private readonly ITramerService _tramer;
        private readonly AdvertisementAppDbContext _db;

        public TramerController(ITramerService tramer, AdvertisementAppDbContext db)
        {
            _tramer = tramer;
            _db = db;
        }

        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] TramerQueryRequest request)
        {
            try
            {
                var result = await _tramer.QueryAsync(request);
                return Ok(ApiResponse<TramerQueryResult>.Ok(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpPost("save/{advertisementId:int}")]
        public async Task<IActionResult> SaveToAd(int advertisementId, [FromBody] TramerQueryRequest request)
        {
            var ad = await _db.Advertisements.FindAsync(advertisementId);
            if (ad == null) return NotFound(ApiResponse.Fail("İlan bulunamadı."));

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            if (ad.UserId != userId) return Forbid();

            var result = await _tramer.QueryAsync(request);
            ad.TramerResultJson = JsonSerializer.Serialize(result);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<TramerQueryResult>.Ok(result, "Tramer sonucu ilana kaydedildi."));
        }
    }
}
