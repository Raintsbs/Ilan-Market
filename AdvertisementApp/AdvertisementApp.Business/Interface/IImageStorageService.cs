using AdvertisementApp.Common.Result;
using Microsoft.AspNetCore.Http;

namespace AdvertisementApp.Business.Interface
{
    public interface IImageStorageService
    {
        Task<IDataResult<string>> SaveImageAsync(IFormFile? file);
    }
}
