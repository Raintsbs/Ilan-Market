using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Helpers;
using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Dtos.AdvertisementDtos;
using AdvertisementApp.Dtos.Seo;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementApp.Business.Service
{
    public class SeoService : ISeoService
    {
        private readonly AdvertisementAppDbContext _db;
        private readonly ICategoryService _categories;
        private readonly IAppCacheService _cache;

        public SeoService(AdvertisementAppDbContext db, ICategoryService categories, IAppCacheService cache)
        {
            _db = db;
            _categories = categories;
            _cache = cache;
        }

        public async Task<SeoLandingDto?> ResolveLandingAsync(string citySlug, string? categoryPath)
        {
            var key = $"seo:landing:{citySlug}:{categoryPath ?? ""}";
            var cached = await _cache.GetAsync<SeoLandingDto>(key);
            if (cached != null) return cached;

            var result = await ResolveLandingCoreAsync(citySlug, categoryPath);
            if (result != null)
                await _cache.SetAsync(key, result, TimeSpan.FromMinutes(15));
            return result;
        }

        private async Task<SeoLandingDto?> ResolveLandingCoreAsync(string citySlug, string? categoryPath)
        {
            if (string.IsNullOrWhiteSpace(citySlug)) return null;

            var provinces = await _db.Provinces.AsNoTracking().ToListAsync();
            var city = provinces.FirstOrDefault(p =>
                string.Equals(SlugHelper.ToSlug(p.Name), citySlug.Trim(), StringComparison.OrdinalIgnoreCase));
            if (city == null) return null;

            var allCategories = await _db.Categories.AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => new { c.Id, c.Name, c.ParentId, c.Slug })
                .ToListAsync();

            int? categoryId = null;
            string? categoryName = null;
            var breadcrumbs = new List<SeoBreadcrumbDto>();
            var slugParts = string.IsNullOrWhiteSpace(categoryPath)
                ? Array.Empty<string>()
                : categoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (slugParts.Length > 0)
            {
                int? parentId = null;
                var pathSoFar = new List<string>();
                foreach (var part in slugParts)
                {
                    var match = allCategories.FirstOrDefault(c =>
                        c.ParentId == parentId &&
                        (string.Equals(c.Slug, part, StringComparison.OrdinalIgnoreCase)
                         || string.Equals(SlugHelper.ToSlug(c.Name), part, StringComparison.OrdinalIgnoreCase)));
                    if (match == null) return null;

                    pathSoFar.Add(match.Slug ?? SlugHelper.ToSlug(match.Name));
                    breadcrumbs.Add(new SeoBreadcrumbDto
                    {
                        Name = match.Name,
                        Slug = match.Slug ?? SlugHelper.ToSlug(match.Name),
                        Path = string.Join("/", pathSoFar),
                    });
                    categoryId = match.Id;
                    categoryName = match.Name;
                    parentId = match.Id;
                }
            }

            var filter = new AdvertisementFilterDto
            {
                Page = 1,
                PageSize = 1,
                City = city.Name,
                CategoryId = categoryId,
                Status = AdvertisementStatus.Approved,
            };

            var countQuery = _db.Advertisements.AsNoTracking()
                .Where(a => a.IsActive && a.Status == AdvertisementStatus.Approved);

            if (categoryId.HasValue)
            {
                var ids = await _categories.GetDescendantCategoryIdsAsync(categoryId.Value);
                countQuery = countQuery.Where(a => ids.Contains(a.CategoryId));
            }

            var cityName = city.Name;
            countQuery = countQuery.Where(a =>
                a.ListingDetailsJson != null && a.ListingDetailsJson.Contains(cityName));

            var total = await countQuery.CountAsync();

            return new SeoLandingDto
            {
                CitySlug = SlugHelper.ToSlug(city.Name),
                CityName = city.Name,
                CategoryId = categoryId,
                CategoryName = categoryName,
                CategoryPath = slugParts.Length > 0 ? string.Join("/", slugParts) : null,
                Breadcrumbs = breadcrumbs,
                TotalCount = total,
                ShouldIndex = total > 0,
            };
        }

        public async Task<List<SeoSitemapEntryDto>> GetSitemapEntriesAsync(int maxEntries = 500)
        {
            var cities = await _db.Provinces.AsNoTracking()
                .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
                .Take(20)
                .Select(p => new { p.Name })
                .ToListAsync();

            var categories = await _db.Categories.AsNoTracking()
                .Where(c => c.IsActive && c.ParentId != null)
                .Select(c => new { c.Id, c.Name, c.Slug, c.ParentId })
                .ToListAsync();

            var lookup = categories.ToLookup(c => c.ParentId);
            var paths = new List<(int Id, string Path)>();

            void Walk(int id, string slugPath)
            {
                foreach (var child in lookup[id].OrderBy(c => c.Name))
                {
                    var slug = child.Slug ?? SlugHelper.ToSlug(child.Name);
                    var path = string.IsNullOrEmpty(slugPath) ? slug : $"{slugPath}/{slug}";
                    paths.Add((child.Id, path));
                    Walk(child.Id, path);
                }
            }

            var roots = await _db.Categories.AsNoTracking()
                .Where(c => c.IsActive && c.ParentId == null)
                .Select(c => c.Id)
                .ToListAsync();
            foreach (var rootId in roots)
                Walk(rootId, "");

            var entries = new List<SeoSitemapEntryDto>();
            foreach (var city in cities)
            {
                var citySlug = SlugHelper.ToSlug(city.Name);
                foreach (var (_, catPath) in paths.Take(maxEntries / Math.Max(cities.Count, 1)))
                {
                    entries.Add(new SeoSitemapEntryDto { CitySlug = citySlug, CategoryPath = catPath });
                    if (entries.Count >= maxEntries) return entries;
                }
            }
            return entries;
        }
    }
}
