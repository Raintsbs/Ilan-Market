using System.ComponentModel.DataAnnotations;

namespace AdvertisementApp.UI.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Token { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni şifre")]
        public string Password { get; set; } = null!;

        [Compare(nameof(Password))]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre tekrar")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
