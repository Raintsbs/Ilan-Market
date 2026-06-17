namespace AdvertisementApp.Business.Service
{
    public static class CargoCarrierUrls
    {
        public static readonly IReadOnlyDictionary<string, string> Names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["yurtici"] = "Yurtiçi Kargo",
            ["aras"] = "Aras Kargo",
            ["mng"] = "MNG Kargo",
            ["ptt"] = "PTT Kargo",
            ["surat"] = "Sürat Kargo",
        };

        public static string GetTrackingUrl(string carrierCode, string trackingNumber)
        {
            var n = Uri.EscapeDataString(trackingNumber.Trim());
            return carrierCode.ToLowerInvariant() switch
            {
                "yurtici" => $"https://www.yurticikargo.com/tr/online-servisler/gonderi-sorgula?code={n}",
                "aras" => $"https://www.araskargo.com.tr/tr/cargo_tracking_detail.aspx?kargo_takip_no={n}",
                "mng" => $"https://www.mngkargo.com.tr/gonderi-takip?q={n}",
                "ptt" => $"https://gonderitakip.ptt.gov.tr/",
                "surat" => $"https://www.suratkargo.com.tr/KargoTakip/?kargotakipno={n}",
                _ => $"https://www.google.com/search?q={n}+kargo+takip",
            };
        }

        public static string GetCarrierName(string code) =>
            Names.TryGetValue(code, out var name) ? name : code;
    }
}
