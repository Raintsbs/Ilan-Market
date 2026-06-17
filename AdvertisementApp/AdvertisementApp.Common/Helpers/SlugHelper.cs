using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AdvertisementApp.Common.Helpers
{
    public static class SlugHelper
    {
        private static readonly Dictionary<char, string> TurkishMap = new()
        {
            ['ç'] = "c", ['Ç'] = "c",
            ['ğ'] = "g", ['Ğ'] = "g",
            ['ı'] = "i", ['I'] = "i", ['İ'] = "i",
            ['ö'] = "o", ['Ö'] = "o",
            ['ş'] = "s", ['Ş'] = "s",
            ['ü'] = "u", ['Ü'] = "u",
        };

        public static string ToSlug(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "kategori";

            var sb = new StringBuilder(text.Trim().Length);
            foreach (var ch in text.Trim())
            {
                if (TurkishMap.TryGetValue(ch, out var mapped))
                    sb.Append(mapped);
                else
                    sb.Append(ch);
            }

            var normalized = sb.ToString().Normalize(NormalizationForm.FormD);
            var ascii = new StringBuilder(normalized.Length);
            foreach (var ch in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    ascii.Append(ch);
            }

            var slug = ascii.ToString().ToLowerInvariant();
            slug = Regex.Replace(slug, @"[^a-z0-9\s\-/]", "");
            slug = Regex.Replace(slug, @"[\s/]+", "-");
            slug = Regex.Replace(slug, @"-+", "-").Trim('-');
            return string.IsNullOrEmpty(slug) ? "kategori" : slug;
        }
    }
}
