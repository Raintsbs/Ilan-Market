using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using AdvertisementApp.Dtos.Marketplace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/saved-searches")]
    [Authorize]
    public class SavedSearchesController : ControllerBase
    {
        private readonly ISavedSearchService _saved;

        public SavedSearchesController(ISavedSearchService saved) => _saved = saved;

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var list = await _saved.ListAsync(GetUserId());
            return Ok(ApiResponse<List<SavedSearchDto>>.Ok(list));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSavedSearchDto dto)
        {
            var created = await _saved.CreateAsync(GetUserId(), dto);
            return created == null
                ? BadRequest(ApiResponse.Fail("Kayıtlı arama oluşturulamadı."))
                : Ok(ApiResponse<SavedSearchDto>.Ok(created, "Arama kaydedildi."));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _saved.DeleteAsync(GetUserId(), id);
            return ok ? Ok(ApiResponse.Ok("Silindi.")) : NotFound(ApiResponse.Fail("Kayıt bulunamadı."));
        }
    }
}
