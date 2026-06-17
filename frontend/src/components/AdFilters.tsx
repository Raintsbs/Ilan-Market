"use client";

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

  return (
    <form
      onSubmit={onSubmit}
      className={`${surfaceElevated} flex flex-col gap-4 p-4 lg:flex-row lg:flex-wrap lg:items-center`}
    >
      <div className="filter-search-wrap min-w-0 flex-1 lg:min-w-[260px]">
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
        className="lg:w-56"
      />
      {advanced && (
        <>
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
            className={`${inputField} w-full lg:w-36`}
          />
          <label className="flex h-10 items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 text-xs font-semibold text-slate-700 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-200">
            <input
              type="checkbox"
              checked={featuredOnly}
              onChange={(e) => onFeaturedOnlyChange?.(e.target.checked)}
              className="rounded border-slate-300 text-blue-600"
            />
            {t("filter.featuredOnly")}
          </label>
        </>
      )}
      <button type="submit" className={`${btnBrandSm} lg:ml-auto`}>
        {t("home.filter")}
      </button>
    </form>
  );
}
