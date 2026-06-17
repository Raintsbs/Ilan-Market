using AdvertisementApp.Common.Result;
using Microsoft.AspNetCore.Http;

namespace AdvertisementApp.Business.Interface
{
    public interface IMediaStorageService : IImageStorageService
    {
        Task<IDataResult<string>> SaveVideoAsync(IFormFile? file);
        Task<IDataResult<string>> SavePanoramaAsync(IFormFile? file);
    }
}
