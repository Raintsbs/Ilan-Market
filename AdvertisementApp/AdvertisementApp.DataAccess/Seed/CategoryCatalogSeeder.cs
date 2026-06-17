using AdvertisementApp.Common.Helpers;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.DataAccess.Seed
{
    public static class CategoryCatalogSeeder
    {
        public static async Task SeedFullCatalogAsync(AdvertisementAppDbContext context, ILogger logger)
        {
            var added = 0;

            async Task<int?> EnsureAsync(string name, int? parentId, int sort, string? schema)
            {
                var existing = await context.Categories
                    .FirstOrDefaultAsync(c => c.Name == name && c.ParentId == parentId);
                if (existing != null)
                {
                    var changed = false;
                    if (!existing.IsActive)
                    {
                        existing.IsActive = true;
                        changed = true;
                    }
                    if (existing.SortOrder != sort)
                    {
                        existing.SortOrder = sort;
                        changed = true;
                    }
                    if (existing.FieldSchemaJson != schema)
                    {
                        existing.FieldSchemaJson = schema;
                        changed = true;
                    }
                    if (string.IsNullOrWhiteSpace(existing.Slug))
                    {
                        existing.Slug = SlugHelper.ToSlug(name);
                        changed = true;
                    }
                    if (changed)
                        await context.SaveChangesAsync();
                    return existing.Id;
                }

                context.Categories.Add(new Category
                {
                    Name = name,
                    ParentId = parentId,
                    SortOrder = sort,
                    Slug = SlugHelper.ToSlug(name),
                    FieldSchemaJson = schema,
                    IsActive = true,
                    CreatedTime = DateTime.Now,
                });
                await context.SaveChangesAsync();
                added++;
                return (await context.Categories
                    .Where(c => c.Name == name && c.ParentId == parentId)
                    .Select(c => (int?)c.Id)
                    .FirstOrDefaultAsync());
            }

            async Task WalkAsync(CategoryCatalogSeed.Node node, int? parentId)
            {
                if (node.Name == "_brands")
                {
                    foreach (var brand in node.Children)
                        await WalkAsync(brand, parentId);
                    return;
                }

                var id = await EnsureAsync(node.Name, parentId, node.Sort, node.Schema);
                if (node.Children.Length == 0) return;
                foreach (var child in node.Children)
                    await WalkAsync(child, id);
            }

            await MergePhoneAndDuplicateRootsAsync(context, logger);
            await MigrateVasitaCategoryNamesAsync(context, logger);

            foreach (var root in CategoryCatalogSeed.Roots)
                await WalkAsync(root, null);

            await MigrateLegacyAracRootAsync(context, logger);
            await CleanupStrayRootCategoriesAsync(context, logger);
            await PurgeLegacyInactiveRootsAsync(context, logger);
            await MergePhoneAndDuplicateRootsAsync(context, logger);
            await MergeYedekParcaRootNameAsync(context, logger);
            await RestructureBilgisayarAsync(context, logger);

            logger.LogInformation("Kategori kataloğu senkronize edildi. {Added} yeni kategori eklendi.", added);
        }

        private static readonly HashSet<string> BilgisayarValidL2 = new(StringComparer.OrdinalIgnoreCase)
        {
            "Dizüstü (Laptop)", "Masaüstü", "Oyuncu Bilgisayarı", "Monitör", "Bilgisayar Bileşenleri",
        };

        private static readonly HashSet<string> PcBrandNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Apple", "Dell", "HP", "Lenovo", "Asus", "Acer", "MSI", "Monster", "Casper", "Huawei", "Diğer",
        };

        private static async Task PurgeLegacyInactiveRootsAsync(AdvertisementAppDbContext context, ILogger logger)
        {
            var allowed = CategoryCatalogSeed.Roots.Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var legacyRoots = await context.Categories
                .Where(c => c.ParentId == null && !allowed.Contains(c.Name))
                .ToListAsync();

            if (legacyRoots.Count == 0) return;

            var elektronikId = await context.Categories
                .Where(c => c.ParentId == null && c.Name == "Elektronik")
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();

            var vasitaId = await context.Categories
                .Where(c => c.ParentId == null && c.Name == "Vasıta")
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();

            var legacyMap = new Dictionary<string, (int? ParentId, string TargetName)>(StringComparer.OrdinalIgnoreCase)
            {
                ["Telefon"] = (elektronikId, "Telefon"),
                ["Cep Telefonu"] = (elektronikId, "Telefon"),
                ["Bilgisayar"] = (elektronikId, "Bilgisayar"),
                ["Tv"] = (elektronikId, "Televizyon"),
                ["Televizyon"] = (elektronikId, "Televizyon"),
                ["Oyun Konsolu"] = (elektronikId, "Oyun & Konsol"),
                ["Kahve makinesi"] = (elektronikId, "Küçük Ev Aletleri"),
                ["Araç"] = (vasitaId, "Otomobil"),
            };

            foreach (var root in legacyRoots)
            {
                if (legacyMap.TryGetValue(root.Name, out var map) && map.ParentId.HasValue)
                {
                    var target = await context.Categories
                        .FirstOrDefaultAsync(c => c.ParentId == map.ParentId && c.Name == map.TargetName);

                    if (target != null)
                    {
                        var ads = await context.Advertisements.Where(a => a.CategoryId == root.Id).ToListAsync();
                        foreach (var ad in ads)
                            ad.CategoryId = target.Id;

                        var children = await context.Categories.Where(c => c.ParentId == root.Id).ToListAsync();
                        foreach (var child in children)
                        {
                            if (!await context.Categories.AnyAsync(c => c.ParentId == target.Id && c.Name == child.Name))
                                child.ParentId = target.Id;
                        }
                    }
                }

                var hasChildren = await context.Categories.AnyAsync(c => c.ParentId == root.Id);
                var hasAds = await context.Advertisements.AnyAsync(a => a.CategoryId == root.Id);

                if (!hasChildren && !hasAds)
                {
                    context.Categories.Remove(root);
                    logger.LogInformation("Eski kök kategori silindi: {Name} (Id={Id})", root.Name, root.Id);
                }
            }

            await context.SaveChangesAsync();
        }

        /// <summary>Elektronik altında karşılığı olan eski kök kopyalarını siler.</summary>
        private static async Task ForceRemoveKnownLegacyRootsAsync(AdvertisementAppDbContext context, ILogger logger)
        {
            var elektronikId = await context.Categories
                .Where(c => c.ParentId == null && c.Name == "Elektronik")
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();
            if (elektronikId == null) return;

            var duplicateRoots = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Telefon"] = "Telefon",
                ["Cep Telefonu"] = "Telefon",
                ["Bilgisayar"] = "Bilgisayar",
                ["Televizyon"] = "Televizyon",
                ["Oyun Konsolu"] = "Oyun & Konsol",
            };

            foreach (var (legacyName, targetName) in duplicateRoots)
            {
                var hasTarget = await context.Categories
                    .AnyAsync(c => c.ParentId == elektronikId && c.Name == targetName);
                if (!hasTarget) continue;

                var legacyRoots = await context.Categories
                    .Where(c => c.ParentId == null && c.Name == legacyName)
                    .ToListAsync();

                foreach (var root in legacyRoots)
                {
                    if (await context.Advertisements.AnyAsync(a => a.CategoryId == root.Id)) continue;
                    if (await context.Categories.AnyAsync(c => c.ParentId == root.Id)) continue;

                    context.Categories.Remove(root);
                    logger.LogInformation("Yinelenen eski kök silindi: {Name} (Id={Id})", root.Name, root.Id);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task RestructureBilgisayarAsync(AdvertisementAppDbContext context, ILogger logger)
        {
            var elektronikId = await context.Categories
                .Where(c => c.ParentId == null && c.Name == "Elektronik")
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();
            if (elektronikId == null) return;

            var bilgisayar = await context.Categories
                .FirstOrDefaultAsync(c => c.ParentId == elektronikId && c.Name == "Bilgisayar");
            if (bilgisayar == null) return;

            var dizustu = await context.Categories
                .FirstOrDefaultAsync(c => c.ParentId == bilgisayar.Id && c.Name == "Dizüstü (Laptop)");

            var stray = await context.Categories
                .Where(c => c.ParentId == bilgisayar.Id && !BilgisayarValidL2.Contains(c.Name))
                .ToListAsync();

            if (stray.Count == 0) return;

            foreach (var node in stray)
            {
                if (!PcBrandNames.Contains(node.Name) || dizustu == null)
                {
                    if (!await context.Advertisements.AnyAsync(a => a.CategoryId == node.Id) &&
                        !await context.Categories.AnyAsync(c => c.ParentId == node.Id))
                    {
                        context.Categories.Remove(node);
                        logger.LogInformation("Bilgisayar altında hatalı düğüm kaldırıldı: {Name}", node.Name);
                    }
                    continue;
                }

                var target = await context.Categories
                    .FirstOrDefaultAsync(c => c.ParentId == dizustu.Id && c.Name == node.Name);

                if (target == null)
                {
                    node.ParentId = dizustu.Id;
                    logger.LogInformation("Marka Dizüstü altına taşındı: {Name}", node.Name);
                    continue;
                }

                var children = await context.Categories.Where(c => c.ParentId == node.Id).ToListAsync();
                foreach (var child in children)
                {
                    if (!await context.Categories.AnyAsync(c => c.ParentId == target.Id && c.Name == child.Name))
                        child.ParentId = target.Id;
                }

                if (!await context.Advertisements.AnyAsync(a => a.CategoryId == node.Id))
                {
                    context.Categories.Remove(node);
                    logger.LogInformation("Yinelenen marka kaldırıldı: {Name}", node.Name);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task CleanupStrayRootCategoriesAsync(AdvertisementAppDbContext context, ILogger logger)
        {
            var allowed = CategoryCatalogSeed.Roots.Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var strayRoots = await context.Categories
                .Where(c => c.ParentId == null && !allowed.Contains(c.Name))
                .ToListAsync();

            if (strayRoots.Count == 0) return;

            var elektronikId = await context.Categories
                .Where(c => c.ParentId == null && c.Name == "Elektronik")
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();

            var legacyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Telefon"] = "Telefon",
                ["Cep Telefonu"] = "Telefon",
                ["Bilgisayar"] = "Bilgisayar",
                ["Tv"] = "Televizyon",
                ["Kahve makinesi"] = "Küçük Ev Aletleri",
                ["Oyun Konsolu"] = "Oyun & Konsol",
                ["Araç"] = "Vasıta",
            };

            foreach (var stray in strayRoots)
            {
                var hasAds = await context.Advertisements.AnyAsync(a => a.CategoryId == stray.Id);
                var children = await context.Categories.Where(c => c.ParentId == stray.Id).ToListAsync();

                if (children.Count > 0 && elektronikId.HasValue &&
                    legacyMap.TryGetValue(stray.Name, out var targetName))
                {
                    var target = await context.Categories
                        .FirstOrDefaultAsync(c => c.ParentId == elektronikId && c.Name == targetName);
                    if (target != null)
                    {
                        foreach (var child in children)
                        {
                            if (!await context.Categories.AnyAsync(c => c.ParentId == target.Id && c.Name == child.Name))
                                child.ParentId = target.Id;
                        }
                        await context.SaveChangesAsync();
                    }
                }

                if (hasAds)
                {
                    stray.IsActive = false;
                    continue;
                }

                var remaining = await context.Categories.CountAsync(c => c.ParentId == stray.Id);
                if (remaining == 0)
                {
                    context.Categories.Remove(stray);
                    logger.LogInformation("Hatalı kök kategori kaldırıldı: {Name}", stray.Name);
                }
                else
                {
                    stray.IsActive = false;
                    logger.LogInformation("Hatalı kök kategori pasifleştirildi: {Name}", stray.Name);
                }
            }

            await context.SaveChangesAsync();
        }

        /// <summary>Telefon / Cep Telefonu birleştirme ve yanlış kök kopyalarını temizleme.</summary>
        private static async Task MergePhoneAndDuplicateRootsAsync(AdvertisementAppDbContext context, ILogger logger)
        {
            var elektronikId = await context.Categories
                .Where(c => c.ParentId == null && c.Name == "Elektronik")
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();
            if (elektronikId == null) return;

            var telefon = await context.Categories
                .FirstOrDefaultAsync(c => c.ParentId == elektronikId && c.Name == "Telefon");
            var cep = await context.Categories
                .FirstOrDefaultAsync(c => c.ParentId == elektronikId && c.Name == "Cep Telefonu");

            if (cep != null && telefon == null)
            {
                cep.Name = "Telefon";
                cep.Slug = SlugHelper.ToSlug("Telefon");
                logger.LogInformation("Elektronik > Cep Telefonu, Telefon olarak güncellendi.");
            }
            else if (cep != null && telefon != null && cep.Id != telefon.Id)
            {
                await MergeCategoryTreeAsync(context, telefon.Id, cep.Id);
                cep.Name = "Telefon";
                cep.Slug = SlugHelper.ToSlug("Telefon");
                if (!await context.Advertisements.AnyAsync(a => a.CategoryId == telefon.Id) &&
                    !await context.Categories.AnyAsync(c => c.ParentId == telefon.Id))
                {
                    context.Categories.Remove(telefon);
                    logger.LogInformation("Yinelenen Elektronik > Telefon kaldırıldı.");
                }
            }

            var catalogUnderElektronik = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Telefon"] = "Telefon",
                ["Cep Telefonu"] = "Telefon",
                ["Bilgisayar"] = "Bilgisayar",
                ["Televizyon"] = "Televizyon",
                ["Tv"] = "Televizyon",
                ["Tablet"] = "Tablet",
                ["Oyun Konsolu"] = "Oyun & Konsol",
                ["Oyun & Konsol"] = "Oyun & Konsol",
            };

            foreach (var (strayName, targetName) in catalogUnderElektronik)
            {
                var target = await context.Categories
                    .FirstOrDefaultAsync(c => c.ParentId == elektronikId && c.Name == targetName);
                if (target == null) continue;

                var strayRoots = await context.Categories
                    .Where(c => c.ParentId == null && c.Name == strayName)
                    .ToListAsync();

                foreach (var root in strayRoots)
                {
                    await MergeCategoryTreeAsync(context, root.Id, target.Id);

                    if (!await context.Categories.AnyAsync(c => c.ParentId == root.Id))
                    {
                        var ads = await context.Advertisements.Where(a => a.CategoryId == root.Id).ToListAsync();
                        foreach (var ad in ads)
                            ad.CategoryId = target.Id;

                        if (!await context.Advertisements.AnyAsync(a => a.CategoryId == root.Id))
                        {
                            context.Categories.Remove(root);
                            logger.LogInformation(
                                "Yanlış kök kategori birleştirildi: {Name} (Id={Id}) → Elektronik > {Target}",
                                root.Name, root.Id, targetName);
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task MergeCategoryTreeAsync(AdvertisementAppDbContext context, int sourceId, int targetId)
        {
            if (sourceId == targetId) return;

            var children = await context.Categories.Where(c => c.ParentId == sourceId).ToListAsync();
            foreach (var child in children)
            {
                var match = await context.Categories
                    .FirstOrDefaultAsync(c => c.ParentId == targetId && c.Name == child.Name);

                if (match == null)
                {
                    child.ParentId = targetId;
                    continue;
                }

                await MergeCategoryTreeAsync(context, child.Id, match.Id);
                if (!await context.Categories.AnyAsync(c => c.ParentId == child.Id) &&
                    !await context.Advertisements.AnyAsync(a => a.CategoryId == child.Id))
                {
                    context.Categories.Remove(child);
                }
            }
        }

        private static async Task MergeYedekParcaRootNameAsync(AdvertisementAppDbContext context, ILogger logger)
        {
            var oldRoot = await context.Categories
                .FirstOrDefaultAsync(c => c.ParentId == null && c.Name == "Yedek Parça & Aksesuar");
            var newRoot = await context.Categories
                .FirstOrDefaultAsync(c => c.ParentId == null && c.Name == "Yedek Parça, Aksesuar & Tuning");

            if (oldRoot != null && newRoot == null)
            {
                oldRoot.Name = "Yedek Parça, Aksesuar & Tuning";
                oldRoot.Slug = SlugHelper.ToSlug(oldRoot.Name);
                logger.LogInformation("Yedek parça kök kategorisi sahibinden adına güncellendi.");
                await context.SaveChangesAsync();
            }
            else if (oldRoot != null && newRoot != null && oldRoot.Id != newRoot.Id)
            {
                await MergeCategoryTreeAsync(context, oldRoot.Id, newRoot.Id);
                if (!await context.Advertisements.AnyAsync(a => a.CategoryId == oldRoot.Id) &&
                    !await context.Categories.AnyAsync(c => c.ParentId == oldRoot.Id))
                {
                    context.Categories.Remove(oldRoot);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task MigrateVasitaCategoryNamesAsync(AdvertisementAppDbContext context, ILogger logger)
        {
            var vasitaId = await context.Categories
                .Where(c => c.ParentId == null && c.Name == "Vasıta")
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();
            if (vasitaId == null) return;

            var renames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["SUV & Pick-up"] = "Arazi, SUV & Pick-up",
                ["Arazi Aracı"] = "Arazi, SUV & Pick-up",
                ["Ticari Araç"] = "Ticari Araçlar",
                ["Deniz Aracı"] = "Deniz Araçları",
                ["Kiralık Araç"] = "Kiralık Araçlar",
                ["Hasarlı Araç"] = "Hasarlı Araçlar",
                ["Elektrikli Araç"] = "Elektrikli Araçlar",
                ["Karavan & Çekici"] = "Karavan",
                ["ATV & Off-road"] = "ATV",
            };

            foreach (var (oldName, newName) in renames)
            {
                var oldCat = await context.Categories
                    .FirstOrDefaultAsync(c => c.ParentId == vasitaId && c.Name == oldName);
                if (oldCat == null) continue;

                var newCat = await context.Categories
                    .FirstOrDefaultAsync(c => c.ParentId == vasitaId && c.Name == newName);

                if (newCat == null)
                {
                    oldCat.Name = newName;
                    oldCat.Slug = SlugHelper.ToSlug(newName);
                    logger.LogInformation("Vasıta kategorisi yeniden adlandırıldı: {Old} → {New}", oldName, newName);
                    continue;
                }

                if (oldCat.Id == newCat.Id) continue;

                var children = await context.Categories.Where(c => c.ParentId == oldCat.Id).ToListAsync();
                foreach (var child in children)
                {
                    if (!await context.Categories.AnyAsync(c => c.ParentId == newCat.Id && c.Name == child.Name))
                        child.ParentId = newCat.Id;
                }

                var ads = await context.Advertisements.Where(a => a.CategoryId == oldCat.Id).ToListAsync();
                foreach (var ad in ads)
                    ad.CategoryId = newCat.Id;

                if (!await context.Categories.AnyAsync(c => c.ParentId == oldCat.Id))
                    context.Categories.Remove(oldCat);
            }

            await context.SaveChangesAsync();
        }

        private static async Task MigrateLegacyAracRootAsync(AdvertisementAppDbContext context, ILogger logger)
        {
            var arac = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Araç" && c.ParentId == null);
            var vasita = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Vasıta" && c.ParentId == null);
            if (arac == null || vasita == null || arac.Id == vasita.Id) return;

            var aracChildren = await context.Categories.Where(c => c.ParentId == arac.Id).ToListAsync();
            foreach (var child in aracChildren)
            {
                var duplicate = await context.Categories.AnyAsync(c =>
                    c.ParentId == vasita.Id && c.Name == child.Name);
                if (duplicate)
                {
                    // Alt ağacı Vasıta altındaki eşleşene taşı
                    var target = await context.Categories.FirstAsync(c => c.ParentId == vasita.Id && c.Name == child.Name);
                    var grandchildren = await context.Categories.Where(c => c.ParentId == child.Id).ToListAsync();
                    foreach (var gc in grandchildren)
                    {
                        if (!await context.Categories.AnyAsync(c => c.ParentId == target.Id && c.Name == gc.Name))
                        {
                            gc.ParentId = target.Id;
                        }
                    }
                    context.Categories.Remove(child);
                }
                else
                {
                    child.ParentId = vasita.Id;
                }
            }

            var aracAds = await context.Advertisements.CountAsync(a => a.CategoryId == arac.Id);
            if (aracAds == 0 && !await context.Categories.AnyAsync(c => c.ParentId == arac.Id))
            {
                context.Categories.Remove(arac);
                logger.LogInformation("Eski 'Araç' kök kategorisi kaldırıldı (Vasıta'ya taşındı).");
            }

            await context.SaveChangesAsync();
        }
    }
}
