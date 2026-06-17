namespace AdvertisementApp.Dtos.Seo
{
    public class SeoLandingDto
    {
        public string CitySlug { get; set; } = null!;
        public string CityName { get; set; } = null!;
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryPath { get; set; }
        public List<SeoBreadcrumbDto> Breadcrumbs { get; set; } = new();
        public int TotalCount { get; set; }
        public bool ShouldIndex { get; set; }
    }

    public class SeoBreadcrumbDto
    {
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Path { get; set; } = null!;
    }

    public class SeoSitemapEntryDto
    {
        public string CitySlug { get; set; } = null!;
        public string CategoryPath { get; set; } = null!;
    }
}
