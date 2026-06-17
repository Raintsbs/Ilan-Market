"use client";

import { LOCALE_OPTIONS } from "@/lib/i18n/locales";
import { useLocale } from "@/context/LocaleContext";
import { ThemeToggle } from "@/components/ui/theme-toggle";

const selectClass =
  "h-9 shrink-0 rounded-lg border border-slate-200 bg-white px-2 text-xs font-medium text-slate-700 outline-none focus:border-blue-400 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-200";

export function ThemeLanguageControls({
  compact = false,
  onDarkBackground = false,
}: {
  compact?: boolean;
  onDarkBackground?: boolean;
}) {
  const labelClass = onDarkBackground
    ? "text-xs font-medium text-slate-400"
    : "text-xs font-medium text-slate-600 dark:text-slate-400";
  const { locale, setLocale, t } = useLocale();

  if (compact) {
    return (
      <div className="flex h-9 shrink-0 items-center gap-1">
        <ThemeToggle />
        <select
          aria-label={t("lang.label")}
          value={locale}
          onChange={(e) => setLocale(e.target.value as typeof locale)}
          className={`${selectClass} w-[4.25rem] sm:w-auto sm:min-w-[6.5rem] sm:max-w-[8.5rem]`}
        >
          {LOCALE_OPTIONS.map((opt) => (
            <option key={opt.code} value={opt.code}>
              {opt.flag} {opt.nativeName}
            </option>
          ))}
        </select>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
      <div className="flex items-center gap-2">
        <span className={labelClass}>{t("theme.toggle")}</span>
        <ThemeToggle />
      </div>
      <label className={`flex flex-col gap-1 text-xs ${labelClass}`}>
        <span className="font-medium">{t("lang.label")}</span>
        <select
          value={locale}
          onChange={(e) => setLocale(e.target.value as typeof locale)}
          className={`${selectClass} min-w-[160px]`}
        >
          {LOCALE_OPTIONS.map((opt) => (
            <option key={opt.code} value={opt.code}>
              {opt.flag} {opt.nativeName}
            </option>
          ))}
        </select>
      </label>
    </div>
  );
}
