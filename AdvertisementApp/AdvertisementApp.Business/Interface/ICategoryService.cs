using AdvertisementApp.Dtos.CategoryDtos;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Entities;

namespace AdvertisementApp.Business.Interface
{
    public interface ICategoryService : IGenericService<Category, CategoryListDto, CategoryCreateDto, CategoryUpdateDto>
    {
        Task<List<CategoryTreeDto>> GetTreeAsync();
        Task<List<int>> GetDescendantCategoryIdsAsync(int categoryId);
    }
}
