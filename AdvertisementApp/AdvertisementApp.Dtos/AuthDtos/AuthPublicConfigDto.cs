namespace AdvertisementApp.Dtos.AuthDtos
{
    public class AuthPublicConfigDto
    {
        public string GoogleClientId { get; set; } = "";
        public bool EmailEnabled { get; set; }
        public bool EmailUsesPickup { get; set; }
        public string StripePublishableKey { get; set; } = "";
        public bool StripeEnabled { get; set; }
        public bool IyzicoEnabled { get; set; }
        public string WebPushVapidPublicKey { get; set; } = "";
        public string CaptchaSiteKey { get; set; } = "";
        public bool CaptchaEnabled { get; set; }
    }
}
