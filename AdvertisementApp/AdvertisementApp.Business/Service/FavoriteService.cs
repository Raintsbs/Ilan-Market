using AdvertisementApp.Business.Interface;
using AdvertisementApp.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.Business.Service
{
    public class FavoriteService : IFavoriteService
    {
        private readonly AdvertisementAppDbContext _context;

        public FavoriteService(AdvertisementAppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsFavoriteAsync(int userId, int advertisementId)
        {
            try
            {
                return await _context.Favorites
                    .AnyAsync(f => f.UserId == userId && f.AdvertisementId == advertisementId);
            }
            catch
            {
                return false;
            }
        }
    }
}
