using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locations;

        public LocationsController(ILocationService locations) => _locations = locations;

        [HttpGet("provinces")]
        public async Task<IActionResult> Provinces()
        {
            await _locations.EnsureSeededAsync();
            return Ok(ApiResponse<List<AdvertisementApp.Dtos.Marketplace.ProvinceDto>>.Ok(await _locations.GetProvincesAsync()));
        }

        [HttpGet("provinces/{provinceId:int}/districts")]
        public async Task<IActionResult> Districts(int provinceId)
        {
            await _locations.EnsureSeededAsync();
            return Ok(ApiResponse<List<AdvertisementApp.Dtos.Marketplace.DistrictDto>>.Ok(await _locations.GetDistrictsAsync(provinceId)));
        }

        [HttpGet("districts/{districtId:int}/neighborhoods")]
        public async Task<IActionResult> Neighborhoods(int districtId) =>
            Ok(ApiResponse<List<AdvertisementApp.Dtos.Marketplace.NeighborhoodDto>>.Ok(await _locations.GetNeighborhoodsAsync(districtId)));
    }
}
