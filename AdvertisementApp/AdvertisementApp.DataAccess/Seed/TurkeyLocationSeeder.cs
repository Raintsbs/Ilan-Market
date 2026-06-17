using System.Text.Json;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.DataAccess.Seed
{
    public static class TurkeyLocationSeeder
    {
        public static async Task EnsureSeededAsync(AdvertisementAppDbContext db, ILogger logger)
        {
            if (await db.Provinces.AnyAsync()) return;

            var path = ResolveSeedPath();
            if (!File.Exists(path))
            {
                logger.LogWarning("Turkey locations seed not found: {Path}", path);
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
                db.Provinces.Add(province);

                if (!p.TryGetProperty("districts", out var districtsEl)) continue;
                foreach (var d in districtsEl.EnumerateArray())
                {
                    db.Districts.Add(new District
                    {
                        Id = d.GetProperty("id").GetInt32(),
                        ProvinceId = province.Id,
                        Name = d.GetProperty("name").GetString()!,
                    });
                }
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} provinces from {Path}", await db.Provinces.CountAsync(), path);
        }

        public static string ResolveSeedPath()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "scripts", "data", "turkey-locations.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "scripts", "data", "turkey-locations.json"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "scripts", "data", "turkey-locations.json")),
            };
            return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
        }
    }
}
