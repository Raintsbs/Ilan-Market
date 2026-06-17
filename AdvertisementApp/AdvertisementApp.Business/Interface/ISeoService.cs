using AdvertisementApp.Dtos.Seo;

namespace AdvertisementApp.Business.Interface
{
    public interface ISeoService
    {
        Task<SeoLandingDto?> ResolveLandingAsync(string citySlug, string? categoryPath);
        Task<List<SeoSitemapEntryDto>> GetSitemapEntriesAsync(int maxEntries = 500);
    }
}
