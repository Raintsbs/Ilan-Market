"use client";

import { useLocale } from "@/context/LocaleContext";

export function LoadingSpinner({ label }: { label?: string }) {
  const { t } = useLocale();
  return (
    <div className="flex flex-col items-center justify-center gap-4 py-20">
      <div className="h-10 w-10 animate-spin rounded-full border-4 border-blue-200 border-t-blue-600 dark:border-blue-900 dark:border-t-blue-400" />
      <p className="text-sm text-slate-500 dark:text-slate-400">{label ?? t("common.loading")}</p>
    </div>
  );
}
