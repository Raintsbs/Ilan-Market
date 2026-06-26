using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.DataAccess.Seed
{
    public static class AdPackageSeeder
    {
        private static readonly (string Code, string Name, decimal Price, int FeaturedDays)[] Defaults =
        {
            ("featured_7", "Öne çıkan 7 gün", 99m, 7),
            ("featured_30", "Öne çıkan 30 gün", 299m, 30),
        };

        public static async Task EnsureSeededAsync(AdvertisementAppDbContext db, ILogger logger)
        {
            if (await db.AdPackages.AnyAsync()) return;

            foreach (var (code, name, price, days) in Defaults)
            {
                db.AdPackages.Add(new AdPackage
                {
                    Code = code,
                    Name = name,
                    Price = price,
                    FeaturedDays = days,
                    IsActive = true,
                });
            }

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} ad packages.", Defaults.Length);
        }
    }
}
