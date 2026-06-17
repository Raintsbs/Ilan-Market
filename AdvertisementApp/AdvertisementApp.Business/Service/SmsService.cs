using AdvertisementApp.Business.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.Business.Service
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmsService> _logger;
        private readonly IEmailService _email;
        private readonly IHostEnvironment _env;

        public SmsService(IConfiguration config, ILogger<SmsService> logger, IEmailService email, IHostEnvironment env)
        {
            _config = config;
            _logger = logger;
            _email = email;
            _env = env;
        }

        public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
        {
            var provider = _config["Sms:Provider"] ?? "dev";
            if (_env.IsProduction() && provider.Equals("dev", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Production ortamında Sms:Provider=dev kullanılamaz.");
                return false;
            }
            if (provider.Equals("dev", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("SMS (dev) → {Phone}: doğrulama kodu {Code}", phoneNumber, code);
                var devEmail = _config["Sms:DevNotifyEmail"];
                if (!string.IsNullOrWhiteSpace(devEmail))
                {
                    await _email.SendAsync(devEmail, "İlanMarket SMS (dev)",
                        $"<p>Telefon: {phoneNumber}</p><p>Kod: <strong>{code}</strong></p>");
                }
                return true;
            }

            if (provider.Equals("netgsm", StringComparison.OrdinalIgnoreCase))
                return await SendNetgsmAsync(phoneNumber, $"İlanMarket doğrulama kodunuz: {code}");

            _logger.LogWarning("SMS provider '{Provider}' not configured; code logged only.", provider);
            _logger.LogInformation("SMS → {Phone}: {Code}", phoneNumber, code);
            return true;
        }

        private async Task<bool> SendNetgsmAsync(string phoneNumber, string message)
        {
            var usercode = _config["Sms:Netgsm:Usercode"]?.Trim();
            var password = _config["Sms:Netgsm:Password"]?.Trim();
            var msgHeader = _config["Sms:Netgsm:MsgHeader"]?.Trim();
            if (string.IsNullOrEmpty(usercode) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(msgHeader))
            {
                _logger.LogError("Netgsm SMS credentials missing in configuration.");
                return false;
            }

            var gsm = NormalizeGsm(phoneNumber);
            if (gsm == null) return false;

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var url =
                $"https://api.netgsm.com.tr/sms/send/get?" +
                $"usercode={Uri.EscapeDataString(usercode)}&" +
                $"password={Uri.EscapeDataString(password)}&" +
                $"gsmno={Uri.EscapeDataString(gsm)}&" +
                $"message={Uri.EscapeDataString(message)}&" +
                $"msgheader={Uri.EscapeDataString(msgHeader)}";

            var response = await http.GetStringAsync(url);
            var ok = response.StartsWith("00", StringComparison.Ordinal) || response.Contains("queued", StringComparison.OrdinalIgnoreCase);
            if (!ok) _logger.LogWarning("Netgsm response: {Response}", response);
            return ok;
        }

        private static string? NormalizeGsm(string raw)
        {
            var digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("90") && digits.Length == 12) return digits;
            if (digits.StartsWith("0") && digits.Length == 11) return "9" + digits;
            if (digits.Length == 10) return "90" + digits;
            return digits.Length >= 10 ? digits : null;
        }
    }
}
