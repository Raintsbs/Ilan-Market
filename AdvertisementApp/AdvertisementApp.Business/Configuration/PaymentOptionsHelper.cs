using Microsoft.Extensions.Configuration;

namespace AdvertisementApp.Business.Configuration
{
    public static class PaymentOptionsHelper
    {
        public static bool IsStripeConfigured(IConfiguration config) =>
            !string.IsNullOrWhiteSpace(config["Stripe:SecretKey"]);

        public static bool IsIyzicoConfigured(IConfiguration config) =>
            !string.IsNullOrWhiteSpace(config["Iyzico:ApiKey"]) &&
            !string.IsNullOrWhiteSpace(config["Iyzico:SecretKey"]);

        public static bool IsRealPaymentConfigured(IConfiguration config) =>
            IsStripeConfigured(config) || IsIyzicoConfigured(config);

        public static string DemoPaymentHint(IConfiguration config)
        {
            if (IsRealPaymentConfigured(config))
                return "Ödeme sağlayıcısı yapılandırıldı ancak checkout başlatılamadı. API loglarını kontrol edin.";

            return "Demo ödeme tamamlandı (gerçek para çekilmedi). "
                   + "Gerçek ödeme için AdvertisementApp.API/appsettings.Development.local.json dosyasına "
                   + "Iyzico sandbox anahtarlarını ekleyin (Iyzico:ApiKey, Iyzico:SecretKey).";
        }
    }
}
