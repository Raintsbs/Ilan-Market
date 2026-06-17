using System.Net.Http.Json;
using System.Text.Json;
using AdvertisementApp.Business.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.Business.Service
{
    public class CaptchaService : ICaptchaService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CaptchaService> _logger;

        public CaptchaService(
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            ILogger<CaptchaService> logger)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public bool IsEnabled =>
            _config.GetValue("Captcha:Enabled", false)
            && !string.IsNullOrWhiteSpace(_config["Captcha:SecretKey"]);

        public async Task<bool> VerifyAsync(string? token, string? remoteIp)
        {
            if (!IsEnabled) return true;
            if (string.IsNullOrWhiteSpace(token)) return false;

            var secret = _config["Captcha:SecretKey"]!.Trim();
            var provider = (_config["Captcha:Provider"] ?? "Turnstile").Trim();

            try
            {
                if (provider.Equals("Turnstile", StringComparison.OrdinalIgnoreCase)
                    || provider.Equals("Cloudflare", StringComparison.OrdinalIgnoreCase))
                {
                    var client = _httpClientFactory.CreateClient();
                    using var form = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["secret"] = secret,
                        ["response"] = token,
                        ["remoteip"] = remoteIp ?? "",
                    });
                    var res = await client.PostAsync(
                        "https://challenges.cloudflare.com/turnstile/v0/siteverify", form);
                    if (!res.IsSuccessStatusCode) return false;
                    var json = await res.Content.ReadFromJsonAsync<JsonElement>();
                    return json.TryGetProperty("success", out var ok) && ok.GetBoolean();
                }

                if (provider.Equals("reCAPTCHA", StringComparison.OrdinalIgnoreCase)
                    || provider.Equals("Recaptcha", StringComparison.OrdinalIgnoreCase))
                {
                    var client = _httpClientFactory.CreateClient();
                    var url =
                        $"https://www.google.com/recaptcha/api/siteverify?secret={Uri.EscapeDataString(secret)}&response={Uri.EscapeDataString(token)}";
                    if (!string.IsNullOrWhiteSpace(remoteIp))
                        url += $"&remoteip={Uri.EscapeDataString(remoteIp)}";
                    var res = await client.PostAsync(url, null);
                    if (!res.IsSuccessStatusCode) return false;
                    var json = await res.Content.ReadFromJsonAsync<JsonElement>();
                    return json.TryGetProperty("success", out var ok) && ok.GetBoolean();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CAPTCHA verification failed");
            }

            return false;
        }
    }
}
