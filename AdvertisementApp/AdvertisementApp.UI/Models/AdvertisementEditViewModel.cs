using System.ComponentModel.DataAnnotations;

namespace AdvertisementApp.UI.Models
{
    public class AdvertisementEditViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = null!;

        [Required]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = null!;

        [Required]
        [Display(Name = "İçerik")]
        public string Content { get; set; } = null!;

        [Display(Name = "Yeni görsel")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Mevcut görsel yolu")]
        public string? ImagePath { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }
    }
}
