using AdvertisementApp.Dtos.Marketplace;

namespace AdvertisementApp.Business.Interface
{
    public interface ISavedSearchService
    {
        Task<List<SavedSearchDto>> ListAsync(int userId);
        Task<SavedSearchDto?> CreateAsync(int userId, CreateSavedSearchDto dto);
        Task<bool> DeleteAsync(int userId, int id);
    }
}
