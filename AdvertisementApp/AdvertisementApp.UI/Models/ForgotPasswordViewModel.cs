using System.ComponentModel.DataAnnotations;

namespace AdvertisementApp.UI.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = null!;
    }
}
