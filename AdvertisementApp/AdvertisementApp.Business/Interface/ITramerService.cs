using AdvertisementApp.Dtos.Marketplace;

namespace AdvertisementApp.Business.Interface
{
    public interface ITramerService
    {
        Task<TramerQueryResult> QueryAsync(TramerQueryRequest request);
    }
}
