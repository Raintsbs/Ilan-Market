using System.Text.RegularExpressions;

namespace AdvertisementApp.Business.Service
{
    /// <summary>Başlıktaki marka/anahtar kelimelerin seçilen kategoriyle uyumunu kontrol eder.</summary>
    public static class CategoryTitleValidationService
    {
        private static readonly Dictionary<string, string[]> BrandAllowedCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            ["asus"] = ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "monitör", "monitor", "tablet", "anakart", "gaming"],
            ["msi"] = ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "monitör", "gaming"],
            ["monster"] = ["bilgisayar", "laptop", "notebook", "pc", "gaming"],
            ["lenovo"] = ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "tablet"],
            ["hp"] = ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "yazıcı", "printer"],
            ["dell"] = ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "monitör"],
            ["acer"] = ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "monitör"],
            ["apple"] = ["bilgisayar", "laptop", "mac", "iphone", "telefon", "tablet", "elektronik"],
            ["macbook"] = ["bilgisayar", "laptop", "apple", "elektronik"],
            ["iphone"] = ["telefon", "apple", "elektronik", "tablet"],
            ["samsung"] = ["telefon", "tablet", "elektronik", "tv", "buzdolabı", "çamaşır", "beyaz"],
            ["xiaomi"] = ["telefon", "tablet", "elektronik", "robot", "süpürge"],
            ["huawei"] = ["telefon", "tablet", "elektronik"],
            ["oppo"] = ["telefon", "elektronik"],
            ["vivo"] = ["telefon", "elektronik"],
            ["realme"] = ["telefon", "elektronik"],
            ["lg"] = ["tv", "elektronik", "buzdolabı", "çamaşır", "beyaz"],
            ["sony"] = ["tv", "elektronik", "playstation", "konsol", "kulaklık", "kamera"],
            ["philips"] = ["tv", "elektronik", "kahve", "mutfak", "beyaz"],
            ["xbox"] = ["oyun", "konsol", "playstation", "elektronik"],
            ["playstation"] = ["oyun", "konsol", "ps", "elektronik"],
            ["ps5"] = ["oyun", "konsol", "playstation", "elektronik"],
            ["ps4"] = ["oyun", "konsol", "playstation", "elektronik"],
            ["nintendo"] = ["oyun", "konsol", "switch", "elektronik"],
            ["delonghi"] = ["kahve", "coffee", "mutfak", "espresso", "kahve makinesi"],
            ["nespresso"] = ["kahve", "coffee", "mutfak", "kahve makinesi"],
            ["arçelik"] = ["beyaz", "mutfak", "kahve", "çamaşır", "buzdolabı", "elektronik", "kahve makinesi"],
            ["beko"] = ["beyaz", "mutfak", "çamaşır", "buzdolabı", "elektronik"],
            ["bosch"] = ["mutfak", "kahve", "beyaz", "bulaşık", "çamaşır", "kahve makinesi"],
            ["siemens"] = ["mutfak", "beyaz", "bulaşık", "çamaşır", "buzdolabı"],
            ["vestel"] = ["tv", "beyaz", "elektronik", "buzdolabı"],
            ["tefal"] = ["mutfak", "tencere", "kahve", "elektrikli"],
            ["karaca"] = ["mutfak", "tencere", "kahve", "ev"],
            ["bmw"] = ["araç", "otomobil", "araba", "oto", "suv"],
            ["mercedes"] = ["araç", "otomobil", "araba", "oto"],
            ["audi"] = ["araç", "otomobil", "araba", "oto"],
            ["volkswagen"] = ["araç", "otomobil", "araba", "oto", "vw"],
            ["toyota"] = ["araç", "otomobil", "araba", "oto"],
            ["honda"] = ["araç", "otomobil", "araba", "oto", "motosiklet"],
            ["ford"] = ["araç", "otomobil", "araba", "oto"],
            ["renault"] = ["araç", "otomobil", "araba", "oto"],
            ["fiat"] = ["araç", "otomobil", "araba", "oto"],
            ["hyundai"] = ["araç", "otomobil", "araba", "oto"],
            ["kawasaki"] = ["motosiklet", "motor", "araç"],
            ["yamaha"] = ["motosiklet", "motor", "araç"],
        };

        private static readonly string[] BrandOrder = BrandAllowedCategories.Keys
            .OrderByDescending(k => k.Length)
            .ToArray();

        public static string? Validate(string title, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(categoryName))
                return null;

            var titleLower = title.ToLowerInvariant();
            var categoryLower = categoryName.ToLowerInvariant();

            foreach (var brand in BrandOrder)
            {
                if (!TitleContainsBrand(titleLower, brand))
                    continue;

                var allowed = BrandAllowedCategories[brand];
                var fits = allowed.Any(keyword =>
                    categoryLower.Contains(keyword, StringComparison.OrdinalIgnoreCase));

                if (!fits)
                {
                    var hint = string.Join(", ", allowed.Take(4));
                    return $"\"{brand}\" markası \"{categoryName}\" kategorisiyle uyumlu değil. Bu marka için uygun kategoriler: {hint}…";
                }
            }

            return null;
        }

        private static bool TitleContainsBrand(string titleLower, string brand)
        {
            var escaped = Regex.Escape(brand);
            return Regex.IsMatch(
                titleLower,
                $@"(^|[^a-z0-9çğıöşü]){escaped}([^a-z0-9çğıöşü]|$)",
                RegexOptions.IgnoreCase);
        }
    }
}
