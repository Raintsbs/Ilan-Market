using AdvertisementApp.Dtos.Marketplace;

namespace AdvertisementApp.Business.Interface
{
    public interface ILocationService
    {
        Task EnsureSeededAsync();
        Task<List<ProvinceDto>> GetProvincesAsync();
        Task<List<DistrictDto>> GetDistrictsAsync(int provinceId);
        Task<List<NeighborhoodDto>> GetNeighborhoodsAsync(int districtId);
    }
}
