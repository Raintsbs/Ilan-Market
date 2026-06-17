namespace AdvertisementApp.Dtos.AuthDtos
{
    public class RegisterRequestDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? CaptchaToken { get; set; }
        public string? ReferralCode { get; set; }
    }
}
