/** Kategori adına göre ilan formu ve detay alanları */

export type CategoryKind =
  | "computer"
  | "phone"
  | "tablet"
  | "coffee"
  | "tv"
  | "gaming"
  | "vehicle"
  | "estate"
  | "appliance"
  | "furniture"
  | "clothing"
  | "electronics"
  | "job"
  | "service"
  | "other";

export interface CategoryProfile {
  kind: CategoryKind;
  label: string;
  productSectionTitle: string;
  brandPlaceholder: string;
  modelPlaceholder: string;
  showYear: boolean;
  showColor: boolean;
  showSwap: boolean;
  showComputer: boolean;
  showPhone: boolean;
  showTablet: boolean;
  showCoffee: boolean;
  showTv: boolean;
  showGaming: boolean;
  showVehicle: boolean;
  showEstate: boolean;
  showAppliance: boolean;
  showFurniture: boolean;
  showClothing: boolean;
  showJob: boolean;
  showService: boolean;
}

const MATCHERS: { kind: CategoryKind; patterns: string[] }[] = [
  { kind: "coffee", patterns: ["kahve", "espresso", "coffee", "nespresso", "barista"] },
  {
    kind: "phone",
    patterns: [
      "telefon", "iphone", "cep telefon", "akıllı telefon", "mobile",
      "galaxy s", "galaxy a", "galaxy z", "galaxy note", "redmi", "poco", "oneplus", "nova", "reno",
    ],
  },
  { kind: "tablet", patterns: ["tablet", "ipad", "galaxy tab", "matepad", "surface pro", "surface go", "yoga tab"] },
  {
    kind: "computer",
    patterns: [
      "bilgisayar", "laptop", "notebook", "pc", "macbook", "monitör", "monitor", "ekran kart", "anakart",
      "thinkpad", "ideapad", "inspiron", "xps", "latitude", "alienware", "rog", "legion", "omen", "mac mini", "imac",
    ],
  },
  { kind: "tv", patterns: ["tv", "televizyon", "oled", "smart tv", "qled", "bravia", "ambilight", "nanocell"] },
  {
    kind: "gaming",
    patterns: ["oyun", "konsol", "playstation", "ps5", "ps4", "xbox", "nintendo", "switch", "steam key", "gamepad"],
  },
  { kind: "vehicle", patterns: ["araç", "araba", "otomobil", "oto ", "suv", "motosiklet", "motor ", "kamyon"] },
  { kind: "estate", patterns: ["emlak", "konut", "daire", "ev ", "arsa", "iş yeri", "bina", "villa", "kiralık"] },
  {
    kind: "appliance",
    patterns: [
      "buzdolab", "çamaşır", "bulaşık", "fırın", "ocak", "klima", "süpürge", "robot süpürge",
      "mikrodalga", "ütü", "beyaz eşya", "kurutma", "davlumbaz", "ankastre",
    ],
  },
  { kind: "furniture", patterns: ["mobilya", "koltuk", "kanepe", "masa", "sandalye", "yatak", "dolap", "sehpa"] },
  { kind: "clothing", patterns: ["giyim", "ayakkabı", "çanta", "aksesuar", "saat", "takı", "mont", "elbise"] },
  { kind: "electronics", patterns: ["elektronik", "ses sistemi", "kulaklık", "hoparlör", "kamera", "fotoğraf"] },
  { kind: "job", patterns: ["iş ilan", "tam zamanlı", "yarı zamanlı", "freelance", "uzaktan çalış", "kariyer", "personel"] },
  { kind: "service", patterns: ["hizmet", "usta", "danışmanlık", "nakliye", "taşıma", "temizlik hizmet"] },
];

function normalize(name: string): string {
  return name.toLocaleLowerCase("tr-TR").trim();
}

export interface CategoryPathNode {
  id: number;
  name: string;
  parentId?: number | null;
}

/** Düz kategori listesinden tam yol üretir: Elektronik › Telefon › Apple › iPhone 16 */
export function buildCategoryPath(
  categories: CategoryPathNode[],
  categoryId: number | string | undefined,
): string {
  if (!categoryId) return "";
  const id = Number(categoryId);
  if (!id) return "";

  const byId = new Map(categories.map((c) => [c.id, c]));
  const parts: string[] = [];
  const seen = new Set<number>();
  let cur = byId.get(id);

  while (cur && !seen.has(cur.id)) {
    seen.add(cur.id);
    parts.unshift(cur.name);
    cur = cur.parentId != null ? byId.get(cur.parentId) : undefined;
  }

  return parts.join(" › ");
}

export function resolveCategoryProfile(categoryName?: string, categoryPath?: string): CategoryProfile | null {
  const path = categoryPath?.trim();
  const name = categoryName?.trim();
  if (!path && !name) return null;

  const text = path || name!;
  const cat = normalize(text);

  for (const { kind, patterns } of MATCHERS) {
    if (patterns.some((p) => cat.includes(p))) {
      return buildProfile(kind, name || path || text);
    }
  }

  return buildProfile("other", name || path || text);
}

