namespace AdvertisementApp.API.Configuration
{
    public static class ProductionConfigurationValidator
    {
        private static readonly string[] DefaultJwtKeys =
        [
            "AdvertisementApp-Dev-Secret-Key-Min-32-Chars-Long!!",
            "CHANGE_ME_IN_PRODUCTION_MIN_32_CHARS!!",
            "CHANGE_ME_MIN_32_CHARS_RANDOM_SECRET_KEY!!",
        ];

        public static void Validate(IConfiguration config, IHostEnvironment env, ILogger logger)
        {
            if (!env.IsProduction()) return;

            var errors = new List<string>();

            var jwtKey = config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32 || DefaultJwtKeys.Contains(jwtKey))
                errors.Add("Jwt:Key — en az 32 karakter, ortam degiskeni Jwt__Key ile ayarlanmali.");

            if (string.IsNullOrWhiteSpace(config.GetConnectionString("DefaultConnection")))
                errors.Add("ConnectionStrings:DefaultConnection zorunlu.");

            if (string.IsNullOrWhiteSpace(config["App:FrontendUrl"]))
                errors.Add("App:FrontendUrl zorunlu (App__FrontendUrl).");

            if (string.IsNullOrWhiteSpace(config["App:ApiUrl"]))
                errors.Add("App:ApiUrl zorunlu (App__ApiUrl).");

            var corsOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            if (corsOrigins.Length == 0 || corsOrigins.All(string.IsNullOrWhiteSpace))
                errors.Add("Cors:AllowedOrigins en az bir origin icermeli.");

            var storageProvider = config["Storage:Provider"] ?? "Local";
            if (storageProvider.Equals("AzureBlob", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(config["Storage:AzureConnectionString"]))
                errors.Add("Storage:AzureConnectionString zorunlu (Storage__AzureConnectionString).");

            if (!config.GetValue("Email:Enabled", false))
                errors.Add("Email:Enabled production'da true olmali.");

            if (config.GetValue("Email:Enabled", false))
            {
                if (string.IsNullOrWhiteSpace(config["Email:SmtpHost"]))
                    errors.Add("Email:SmtpHost zorunlu (SMTP sunucusu).");
                if (string.IsNullOrWhiteSpace(config["Email:From"]))
                    errors.Add("Email:From zorunlu.");
            }

            if (config.GetValue("Sms:Provider", "dev").Equals("dev", StringComparison.OrdinalIgnoreCase))
                errors.Add("Sms:Provider production'da 'netgsm' olmali (Sms__Provider).");

            if (config.GetValue("Sms:Provider", "").Equals("netgsm", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(config["Sms:Netgsm:Usercode"]))
                    errors.Add("Sms:Netgsm:Usercode zorunlu.");
                if (string.IsNullOrWhiteSpace(config["Sms:Netgsm:Password"]))
                    errors.Add("Sms:Netgsm:Password zorunlu.");
                if (string.IsNullOrWhiteSpace(config["Sms:Netgsm:MsgHeader"]))
                    errors.Add("Sms:Netgsm:MsgHeader zorunlu.");
            }

            var stripe = config["Stripe:SecretKey"];
            var iyzico = config["Iyzico:ApiKey"];
            if (string.IsNullOrWhiteSpace(stripe) && string.IsNullOrWhiteSpace(iyzico))
                errors.Add("Stripe:SecretKey veya Iyzico:ApiKey yapilandirilmali (gercek odeme icin).");

            if (!string.IsNullOrWhiteSpace(iyzico)
                && (config["Iyzico:BaseUrl"] ?? "").Contains("sandbox", StringComparison.OrdinalIgnoreCase))
                errors.Add("Iyzico:BaseUrl production'da sandbox olmamali (https://api.iyzipay.com).");

            if (config.GetValue("Payments:AllowDemo", false))
                errors.Add("Payments:AllowDemo production'da false olmali.");

            if (config.GetValue("Seed:RunCategoryCatalog", true))
                logger.LogWarning("Seed:RunCategoryCatalog=true — yalnizca kategori sync icin gecici acin.");

            if (errors.Count == 0) return;

            var message = "Production yapilandirma hatasi:\n- " + string.Join("\n- ", errors);
            logger.LogCritical(message);
            throw new InvalidOperationException(message);
        }
    }
}
