using System.Text.Json;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.DataAccess.Seed
{
    public static class LocalAdvertisementsSeeder
    {
        private static readonly Dictionary<int, string> LocalCategoryNames = new()
        {
            [229] = "Telefon",
            [243] = "Bilgisayar",
            [323] = "Oyun & Konsol",
        };

        public static async Task EnsureImportedAsync(
            AdvertisementAppDbContext db,
            UserManager<AppUser> userManager,
            IConfiguration config,
            ILogger logger)
        {
            if (!config.GetValue("Seed:ImportLocalAds", false))
            {
                logger.LogInformation("Seed:ImportLocalAds=false — yerel ilan aktarımı atlandı.");
                return;
            }

            var path = ResolveSeedPath();
            if (!File.Exists(path))
            {
                logger.LogWarning("Local ads seed not found: {Path}", path);
                return;
            }

            var json = await File.ReadAllTextAsync(path);
            var items = JsonSerializer.Deserialize<List<LocalAdSeed>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            if (items == null || items.Count == 0) return;

            var categoryRows = await db.Categories.AsNoTracking()
                .Where(c => LocalCategoryNames.Values.Contains(c.Name))
                .ToListAsync();
            var categoryIdsByName = categoryRows
                .GroupBy(c => c.Name)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.ParentId != null).First().Id);

            var imported = 0;
            foreach (var item in items)
            {
                if (!LocalCategoryNames.TryGetValue(item.CategoryId, out var categoryName)
                    || !categoryIdsByName.TryGetValue(categoryName, out var targetCategoryId))
                {
                    logger.LogWarning("Kategori eşleşmedi (local id {CategoryId}, title {Title})",
                        item.CategoryId, item.Title);
                    continue;
                }

                var user = await EnsureUserAsync(userManager, logger, item.Email);
                if (user == null) continue;

                var exists = await db.Advertisements.AnyAsync(a =>
                    a.UserId == user.Id && a.Title == item.Title && a.CategoryId == targetCategoryId);
                if (exists) continue;

                db.Advertisements.Add(new Advertisement
                {
                    UserId = user.Id,
                    CategoryId = targetCategoryId,
                    Title = item.Title,
                    Description = item.Description,
                    Content = item.Content,
                    ListingDetailsJson = item.ListingDetailsJson,
                    ImagePath = item.ImagePath,
                    ImagePathsJson = item.ImagePathsJson,
                    VideoPath = item.VideoPath,
                    PanoramaPath = item.PanoramaPath,
                    ListingType = (ListingType)item.ListingType,
                    Status = item.IsActive ? AdvertisementStatus.Approved : (AdvertisementStatus)item.Status,
                    IsActive = item.IsActive,
                    CreatedTime = DateTime.UtcNow,
                });
                imported++;
            }

            if (imported > 0)
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Imported {Count} local advertisements from {Path}", imported, path);
            }
            else
            {
                logger.LogInformation("No new local advertisements to import from {Path}", path);
            }
        }

        private static async Task<AppUser?> EnsureUserAsync(UserManager<AppUser> userManager, ILogger logger, string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null) return user;

            user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = email.Split('@')[0],
                LastName = "User",
            };
            var result = await userManager.CreateAsync(user, "123456");
            if (!result.Succeeded)
            {
                logger.LogWarning("{Email} oluşturulamadı: {Errors}",
                    email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return null;
            }

            return user;
        }

        public static string ResolveSeedPath()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "scripts", "data", "seed-advertisements.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "scripts", "data", "seed-advertisements.json"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "scripts", "data", "seed-advertisements.json")),
            };
            return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
        }

        private sealed class LocalAdSeed
        {
            public string Email { get; set; } = "";
            public int CategoryId { get; set; }
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string Content { get; set; } = "";
            public string? ListingDetailsJson { get; set; }
            public string? ImagePath { get; set; }
            public string? ImagePathsJson { get; set; }
            public string? VideoPath { get; set; }
            public string? PanoramaPath { get; set; }
            public int ListingType { get; set; }
            public int Status { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
