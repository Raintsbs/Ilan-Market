using AdvertisementApp.Common.Result;

namespace AdvertisementApp.Business.Interface
{
    public interface IGenericService<TEntity, TListDto, TCreateDto, TUpdateDto>
        where TEntity : class
        where TListDto : class
        where TCreateDto : class
        where TUpdateDto : class
    {
        Task<IDataResult<List<TListDto>>> GetAllAsync();
        Task<IDataResult<TListDto>> GetByIdAsync(int id);
        Task<IResult> CreateAsync(TCreateDto dto);
        Task<IResult> UpdateAsync(TUpdateDto dto);
        Task<IResult> DeleteAsync(int id);
    }
}
