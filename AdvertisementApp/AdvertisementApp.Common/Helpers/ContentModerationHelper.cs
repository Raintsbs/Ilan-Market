namespace AdvertisementApp.Common.Helpers
{
    public static class ContentModerationHelper
    {
        private static readonly string[] BlockedPatterns =
        [
            "viagra", "casino", "kumar", "btc giveaway", "click here now",
            "whatsapp +", "telegram @", "ücretsiz para", "free money",
        ];

        public static string? RejectReasonIfSpam(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var lower = text.ToLowerInvariant();
            foreach (var pattern in BlockedPatterns)
            {
                if (lower.Contains(pattern, StringComparison.Ordinal))
                    return "İçerik spam veya yasaklı anahtar kelime içeriyor.";
            }
            if (CountUrls(lower) > 5)
                return "Çok fazla bağlantı içeriyor.";
            return null;
        }

        private static int CountUrls(string text)
        {
            var count = 0;
            var idx = 0;
            while (idx >= 0)
            {
                var http = text.IndexOf("http", idx, StringComparison.Ordinal);
                var www = text.IndexOf("www.", idx, StringComparison.Ordinal);
                int next;
                if (http < 0 && www < 0) break;
                if (http < 0) next = www;
                else if (www < 0) next = http;
                else next = Math.Min(http, www);
                count++;
                idx = next + 4;
            }
            return count;
        }
    }
}
