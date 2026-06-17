import type { MessageKey } from "@/lib/i18n/messages";
import { AdvertisementStatus } from "./types";

type TFn = (key: MessageKey) => string;

export function getStatusLabel(
  status: AdvertisementStatus,
  isActive: boolean,
  t: TFn,
): { label: string; className: string } {
  if (status === AdvertisementStatus.Approved && isActive) {
    return {
      label: t("status.live"),
      className: "bg-blue-500/15 text-blue-700 ring-blue-500/30 dark:text-blue-300",
    };
  }
  switch (status) {
    case AdvertisementStatus.Pending:
      return {
        label: t("status.pending"),
        className: "bg-amber-500/15 text-amber-800 ring-amber-500/30 dark:text-amber-200",
      };
    case AdvertisementStatus.Rejected:
      return {
        label: t("status.rejected"),
        className: "bg-rose-500/15 text-rose-700 ring-rose-500/30 dark:text-rose-300",
      };
    case AdvertisementStatus.Approved:
      return {
        label: t("status.inactive"),
        className: "bg-slate-500/15 text-slate-600 ring-slate-500/30 dark:text-slate-300",
      };
    default:
      return {
        label: t("status.inactive"),
        className: "bg-slate-500/15 text-slate-600 ring-slate-500/30 dark:text-slate-300",
      };
  }
}

const LOCALE_MAP: Record<string, string> = {
  tr: "tr-TR",
  en: "en-US",
  de: "de-DE",
  fr: "fr-FR",
  es: "es-ES",
  it: "it-IT",
  pt: "pt-PT",
  ru: "ru-RU",
  ar: "ar-SA",
  zh: "zh-CN",
  ja: "ja-JP",
  ko: "ko-KR",
};

export function formatDate(dateStr: string, locale = "tr"): string {
  const intlLocale = LOCALE_MAP[locale] ?? "en-US";
  return new Intl.DateTimeFormat(intlLocale, {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(new Date(dateStr));
}
