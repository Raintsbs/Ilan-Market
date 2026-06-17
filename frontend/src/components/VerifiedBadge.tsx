"use client";

import { useLocale } from "@/context/LocaleContext";

export function VerifiedBadge() {
  const { t } = useLocale();
  return (
    <span
      className="inline-flex items-center gap-1 rounded-full bg-sky-100 px-2 py-0.5 text-xs font-semibold text-sky-800 dark:bg-sky-950 dark:text-sky-200"
      title={t("trust.verified")}
    >
      ✓ {t("trust.verifiedShort")}
    </span>
  );
}
