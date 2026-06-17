"use client";

import { useLocale } from "@/context/LocaleContext";
import { btnOutline } from "@/lib/uiStyles";

interface PaginationProps {
  page: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
  onPageChange: (page: number) => void;
}

export function Pagination({
  page,
  totalPages,
  hasPrevious,
  hasNext,
  onPageChange,
}: PaginationProps) {
  const { t } = useLocale();
  if (totalPages <= 1) return null;

  return (
    <div className="flex items-center justify-center gap-3 pt-10">
      <button
        type="button"
        disabled={!hasPrevious}
        onClick={() => onPageChange(page - 1)}
        className={`${btnOutline} h-10 px-4 disabled:cursor-not-allowed disabled:opacity-40`}
      >
        {t("pagination.prev")}
      </button>
      <span className="min-w-[4rem] text-center text-sm font-medium tabular-nums text-slate-600 dark:text-slate-400">
        {page} / {totalPages}
      </span>
      <button
        type="button"
        disabled={!hasNext}
        onClick={() => onPageChange(page + 1)}
        className={`${btnOutline} h-10 px-4 disabled:cursor-not-allowed disabled:opacity-40`}
      >
        {t("pagination.next")}
      </button>
    </div>
  );
}
