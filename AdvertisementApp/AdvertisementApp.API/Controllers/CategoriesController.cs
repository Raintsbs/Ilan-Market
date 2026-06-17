using AdvertisementApp.API.Extensions;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Constants;
using AdvertisementApp.Dtos.CategoryDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdvertisementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllAsync();
            return this.ToActionResult(result);
        }

        [HttpGet("tree")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTree() =>
            Ok(AdvertisementApp.Common.Models.ApiResponse<List<AdvertisementApp.Dtos.Marketplace.CategoryTreeDto>>.Ok(await _categoryService.GetTreeAsync()));

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            return this.ToActionResult(result);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var result = await _categoryService.CreateAsync(dto);
            return this.ToActionResult(result, notFoundStatus: 400);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            dto.Id = id;
            var result = await _categoryService.UpdateAsync(dto);
            return this.ToActionResult(result, notFoundStatus: 400);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return this.ToActionResult(result);
        }
    }
}
