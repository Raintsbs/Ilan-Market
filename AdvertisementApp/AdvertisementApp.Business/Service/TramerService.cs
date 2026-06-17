using System.Text.RegularExpressions;
using AdvertisementApp.Business.Interface;
using AdvertisementApp.Dtos.Marketplace;
using Microsoft.Extensions.Configuration;

namespace AdvertisementApp.Business.Service
{
    /// <summary>
    /// Tramer sorgusu — gerçek API anahtarı yoksa deterministik simülasyon döner.
    /// appsettings: Tramer:ApiUrl, Tramer:ApiKey
    /// </summary>
    public class TramerService : ITramerService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public TramerService(IConfiguration config, HttpClient http)
        {
            _config = config;
            _http = http;
        }

        public async Task<TramerQueryResult> QueryAsync(TramerQueryRequest request)
        {
            var plate = NormalizePlate(request.Plate);
            if (string.IsNullOrWhiteSpace(plate))
                throw new InvalidOperationException("Geçerli plaka girin.");

            var apiUrl = _config["Tramer:ApiUrl"]?.Trim();
            var apiKey = _config["Tramer:ApiKey"]?.Trim();

            if (!string.IsNullOrEmpty(apiUrl) && !string.IsNullOrEmpty(apiKey))
            {
                try
                {
                    // Gerçek entegrasyon noktası — sağlayıcıya göre uyarlanır
                    var response = await _http.GetAsync($"{apiUrl}?plate={Uri.EscapeDataString(plate)}&key={apiKey}");
                    if (response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        return new TramerQueryResult
                        {
                            Plate = plate,
                            Status = "Kayıt var",
                            Summary = body.Length > 500 ? body[..500] : body,
                            QueriedAt = DateTime.UtcNow,
                            IsSimulated = false,
                        };
                    }
                }
                catch
                {
                    // Simülasyona düş
                }
            }

            return Simulate(plate);
        }

        private static TramerQueryResult Simulate(string plate)
        {
            var hash = plate.Aggregate(0, (acc, c) => acc + c);
            var hasDamage = hash % 3 != 0;
            var count = hasDamage ? (hash % 4) + 1 : 0;
            var amount = hasDamage ? (hash % 50_000) + 3_000m : 0m;

            return new TramerQueryResult
            {
                Plate = plate,
                Status = hasDamage ? "Hasar kaydı var" : "Temiz",
                DamageCount = count,
                TotalDamageAmount = hasDamage ? amount : null,
                Summary = hasDamage
                    ? $"{count} adet hasar kaydı; tahmini toplam {amount:N0} TL (simülasyon)."
                    : "Tramer kaydında hasar bulunamadı (simülasyon).",
                QueriedAt = DateTime.UtcNow,
                IsSimulated = true,
            };
        }

        private static string NormalizePlate(string raw)
        {
            var cleaned = Regex.Replace(raw.ToUpperInvariant(), @"[^A-Z0-9]", "");
            return cleaned.Length >= 5 ? cleaned : "";
        }
    }
}
