using AdvertisementApp.Common.Constants;
using AdvertisementApp.Common.Helpers;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Entities;
using AdvertisementApp.DataAccess.Extension;
using AdvertisementApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.DataAccess.Seed
{
    public static class DatabaseSeeder
    {
        public const string AdminEmail = "admin@advertisement.local";
        public const string AdminPassword = "Admin1234!";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AdvertisementAppDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

            var runOnStartup = config.GetValue("Seed:RunOnStartup", true);
            if (!runOnStartup)
            {
                logger.LogInformation("Seed:RunOnStartup=false — başlangıç seed atlandı.");
                return;
            }

            try
            {
                var provider = config["Database:Provider"] ?? "SqlServer";
                if (DependencyExtension.IsSqlite(provider, context.Database.GetConnectionString() ?? ""))
                {
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("SQLite EnsureCreated tamamlandı.");
                }
                else
                {
                    await context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Migration sırasında hata oluştu.");
                return;
            }

            foreach (var role in new[] { AppRoles.Admin, AppRoles.Moderator, AppRoles.User })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
            }

            var admin = await userManager.FindByEmailAsync(AdminEmail);
            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User"
                };
                var createResult = await userManager.CreateAsync(admin, AdminPassword);
                if (!createResult.Succeeded)
                {
                    logger.LogWarning("Admin kullanıcı oluşturulamadı: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(admin);
                var resetResult = await userManager.ResetPasswordAsync(admin, token, AdminPassword);
                if (!resetResult.Succeeded)
                {
                    logger.LogWarning("Admin şifresi güncellenemedi: {Errors}",
                        string.Join(", ", resetResult.Errors.Select(e => e.Description)));
                }
            }

            if (admin != null && !await userManager.IsInRoleAsync(admin, AppRoles.Admin))
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);

            await EnsureSeedUserAsync(
                userManager,
                logger,
                email: "tahatokay2006@gmail.com",
                password: "123456",
                firstName: "Taha",
                lastName: "Tokay",
                role: AppRoles.User);

            if (config.GetValue("Seed:RunCategoryCatalog", false))
            {
                await CategoryCatalogSeeder.SeedFullCatalogAsync(context, logger);
                await BackfillCategorySlugsAsync(context, logger);
            }
            else
            {
                logger.LogInformation(
                    "Seed:RunCategoryCatalog=false — kategori kataloğu atlandı. Güncellemek için Seed__RunCategoryCatalog=true.");
            }

            if (config.GetValue("Seed:RunLegalPages", true))
                await LegalPagesSeeder.EnsureAsync(context, logger);

            await AdPackageSeeder.EnsureSeededAsync(context, logger);
            await TurkeyLocationSeeder.EnsureSeededAsync(context, logger);

            await LocalAdvertisementsSeeder.EnsureImportedAsync(context, userManager, config, logger);
        }

        private static async Task EnsureSeedUserAsync(
            UserManager<AppUser> userManager,
            ILogger logger,
            string email,
            string password,
            string firstName,
            string lastName,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                };
                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    logger.LogWarning("{Email} oluşturulamadı: {Errors}",
                        email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return;
                }
            }
            else
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await userManager.ResetPasswordAsync(user, token, password);
                if (!resetResult.Succeeded)
                {
                    logger.LogWarning("{Email} şifresi güncellenemedi: {Errors}",
                        email, string.Join(", ", resetResult.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);
        }

        private static async Task BackfillCategorySlugsAsync(AdvertisementAppDbContext context, ILogger logger)
        {
            var categories = await context.Categories.Where(c => c.Slug == null || c.Slug == "").ToListAsync();
            if (categories.Count == 0) return;

            var used = await context.Categories.AsNoTracking()
                .Where(c => c.Slug != null && c.Slug != "")
                .Select(c => c.Slug!)
                .ToListAsync();
            var usedSet = new HashSet<string>(used, StringComparer.OrdinalIgnoreCase);

            foreach (var cat in categories)
            {
                var baseSlug = SlugHelper.ToSlug(cat.Name);
                var slug = baseSlug;
                var n = 2;
                while (usedSet.Contains(slug))
                    slug = $"{baseSlug}-{n++}";
                cat.Slug = slug;
                usedSet.Add(slug);
            }
            await context.SaveChangesAsync();
            logger.LogInformation("{Count} kategori slug güncellendi.", categories.Count);
        }
    }
}
