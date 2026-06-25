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
    <div className="flex flex-col items-stretch justify-center gap-2 px-1 pt-6 sm:flex-row sm:items-center sm:gap-3 sm:px-0 sm:pt-10">
      <button
        type="button"
        disabled={!hasPrevious}
        onClick={() => onPageChange(page - 1)}
        className={`${btnOutline} h-10 w-full px-4 sm:w-auto disabled:cursor-not-allowed disabled:opacity-40`}
      >
        {t("pagination.prev")}
      </button>
      <span className="order-first py-1 text-center text-sm font-medium tabular-nums text-slate-600 sm:order-none sm:min-w-[4rem] dark:text-slate-400">
        {page} / {totalPages}
      </span>
      <button
        type="button"
        disabled={!hasNext}
        onClick={() => onPageChange(page + 1)}
        className={`${btnOutline} h-10 w-full px-4 sm:w-auto disabled:cursor-not-allowed disabled:opacity-40`}
      >
        {t("pagination.next")}
      </button>
    </div>
  );
}
