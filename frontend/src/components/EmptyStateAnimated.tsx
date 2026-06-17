"use client";

import Link from "next/link";
import { useLocale } from "@/context/LocaleContext";

type EmptyStateAnimatedProps = {
  title: string;
  description: string;
  actionHref?: string;
  actionLabel?: string;
};

export function EmptyStateAnimated({
  title,
  description,
  actionHref,
  actionLabel,
}: EmptyStateAnimatedProps) {
  const { t } = useLocale();

  return (
    <div className="mt-12 flex flex-col items-center text-center">
      <div className="animate-empty-float relative mb-6 flex h-24 w-24 items-center justify-center rounded-full bg-slate-100 dark:bg-slate-800">
        <div className="absolute inset-0 rounded-full bg-blue-500/10 animate-ping opacity-40" style={{ animationDuration: "2.5s" }} />
        <svg
          className="relative h-12 w-12 text-slate-400 dark:text-slate-500"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={1.5}
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
          />
        </svg>
      </div>
      <p className="animate-fade-in-up text-lg font-medium text-slate-700 dark:text-slate-200" style={{ animationDelay: "80ms" }}>
        {title}
      </p>
      <p className="animate-fade-in-up mt-2 max-w-sm text-sm text-slate-500 dark:text-slate-400" style={{ animationDelay: "160ms" }}>
        {description}
      </p>
      {actionHref && actionLabel && (
        <Link
          href={actionHref}
          className="btn-ripple animate-fade-in-up mt-6 rounded-xl bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-blue-700"
          style={{ animationDelay: "240ms" }}
        >
          {actionLabel}
        </Link>
      )}
      {!actionHref && (
        <p className="animate-fade-in-up mt-4 text-xs text-slate-400" style={{ animationDelay: "240ms" }}>
          {t("home.emptyHint")}
        </p>
      )}
    </div>
  );
}
