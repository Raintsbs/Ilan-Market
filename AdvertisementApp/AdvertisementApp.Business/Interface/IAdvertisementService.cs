using AdvertisementApp.Common.Models;
using AdvertisementApp.Common.Result;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Entities;

namespace AdvertisementApp.Business.Interface
{
    public interface IAdvertisementService : IGenericService<Advertisement, AdvertisementListDto, AdvertisementCreateDto, AdvertisementUpdateDto>
    {
        Task<IDataResult<PagedResult<AdvertisementListDto>>> GetPagedAsync(AdvertisementFilterDto filter);
        Task<IDataResult<AdvertisementListDto>> CreateReturningAsync(AdvertisementCreateDto dto);
        Task<IDataResult<AdvertisementListDto>> GetPublicByIdAsync(int id, int? viewerUserId = null);
        Task<IDataResult<AdvertisementListDto>> UpdateReturningAsync(AdvertisementUpdateDto dto);
        Task<IDataResult<List<AdvertisementListDto>>> GetSimilarAsync(int excludeId, int count = 4);
        Task<IDataResult<List<PriceHistoryPointDto>>> GetPriceHistoryAsync(int advertisementId);
        Task<IDataResult<List<AdvertisementListDto>>> GetByIdsAsync(IReadOnlyList<int> ids);
        Task<IDataResult<MyAdCountsDto>> GetMyAdCountsAsync(int userId);
        Task<IDataResult<PhoneRevealDto>> RevealPhoneAsync(int advertisementId, int viewerUserId);
        Task<IResult> BumpAdAsync(int advertisementId, int userId);
        Task<IResult> ExtendAdAsync(int advertisementId, int userId, int days);
        Task<IResult> ApproveAsync(int id);
        Task<IResult> RejectAsync(int id);
    }
}
