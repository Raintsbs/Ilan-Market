using System.Net.Http.Json;
using System.Text.Json;

namespace AdvertisementApp.API.Services
{
    public interface IExternalAuthService
    {
        Task<(string Email, string Subject, string? FirstName, string? LastName)?> VerifyGoogleTokenAsync(string idToken);
    }

    public class ExternalAuthService : IExternalAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ExternalAuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(string Email, string Subject, string? FirstName, string? LastName)?> VerifyGoogleTokenAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken)) return null;
            try
            {
                var client = _httpClientFactory.CreateClient();
                var res = await client.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}");
                if (!res.IsSuccessStatusCode) return null;
                var json = await res.Content.ReadFromJsonAsync<JsonElement>();
                var email = json.TryGetProperty("email", out var e) ? e.GetString() : null;
                var sub = json.TryGetProperty("sub", out var s) ? s.GetString() : null;
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sub)) return null;
                var given = json.TryGetProperty("given_name", out var g) ? g.GetString() : null;
                var family = json.TryGetProperty("family_name", out var f) ? f.GetString() : null;
                return (email, sub, given, family);
            }
            catch
            {
                return null;
            }
        }
    }
}
