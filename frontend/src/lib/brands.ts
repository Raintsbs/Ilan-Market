/**
 * Marka → uygun kategori anahtar kelimeleri.
 * Başlıkta marka geçiyorsa kategori adında bu kelimelerden biri olmalı.
 */
export const BRAND_CATEGORY_RULES: Record<string, string[]> = {
  asus: ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "monitör", "monitor", "tablet", "anakart", "gaming"],
  msi: ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "monitör", "gaming"],
  monster: ["bilgisayar", "laptop", "notebook", "pc", "gaming"],
  lenovo: ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "tablet"],
  hp: ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "yazıcı", "printer"],
  dell: ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "monitör"],
  acer: ["bilgisayar", "laptop", "notebook", "pc", "elektronik", "monitör"],
  apple: ["bilgisayar", "laptop", "mac", "iphone", "telefon", "tablet", "elektronik"],
  macbook: ["bilgisayar", "laptop", "apple", "elektronik"],
  iphone: ["telefon", "apple", "elektronik", "tablet"],
  samsung: ["telefon", "tablet", "elektronik", "tv", "buzdolabı", "çamaşır", "beyaz"],
  xiaomi: ["telefon", "tablet", "elektronik", "robot", "süpürge"],
  huawei: ["telefon", "tablet", "elektronik"],
  oppo: ["telefon", "elektronik"],
  vivo: ["telefon", "elektronik"],
  realme: ["telefon", "elektronik"],
  lg: ["tv", "elektronik", "buzdolabı", "çamaşır", "beyaz"],
  sony: ["tv", "elektronik", "playstation", "konsol", "kulaklık", "kamera"],
  philips: ["tv", "elektronik", "kahve", "mutfak", "beyaz"],
  xbox: ["oyun", "konsol", "playstation", "elektronik"],
  playstation: ["oyun", "konsol", "ps", "elektronik"],
  ps5: ["oyun", "konsol", "playstation", "elektronik"],
  ps4: ["oyun", "konsol", "playstation", "elektronik"],
  nintendo: ["oyun", "konsol", "switch", "elektronik"],
  delonghi: ["kahve", "coffee", "mutfak", "espresso", "kahve makinesi"],
  nespresso: ["kahve", "coffee", "mutfak", "kahve makinesi"],
  arçelik: ["beyaz", "mutfak", "kahve", "çamaşır", "buzdolabı", "elektronik", "kahve makinesi"],
  beko: ["beyaz", "mutfak", "çamaşır", "buzdolabı", "elektronik"],
  bosch: ["mutfak", "kahve", "beyaz", "bulaşık", "çamaşır", "kahve makinesi"],
  siemens: ["mutfak", "beyaz", "bulaşık", "çamaşır", "buzdolabı"],
  vestel: ["tv", "beyaz", "elektronik", "buzdolabı"],
  tefal: ["mutfak", "tencere", "kahve", "elektrikli"],
  karaca: ["mutfak", "tencere", "kahve", "ev"],
  bmw: ["araç", "otomobil", "araba", "oto", "suv"],
  mercedes: ["araç", "otomobil", "araba", "oto"],
  audi: ["araç", "otomobil", "araba", "oto"],
  volkswagen: ["araç", "otomobil", "araba", "oto", "vw"],
  toyota: ["araç", "otomobil", "araba", "oto"],
  honda: ["araç", "otomobil", "araba", "oto", "motosiklet"],
  ford: ["araç", "otomobil", "araba", "oto"],
  renault: ["araç", "otomobil", "araba", "oto"],
  fiat: ["araç", "otomobil", "araba", "oto"],
  hyundai: ["araç", "otomobil", "araba", "oto"],
  kawasaki: ["motosiklet", "motor", "araç"],
  yamaha: ["motosiklet", "motor", "araç"],
};

const BRAND_ORDER = Object.keys(BRAND_CATEGORY_RULES).sort((a, b) => b.length - a.length);

function titleContainsBrand(titleLower: string, brand: string): boolean {
  const escaped = brand.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  return new RegExp(`(^|[^a-z0-9çğıöşü])${escaped}([^a-z0-9çğıöşü]|$)`, "i").test(titleLower);
}

export function validateTitleCategory(title: string, categoryName: string): string | null {
  if (!title.trim() || !categoryName.trim()) return null;

  const titleLower = title.toLowerCase();
  const categoryLower = categoryName.toLowerCase();

  for (const brand of BRAND_ORDER) {
    if (!titleContainsBrand(titleLower, brand)) continue;
    const allowed = BRAND_CATEGORY_RULES[brand];
    const fits = allowed.some((k) => categoryLower.includes(k));
    if (!fits) {
      const hint = allowed.slice(0, 4).join(", ");
      return `"${brand}" markası "${categoryName}" kategorisiyle uyumlu değil. Bu marka için uygun kategoriler: ${hint}…`;
    }
  }
  return null;
}
