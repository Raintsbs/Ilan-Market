using System.ComponentModel.DataAnnotations;

namespace AdvertisementApp.UI.Models
{
    public class AdvertisementCreateViewModel
    {
        [Required(ErrorMessage = "Kategori seçiniz.")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [MaxLength(200)]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "İçerik zorunludur.")]
        [Display(Name = "İçerik")]
        public string Content { get; set; } = null!;

        [Display(Name = "Görsel yükle")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Görsel URL (opsiyonel)")]
        public string? ImagePath { get; set; }
    }
}
