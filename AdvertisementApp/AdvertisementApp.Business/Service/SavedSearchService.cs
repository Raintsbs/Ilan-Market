using AdvertisementApp.Business.Interface;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.Business.Service
{
    public class SavedSearchService : ISavedSearchService
    {
        private readonly AdvertisementAppDbContext _db;

        public SavedSearchService(AdvertisementAppDbContext db) => _db = db;

        public async Task<List<SavedSearchDto>> ListAsync(int userId)
        {
            var rows = await _db.SavedSearches.AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedTime)
                .ToListAsync();
            return rows.Select(Map).ToList();
        }

        public async Task<SavedSearchDto?> CreateAsync(int userId, CreateSavedSearchDto dto)
        {
            var name = dto.Name.Trim();
            if (string.IsNullOrEmpty(name)) return null;

            var entity = new SavedSearch
            {
                UserId = userId,
                Name = name.Length > 120 ? name[..120] : name,
                FilterJson = string.IsNullOrWhiteSpace(dto.FilterJson) ? "{}" : dto.FilterJson,
                NotifyOnNew = dto.NotifyOnNew,
                CreatedTime = DateTime.UtcNow,
            };
            _db.SavedSearches.Add(entity);
            await _db.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int userId, int id)
        {
            var entity = await _db.SavedSearches.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (entity == null) return false;
            _db.SavedSearches.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        private static SavedSearchDto Map(SavedSearch s) => new()
        {
            Id = s.Id,
            Name = s.Name,
            FilterJson = s.FilterJson,
            NotifyOnNew = s.NotifyOnNew,
            CreatedTime = s.CreatedTime,
        };
    }
}
