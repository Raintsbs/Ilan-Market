import type { MessageKey } from "@/lib/i18n/messages";
import type { ListingDetails as ApiListingDetails } from "./types";
import { resolveCategoryProfile } from "./categoryProfile";

export interface ListingDetails {
  price: number | null;
  city: string;
  district: string;
  neighborhood: string;
  condition: string;
  brand: string;
  model: string;
  year: string;
  warranty: string;
  sellerType: string;
  swap: boolean;
  color: string;
  storage: string;
  memory: string;
  processor: string;
  screenSize: string;
  mileage: string;
  fuelType: string;
  transmission: string;
  damageStatus: string;
  tramerStatus: string;
  expertReportUrl: string;
  damageRecord: string;
  floorPlanUrl: string;
  deedStatus: string;
  rentalYield: string;
  videoUrl: string;
  virtualTourUrl: string;
  roomCount: string;
  squareMeters: string;
  buildingAge: string;
  floor: string;
  heating: string;
  coffeeType: string;
  capacity: string;
  powerWatts: string;
  batteryHealth: string;
  material: string;
  size: string;
  salaryMin: string;
  salaryMax: string;
  employmentType: string;
  experienceLevel: string;
  workMode: string;
  serviceType: string;
  serviceArea: string;
  priceUnit: string;
}

export const EMPTY_LISTING_DETAILS: ListingDetails = {
  price: null,
  city: "",
  district: "",
  neighborhood: "",
  condition: "",
  brand: "",
  model: "",
  year: "",
  warranty: "",
  sellerType: "Bireysel",
  swap: false,
  color: "",
  storage: "",
  memory: "",
  processor: "",
  screenSize: "",
  mileage: "",
  fuelType: "",
  transmission: "",
  damageStatus: "",
  tramerStatus: "",
  expertReportUrl: "",
  damageRecord: "",
  floorPlanUrl: "",
  deedStatus: "",
  rentalYield: "",
  videoUrl: "",
  virtualTourUrl: "",
  roomCount: "",
  squareMeters: "",
  buildingAge: "",
  floor: "",
  heating: "",
  coffeeType: "",
  capacity: "",
  powerWatts: "",
  batteryHealth: "",
  material: "",
  size: "",
  salaryMin: "",
  salaryMax: "",
  employmentType: "",
  experienceLevel: "",
  workMode: "",
  serviceType: "",
  serviceArea: "",
  priceUnit: "",
};

export const TURKISH_CITIES = [
  "Adana", "Ankara", "Antalya", "Bursa", "İstanbul", "İzmir", "Konya", "Mersin",
  "Gaziantep", "Kocaeli", "Samsun", "Trabzon", "Eskişehir", "Diyarbakır",
];

export const CONDITIONS = ["Sıfır", "İkinci El", "Az Kullanılmış", "Yenilenmiş"];
export const WARRANTIES = ["Var", "Yok", "Garantisi devam ediyor"];
export const SELLER_TYPES = ["Bireysel", "Mağazadan", "Yetkili satıcı"];
export const COFFEE_TYPES = ["Espresso", "Kapsüllü", "Filtre kahve", "Tam otomatik", "Manuel"];
export const FUEL_TYPES = ["Benzin", "Dizel", "LPG", "Elektrik", "Hibrit"];
export const TRANSMISSIONS = ["Manuel", "Otomatik", "Yarı otomatik"];
export const TRAMER_STATUSES = ["Yok", "Var — temiz", "Var — hasar kayıtlı", "Sorgulanmadı"];
export const EMPLOYMENT_TYPES = ["Tam zamanlı", "Yarı zamanlı", "Freelance", "Staj", "Dönemsel"];
export const EXPERIENCE_LEVELS = ["Deneyimsiz", "1-3 yıl", "3-5 yıl", "5+ yıl"];
export const WORK_MODES = ["Ofiste", "Uzaktan", "Hibrit"];
export const PRICE_UNITS = ["Saatlik", "Günlük", "İş başı", "Aylık"];

export const DEED_STATUSES = [
  "Kat mülkiyeti",
  "Arsa tapulu",
  "Hisseli tapu",
  "İntifa hakkı",
  "Kooperatif",
  "Belirtilmemiş",
];

export function normalizeListingDetails(details: Partial<ListingDetails>): ListingDetails {
  const base: ListingDetails = { ...EMPTY_LISTING_DETAILS };
  for (const key of Object.keys(EMPTY_LISTING_DETAILS) as (keyof ListingDetails)[]) {
    const val = details[key];
    if (key === "price") {
      base.price = val != null && !Number.isNaN(Number(val)) ? Number(val) : null;
    } else if (key === "swap") {
      base.swap = Boolean(val);
    } else if (typeof val === "string") {
      base[key] = val;
    }
  }
  return base;
}

export function parseListingDetails(
  raw?: ApiListingDetails | ListingDetails | null,
): ListingDetails {
  if (!raw) return { ...EMPTY_LISTING_DETAILS };
  return normalizeListingDetails({
    ...EMPTY_LISTING_DETAILS,
    ...raw,
    price: raw.price != null ? Number(raw.price) : null,
  });
}

