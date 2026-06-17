using System.Text.Json;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Dtos.Marketplace;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.Business.Service
{
    public class LocationService : ILocationService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LocationService> _logger;

        public LocationService(
            AdvertisementAppDbContext db,
            IHttpClientFactory httpClientFactory,
            ILogger<LocationService> logger)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task EnsureSeededAsync()
        {
            if (await _db.Provinces.AnyAsync()) return;

            var path = ResolveSeedPath();
            if (!File.Exists(path))
            {
                _logger.LogWarning("Turkey locations seed not found: {Path}", path);
                return;
            }

            var json = await File.ReadAllTextAsync(path);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("provinces", out var provincesEl)) return;

            foreach (var p in provincesEl.EnumerateArray())
            {
                var province = new Province
                {
                    Id = p.GetProperty("id").GetInt32(),
                    Name = p.GetProperty("name").GetString()!,
                    PlateCode = p.TryGetProperty("plate", out var plate) ? plate.GetString() : null,
                    SortOrder = p.GetProperty("id").GetInt32(),
                };
                _db.Provinces.Add(province);

                if (!p.TryGetProperty("districts", out var districtsEl)) continue;
                foreach (var d in districtsEl.EnumerateArray())
                {
                    _db.Districts.Add(new District
                    {
                        Id = d.GetProperty("id").GetInt32(),
                        ProvinceId = province.Id,
                        Name = d.GetProperty("name").GetString()!,
                    });
                }
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} provinces from {Path}", await _db.Provinces.CountAsync(), path);
        }

        public async Task<List<ProvinceDto>> GetProvincesAsync() =>
            await _db.Provinces.AsNoTracking()
                .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
                .Select(p => new ProvinceDto { Id = p.Id, Name = p.Name, PlateCode = p.PlateCode })
                .ToListAsync();

        public async Task<List<DistrictDto>> GetDistrictsAsync(int provinceId) =>
            await _db.Districts.AsNoTracking()
                .Where(d => d.ProvinceId == provinceId)
                .OrderBy(d => d.Name)
                .Select(d => new DistrictDto { Id = d.Id, ProvinceId = d.ProvinceId, Name = d.Name })
                .ToListAsync();

        public async Task<List<NeighborhoodDto>> GetNeighborhoodsAsync(int districtId)
        {
            var cached = await _db.Neighborhoods.AsNoTracking()
                .Where(n => n.DistrictId == districtId)
                .OrderBy(n => n.Name)
                .Select(n => new NeighborhoodDto { Id = n.Id, DistrictId = n.DistrictId, Name = n.Name })
                .ToListAsync();

            if (cached.Count > 0) return cached;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync($"https://turkiyeapi.dev/api/v1/districts/{districtId}");
                using var doc = JsonDocument.Parse(response);
                if (!doc.RootElement.TryGetProperty("data", out var data)) return cached;
                if (!data.TryGetProperty("neighborhoods", out var neighborhoods)) return cached;

                var entities = new List<Neighborhood>();
                foreach (var n in neighborhoods.EnumerateArray())
                {
                    entities.Add(new Neighborhood
                    {
                        Id = n.GetProperty("id").GetInt32(),
                        DistrictId = districtId,
                        Name = n.GetProperty("name").GetString()!,
                    });
                }

                if (entities.Count == 0) return cached;

                _db.Neighborhoods.AddRange(entities);
                await _db.SaveChangesAsync();
                return entities.Select(n => new NeighborhoodDto { Id = n.Id, DistrictId = n.DistrictId, Name = n.Name }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Neighborhood fetch failed for district {DistrictId}", districtId);
                return cached;
            }
        }

        private static string ResolveSeedPath()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "scripts", "data", "turkey-locations.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "scripts", "data", "turkey-locations.json"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "scripts", "data", "turkey-locations.json")),
                @"c:\Projects\advertisement\AdvertisementApp\scripts\data\turkey-locations.json",
            };
            return candidates.FirstOrDefault(File.Exists) ?? candidates[^1];
        }
    }
}
