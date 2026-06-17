using AdvertisementApp.DataAccess.Entities;

namespace AdvertisementApp.API.Services
{
    public interface IJwtTokenService
    {
        Task<string> GenerateTokenAsync(AppUser user);
    }
}
