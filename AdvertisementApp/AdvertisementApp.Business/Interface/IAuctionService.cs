using AdvertisementApp.Common.Result;
using AdvertisementApp.Dtos.Marketplace;

namespace AdvertisementApp.Business.Interface
{
    public interface IAuctionService
    {
        Task<IDataResult<AuctionDto>> CreateAsync(CreateAuctionDto dto, int userId);
        Task<IDataResult<AuctionDto>> GetByAdvertisementAsync(int advertisementId);
        Task<IDataResult<AuctionDto>> PlaceBidAsync(int auctionId, int userId, decimal amount);
        Task CloseExpiredAsync();
    }
}
