export type LocaleCode =
  | "tr" | "en" | "de" | "fr" | "es" | "it" | "pt" | "ru" | "ar" | "zh" | "ja" | "ko";

export const DEFAULT_LOCALE: LocaleCode = "tr";
export const FALLBACK_LOCALE: LocaleCode = "en";

export interface LocaleOption {
  code: LocaleCode;
  flag: string;
  nativeName: string;
}

/** Popüler diller */
export const LOCALE_OPTIONS: LocaleOption[] = [
  { code: "tr", flag: "🇹🇷", nativeName: "Türkçe" },
  { code: "en", flag: "🇬🇧", nativeName: "English" },
  { code: "de", flag: "🇩🇪", nativeName: "Deutsch" },
  { code: "fr", flag: "🇫🇷", nativeName: "Français" },
  { code: "es", flag: "🇪🇸", nativeName: "Español" },
  { code: "it", flag: "🇮🇹", nativeName: "Italiano" },
  { code: "pt", flag: "🇵🇹", nativeName: "Português" },
  { code: "ru", flag: "🇷🇺", nativeName: "Русский" },
  { code: "ar", flag: "🇸🇦", nativeName: "العربية" },
  { code: "zh", flag: "🇨🇳", nativeName: "中文" },
  { code: "ja", flag: "🇯🇵", nativeName: "日本語" },
  { code: "ko", flag: "🇰🇷", nativeName: "한국어" },
];

export function isLocaleCode(value: string): value is LocaleCode {
  return LOCALE_OPTIONS.some((o) => o.code === value);
}
