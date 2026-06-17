using AdvertisementApp.Entities;

namespace AdvertisementApp.Dtos.AdvertisementDtos
{
    public class AdvertisementUpdateDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImagePath { get; set; }
        public string? ImagePathsJson { get; set; }
        public string? VideoPath { get; set; }
        public string? PanoramaPath { get; set; }
        public ListingType ListingType { get; set; } = ListingType.Standard;
        public string? ListingDetailsJson { get; set; }
        public string Content { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