function buildProfile(kind: CategoryKind, categoryName: string): CategoryProfile {
  const base = {
    kind,
    label: categoryName,
    showYear: true,
    showColor: true,
    showSwap: true,
    showComputer: false,
    showPhone: false,
    showTablet: false,
    showCoffee: false,
    showTv: false,
    showGaming: false,
    showVehicle: false,
    showEstate: false,
    showAppliance: false,
    showFurniture: false,
    showClothing: false,
    showJob: false,
    showService: false,
  };

  switch (kind) {
    case "computer":
      return {
        ...base,
        productSectionTitle: "Bilgisayar bilgileri",
        brandPlaceholder: "Örn. Asus, Apple, HP",
        modelPlaceholder: "Örn. ROG Strix G15, MacBook Air M2",
        showComputer: true,
        showSwap: true,
      };
    case "phone":
      return {
        ...base,
        productSectionTitle: "Telefon bilgileri",
        brandPlaceholder: "Örn. Apple, Samsung, Xiaomi",
        modelPlaceholder: "Örn. iPhone 15, Galaxy S24",
        showPhone: true,
      };
    case "tablet":
      return {
        ...base,
        productSectionTitle: "Tablet bilgileri",
        brandPlaceholder: "Örn. Apple, Samsung, Lenovo",
        modelPlaceholder: "Örn. iPad Air, Tab S9",
        showTablet: true,
      };
    case "coffee":
      return {
        ...base,
        productSectionTitle: "Kahve makinesi bilgileri",
        brandPlaceholder: "Örn. DeLonghi, Nespresso, Arçelik",
        modelPlaceholder: "Örn. Magnifica, Vertuo",
        showCoffee: true,
        showYear: true,
      };
    case "tv":
      return {
        ...base,
        productSectionTitle: "Televizyon bilgileri",
        brandPlaceholder: "Örn. Samsung, LG, Sony",
        modelPlaceholder: "Örn. 55 inç QLED",
        showTv: true,
      };
    case "gaming":
      return {
        ...base,
        productSectionTitle: "Oyun / konsol bilgileri",
        brandPlaceholder: "Örn. Sony, Microsoft, Nintendo",
        modelPlaceholder: "Örn. PS5, Xbox Series X",
        showGaming: true,
      };
    case "vehicle":
      return {
        ...base,
        productSectionTitle: "Araç bilgileri",
        brandPlaceholder: "Örn. Toyota, BMW, Renault",
        modelPlaceholder: "Örn. Corolla 1.6, 320i",
        showVehicle: true,
        showColor: true,
      };
    case "estate":
      return {
        ...base,
        productSectionTitle: "Konut / emlak bilgileri",
        brandPlaceholder: "Proje veya site adı (opsiyonel)",
        modelPlaceholder: "Daire tipi, örn. 3+1",
        showEstate: true,
        showSwap: false,
        showYear: false,
      };
    case "appliance":
      return {
        ...base,
        productSectionTitle: "Beyaz eşya / ev aleti bilgileri",
        brandPlaceholder: "Örn. Arçelik, Bosch, Siemens",
        modelPlaceholder: "Örn. 9 kg çamaşır makinesi",
        showAppliance: true,
      };
    case "furniture":
      return {
        ...base,
        productSectionTitle: "Mobilya bilgileri",
        brandPlaceholder: "Marka veya üretici",
        modelPlaceholder: "Ürün adı / model",
        showFurniture: true,
        showYear: false,
      };
    case "clothing":
      return {
        ...base,
        productSectionTitle: "Giyim / aksesuar bilgileri",
        brandPlaceholder: "Örn. Nike, Zara, Adidas",
        modelPlaceholder: "Model veya beden bilgisi",
        showClothing: true,
        showYear: false,
        showSwap: false,
      };
    case "electronics":
      return {
        ...base,
        productSectionTitle: "Elektronik ürün bilgileri",
        brandPlaceholder: "Marka",
        modelPlaceholder: "Model",
        showPhone: false,
        showComputer: false,
        showTv: false,
        showColor: true,
      };
    case "job":
      return {
        ...base,
        productSectionTitle: "İş ilanı bilgileri",
        brandPlaceholder: "Firma / işveren",
        modelPlaceholder: "Pozisyon",
        showJob: true,
        showSwap: false,
        showYear: false,
        showColor: false,
      };
    case "service":
      return {
        ...base,
        productSectionTitle: "Hizmet bilgileri",
        brandPlaceholder: "Hizmet sağlayıcı / firma",
        modelPlaceholder: "Hizmet adı",
        showService: true,
        showSwap: false,
        showYear: false,
        showColor: false,
      };
    default:
      return {
        ...base,
        productSectionTitle: "Ürün bilgileri",
        brandPlaceholder: "Marka",
        modelPlaceholder: "Model",
      };
  }
}

/** Emlak için marka/model zorunlu değil */
export function isBrandModelRequired(profile: CategoryProfile | null): boolean {
  if (!profile) return true;
  return profile.kind !== "estate" && profile.kind !== "job" && profile.kind !== "service";
}

export function inferListingType(profile: CategoryProfile | null): number {
  if (!profile) return 0;
  if (profile.kind === "job") return 3;
  if (profile.kind === "service") return 4;
  return 0;
}
