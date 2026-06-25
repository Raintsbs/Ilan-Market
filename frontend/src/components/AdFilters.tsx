"use client";

import { useState } from "react";
import { CategoryTreePicker } from "@/components/CategoryTreePicker";
import { AnimatedGlowingSearchBar } from "@/components/ui/animated-glowing-search-bar";
import { useLocale } from "@/context/LocaleContext";
import { btnBrandSm, inputField, surfaceElevated } from "@/lib/uiStyles";

interface AdFiltersProps {
  search: string;
  categoryId: string;
  onSearchChange: (value: string) => void;
  onCategoryChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  advanced?: boolean;
  minPrice?: string;
  maxPrice?: string;
  city?: string;
  featuredOnly?: boolean;
  onMinPriceChange?: (v: string) => void;
  onMaxPriceChange?: (v: string) => void;
  onCityChange?: (v: string) => void;
  onFeaturedOnlyChange?: (v: boolean) => void;
}

export function AdFilters({
  search,
  categoryId,
  onSearchChange,
  onCategoryChange,
  onSubmit,
  advanced,
  minPrice = "",
  maxPrice = "",
  city = "",
  featuredOnly = false,
  onMinPriceChange,
  onMaxPriceChange,
  onCityChange,
  onFeaturedOnlyChange,
}: AdFiltersProps) {
  const { t } = useLocale();
  const [mobileAdvancedOpen, setMobileAdvancedOpen] = useState(false);

  return (
    <form
      onSubmit={onSubmit}
      className={`${surfaceElevated} flex flex-col gap-3 p-3 sm:gap-4 sm:p-4 lg:flex-row lg:flex-wrap lg:items-center`}
    >
      <div className="filter-search-wrap min-w-0 w-full flex-1 lg:min-w-[260px]">
        <AnimatedGlowingSearchBar
          value={search}
          onChange={onSearchChange}
          placeholder={t("home.searchPlaceholder")}
        />
      </div>

      <CategoryTreePicker
        compact
        value={categoryId}
        onChange={onCategoryChange}
        className="w-full lg:w-56"
      />

      {advanced && (
        <>
          <button
            type="button"
            onClick={() => setMobileAdvancedOpen((o) => !o)}
            className="flex h-10 w-full items-center justify-between rounded-xl border border-slate-200 bg-white px-3.5 text-sm font-semibold text-slate-700 lg:hidden dark:border-slate-600 dark:bg-slate-800 dark:text-slate-200"
          >
            {t("filter.moreFilters")}
            <svg
              className={`h-4 w-4 transition-transform ${mobileAdvancedOpen ? "rotate-180" : ""}`}
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
            </svg>
          </button>
          <div
            className={`grid grid-cols-1 gap-3 sm:grid-cols-2 ${
              mobileAdvancedOpen ? "" : "hidden"
            } lg:contents`}
          >
          <input
            type="number"
            value={minPrice}
            onChange={(e) => onMinPriceChange?.(e.target.value)}
            placeholder={t("filter.minPrice")}
            className={`${inputField} w-full lg:w-28`}
          />
          <input
            type="number"
            value={maxPrice}
            onChange={(e) => onMaxPriceChange?.(e.target.value)}
            placeholder={t("filter.maxPrice")}
            className={`${inputField} w-full lg:w-28`}
          />
          <input
            value={city}
            onChange={(e) => onCityChange?.(e.target.value)}
            placeholder={t("listing.city")}
            className={`${inputField} col-span-2 w-full sm:col-span-1 lg:w-36`}
          />
          <label className="col-span-2 flex h-10 w-full items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 text-xs font-semibold text-slate-700 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-200 lg:col-span-1 lg:w-auto">
            <input
              type="checkbox"
              checked={featuredOnly}
              onChange={(e) => onFeaturedOnlyChange?.(e.target.checked)}
              className="rounded border-slate-300 text-blue-600"
            />
            {t("filter.featuredOnly")}
          </label>
          </div>
        </>
      )}

      <button type="submit" className={`${btnBrandSm} lg:ml-auto`}>
        {t("home.filter")}
      </button>
    </form>
  );
}