export function serializeListingDetails(details: ListingDetails): string {
  const payload: Record<string, unknown> = {};
  if (details.price != null && !Number.isNaN(details.price)) {
    payload.price = details.price;
  }
  const stringFields: (keyof ListingDetails)[] = [
    "city", "district", "condition", "brand", "model", "year", "warranty",
    "sellerType", "color", "storage", "memory", "processor", "screenSize",
    "mileage", "fuelType", "transmission", "damageStatus",
    "tramerStatus", "expertReportUrl", "damageRecord",
    "floorPlanUrl", "deedStatus", "rentalYield", "videoUrl", "virtualTourUrl",
    "roomCount",
    "squareMeters", "buildingAge", "floor", "heating",
    "coffeeType", "capacity", "powerWatts", "batteryHealth", "material", "size",
    "salaryMin", "salaryMax", "employmentType", "experienceLevel", "workMode",
    "serviceType", "serviceArea", "priceUnit",
  ];
  for (const key of stringFields) {
    const v = details[key];
    if (typeof v === "string" && v.trim()) payload[key] = v.trim();
  }
  if (details.swap) payload.swap = true;
  return JSON.stringify(payload);
}

const PRICE_LOCALE: Record<string, string> = {
  tr: "tr-TR",
  en: "en-US",
  de: "de-DE",
  fr: "fr-FR",
  es: "es-ES",
  ar: "ar-SA",
  ru: "ru-RU",
  zh: "zh-CN",
  ja: "ja-JP",
  ko: "ko-KR",
};

export function formatPrice(
  price: number | null | undefined,
  notSetLabel = "Fiyat belirtilmedi",
  locale = "tr",
): string {
  if (price == null || Number.isNaN(price)) return notSetLabel;
  const intl = PRICE_LOCALE[locale] ?? "en-US";
  return `${new Intl.NumberFormat(intl).format(price)} TL`;
}

export interface DetailRow {
  label: string;
  value: string;
}

export function getListingDetailRows(
  details: ListingDetails,
  categoryName: string | undefined,
  t: (key: MessageKey) => string,
  yesNo: { yes: string; no: string },
): DetailRow[] {
  const profile = resolveCategoryProfile(categoryName);
  const rows: DetailRow[] = [];

  const add = (labelKey: MessageKey, value?: string | null) => {
    if (value?.trim()) rows.push({ label: t(labelKey), value: value.trim() });
  };

  add("row.category", categoryName);
  add("row.condition", details.condition);
  add("row.seller", details.sellerType);
  if (profile?.showSwap !== false) {
    add("row.swap", details.swap ? yesNo.yes : yesNo.no);
  }
  add("row.brand", details.brand);
  add("row.model", details.model);
  if (profile?.showYear !== false) add("row.year", details.year);
  add("row.warranty", details.warranty);
  if (profile?.showColor !== false) add("row.color", details.color);

  if (profile?.showComputer) {
    add("row.storage", details.storage);
    add("row.ram", details.memory);
    add("row.processor", details.processor);
    add("row.screen", details.screenSize);
  }
  if (profile?.showPhone || profile?.showTablet) {
    add("row.storage", details.storage);
    add("row.screen", details.screenSize);
    add("row.battery", details.batteryHealth);
  }
  if (profile?.showCoffee) {
    add("row.coffeeType", details.coffeeType);
    add("row.capacity", details.capacity);
    add("row.pressure", details.processor);
    add("row.power", details.powerWatts);
  }
  if (profile?.showTv) {
    add("row.screen", details.screenSize);
    add("row.resolution", details.storage);
  }
  if (profile?.showGaming) {
    add("row.storage", details.storage);
  }
  if (profile?.showVehicle) {
    add("row.mileage", details.mileage);
    add("row.fuel", details.fuelType);
    add("row.transmission", details.transmission);
    add("row.damage", details.damageStatus);
    add("row.tramer", details.tramerStatus);
    add("row.damageRecord", details.damageRecord);
    if (details.expertReportUrl?.trim()) {
      rows.push({ label: t("row.expertReport"), value: details.expertReportUrl.trim() });
    }
  }
  if (profile?.showEstate) {
    add("row.rooms", details.roomCount);
    add("row.sqm", details.squareMeters);
    add("row.buildingAge", details.buildingAge);
    add("row.floor", details.floor);
    add("row.heating", details.heating);
    add("row.deedStatus", details.deedStatus);
    add("row.rentalYield", details.rentalYield);
    if (details.floorPlanUrl?.trim()) {
      rows.push({ label: t("row.floorPlan"), value: details.floorPlanUrl.trim() });
    }
  }
  if (details.videoUrl?.trim()) {
    rows.push({ label: t("row.video"), value: details.videoUrl.trim() });
  }
  if (details.virtualTourUrl?.trim()) {
    rows.push({ label: t("row.virtualTour"), value: details.virtualTourUrl.trim() });
  }
  if (profile?.showAppliance) {
    add("row.capacity", details.capacity);
    add("row.power", details.powerWatts);
    add("row.energy", details.storage);
  }
  if (profile?.showFurniture) {
    add("row.material", details.material);
    add("row.size", details.size);
  }
  if (profile?.showClothing) {
    add("row.size", details.size);
    add("row.material", details.material);
  }
  if (profile?.showJob) {
    add("row.employmentType", details.employmentType);
    add("row.experienceLevel", details.experienceLevel);
    add("row.workMode", details.workMode);
    if (details.salaryMin?.trim() || details.salaryMax?.trim()) {
      rows.push({
        label: t("row.salary"),
        value: [details.salaryMin, details.salaryMax].filter(Boolean).join(" – "),
      });
    }
  }
  if (profile?.showService) {
    add("row.serviceType", details.serviceType);
    add("row.serviceArea", details.serviceArea);
    add("row.priceUnit", details.priceUnit);
  }

  return rows;
}

export function getLocationLine(details: ListingDetails): string | null {
  const parts = [details.neighborhood, details.district, details.city].filter((p) => p?.trim());
  return parts.length ? parts.join(" / ") : null;
}
