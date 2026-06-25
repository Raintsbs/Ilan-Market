"use client";

import { useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import { AdCard } from "@/components/AdCard";
import { AdFilters } from "@/components/AdFilters";
import { SavedSearchPanel, type SavedSearchFilter } from "@/components/SavedSearchPanel";
import { ParametricFilters, type ParametricFilterValues } from "@/components/ParametricFilters";
import { AdGridSkeleton } from "@/components/AdGridSkeleton";
import { EmptyStateAnimated } from "@/components/EmptyStateAnimated";
import { RecentlyViewedSection } from "@/components/RecentlyViewedSection";
import { Pagination } from "@/components/Pagination";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { useAdsChangeListener } from "@/lib/adsSync";
import { gridAds, siteShell } from "@/lib/uiStyles";
import type { Advertisement, Category, PagedResult } from "@/lib/types";

export function HomePage() {
  const { t } = useLocale();
  const searchParams = useSearchParams();
  const urlCategoryId = searchParams.get("categoryId") ?? "";
  const urlSearch = searchParams.get("q") ?? "";
  const urlMinPrice = searchParams.get("minPrice") ?? "";
  const urlMaxPrice = searchParams.get("maxPrice") ?? "";
  const urlCity = searchParams.get("city") ?? "";
  const urlFeatured = searchParams.get("featured") === "1";
  const urlBrand = searchParams.get("brand") ?? "";
  const urlModel = searchParams.get("model") ?? "";
  const urlMinYear = searchParams.get("minYear") ?? "";
  const urlMaxYear = searchParams.get("maxYear") ?? "";
  const urlMinKm = searchParams.get("minMileage") ?? "";
  const urlMaxKm = searchParams.get("maxMileage") ?? "";

  const [search, setSearch] = useState(urlSearch);
  const [categoryId, setCategoryId] = useState(urlCategoryId);
  const [minPrice, setMinPrice] = useState(urlMinPrice);
  const [maxPrice, setMaxPrice] = useState(urlMaxPrice);
  const [city, setCity] = useState(urlCity);
  const [featuredOnly, setFeaturedOnly] = useState(urlFeatured);
  const [appliedSearch, setAppliedSearch] = useState(urlSearch);
  const [appliedCategoryId, setAppliedCategoryId] = useState(urlCategoryId);
  const [appliedMinPrice, setAppliedMinPrice] = useState<number | undefined>(
    urlMinPrice ? Number(urlMinPrice) : undefined,
  );
  const [appliedMaxPrice, setAppliedMaxPrice] = useState<number | undefined>(
    urlMaxPrice ? Number(urlMaxPrice) : undefined,
  );
  const [appliedCity, setAppliedCity] = useState(urlCity);
  const [appliedFeaturedOnly, setAppliedFeaturedOnly] = useState(urlFeatured);
  const [parametric, setParametric] = useState<ParametricFilterValues>({
    brand: urlBrand,
    model: urlModel,
    minYear: urlMinYear,
    maxYear: urlMaxYear,
    minMileage: urlMinKm,
    maxMileage: urlMaxKm,
  });
  const [appliedParametric, setAppliedParametric] = useState<ParametricFilterValues>(() => ({
    brand: urlBrand,
    model: urlModel,
    minYear: urlMinYear,
    maxYear: urlMaxYear,
    minMileage: urlMinKm,
    maxMileage: urlMaxKm,
  }));
  const [page, setPage] = useState(1);
  const [categories, setCategories] = useState<Category[]>([]);
  const [result, setResult] = useState<PagedResult<Advertisement> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [staggerSeed, setStaggerSeed] = useState(0);
  const [staggerActive, setStaggerActive] = useState(false);
  const [heroActive, setHeroActive] = useState(false);
  const [refreshTick, setRefreshTick] = useState(0);
  const initialLoading = loading && !result;

  useEffect(() => {
    setHeroActive(false);
    let raf1 = 0;
    let raf2 = 0;
    raf1 = requestAnimationFrame(() => {
      raf2 = requestAnimationFrame(() => setHeroActive(true));
    });

    return () => {
      cancelAnimationFrame(raf1);
      cancelAnimationFrame(raf2);
    };
  }, []);

  useEffect(() => {
    api
      .getCategories()
      .then((res) => {
        if (res.success && res.data) setCategories(res.data.filter((c) => c.isActive));
      })
      .catch(() => {
        /* API kapalıyken ana sayfa yine açılsın */
      });
  }, []);

  const hasUrlFilters =
    !!urlSearch ||
    !!urlCategoryId ||
    !!urlMinPrice ||
    !!urlMaxPrice ||
    !!urlCity ||
    urlFeatured ||
    !!urlBrand ||
    !!urlModel ||
    !!urlMinYear ||
    !!urlMaxYear ||
    !!urlMinKm ||
    !!urlMaxKm;

  useEffect(() => {
    if (!hasUrlFilters) return;
    setSearch(urlSearch);
    setCategoryId(urlCategoryId);
    setMinPrice(urlMinPrice);
    setMaxPrice(urlMaxPrice);
    setCity(urlCity);
    setFeaturedOnly(urlFeatured);
    setAppliedSearch(urlSearch);
    setAppliedCategoryId(urlCategoryId);
    setAppliedMinPrice(urlMinPrice ? Number(urlMinPrice) : undefined);
    setAppliedMaxPrice(urlMaxPrice ? Number(urlMaxPrice) : undefined);
    setAppliedCity(urlCity);
    setAppliedFeaturedOnly(urlFeatured);
    const p = {
      brand: urlBrand,
      model: urlModel,
      minYear: urlMinYear,
      maxYear: urlMaxYear,
      minMileage: urlMinKm,
      maxMileage: urlMaxKm,
    };
    setParametric(p);
    setAppliedParametric(p);
    setPage(1);
  }, [
    hasUrlFilters,
    urlCategoryId,
    urlSearch,
    urlMinPrice,
    urlMaxPrice,
    urlCity,
    urlFeatured,
    urlBrand,
    urlModel,
    urlMinYear,
    urlMaxYear,
    urlMinKm,
    urlMaxKm,
  ]);

  const loadKey = [
    appliedSearch,
    appliedCategoryId,
    appliedMinPrice ?? "",
    appliedMaxPrice ?? "",
    appliedCity,
    appliedFeaturedOnly ? "1" : "0",
    appliedParametric.brand ?? "",
    appliedParametric.model ?? "",
    appliedParametric.minYear ?? "",
    appliedParametric.maxYear ?? "",
    appliedParametric.minMileage ?? "",
    appliedParametric.maxMileage ?? "",
    page,
  ].join("|");

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError("");

    (async () => {
      try {
        const res = await api.getAdvertisements({
          search: appliedSearch || undefined,
          categoryId: appliedCategoryId ? Number(appliedCategoryId) : undefined,
          minPrice: appliedMinPrice,
          maxPrice: appliedMaxPrice,
          city: appliedCity || undefined,
          featuredOnly: appliedFeaturedOnly || undefined,
          brand: appliedParametric.brand || undefined,
          model: appliedParametric.model || undefined,
          minYear: appliedParametric.minYear ? Number(appliedParametric.minYear) : undefined,
          maxYear: appliedParametric.maxYear ? Number(appliedParametric.maxYear) : undefined,
          minMileage: appliedParametric.minMileage
            ? Number(appliedParametric.minMileage)
            : undefined,
          maxMileage: appliedParametric.maxMileage
            ? Number(appliedParametric.maxMileage)
            : undefined,
          listingId: /^\d+$/.test(appliedSearch.trim()) ? Number(appliedSearch.trim()) : undefined,
          page,
          pageSize: 12,
        });
        if (cancelled) return;
        void api
          .logSearch(
            appliedCategoryId ? Number(appliedCategoryId) : undefined,
            appliedSearch || undefined,
          )
          .catch(() => {});
        if (res.success && res.data) setResult(res.data);
        else setError(res.message || t("home.loadError"));
      } catch (err) {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : t("home.apiError"));
      } finally {
        if (!cancelled) {
          setStaggerActive(false);
          setLoading(false);
          setStaggerSeed((s) => s + 1);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [loadKey, refreshTick, t]);

  useAdsChangeListener(() => setRefreshTick((n) => n + 1));

  useEffect(() => {
    if (loading || !result?.items.length) return;

    setStaggerActive(false);
    let raf1 = 0;
    let raf2 = 0;
    raf1 = requestAnimationFrame(() => {
      raf2 = requestAnimationFrame(() => setStaggerActive(true));
    });

    return () => {
      cancelAnimationFrame(raf1);
      cancelAnimationFrame(raf2);
    };
  }, [loading, staggerSeed, result?.items.length]);

  function handleFilterSubmit(e: React.FormEvent) {
    e.preventDefault();
    setPage(1);
    setAppliedSearch(search);
    setAppliedCategoryId(categoryId);
    setAppliedMinPrice(minPrice ? Number(minPrice) : undefined);
    setAppliedMaxPrice(maxPrice ? Number(maxPrice) : undefined);
    setAppliedCity(city);
    setAppliedFeaturedOnly(featuredOnly);
    setAppliedParametric(parametric);
  }

  function applySavedSearch(filter: SavedSearchFilter) {
    setSearch(filter.search ?? "");
    setCategoryId(filter.categoryId ?? "");
    setMinPrice(filter.minPrice ?? "");
    setMaxPrice(filter.maxPrice ?? "");
    setCity(filter.city ?? "");
    setFeaturedOnly(!!filter.featuredOnly);
    setParametric({
      brand: filter.brand ?? "",
      model: filter.model ?? "",
      minYear: filter.minYear ?? "",
      maxYear: filter.maxYear ?? "",
      minMileage: filter.minMileage ?? "",
      maxMileage: filter.maxMileage ?? "",
    });
    setPage(1);
    setAppliedSearch(filter.search ?? "");
    setAppliedCategoryId(filter.categoryId ?? "");
    setAppliedMinPrice(filter.minPrice ? Number(filter.minPrice) : undefined);
    setAppliedMaxPrice(filter.maxPrice ? Number(filter.maxPrice) : undefined);
    setAppliedCity(filter.city ?? "");
    setAppliedFeaturedOnly(!!filter.featuredOnly);
    setAppliedParametric({
      brand: filter.brand ?? "",
      model: filter.model ?? "",
      minYear: filter.minYear ?? "",
      maxYear: filter.maxYear ?? "",
      minMileage: filter.minMileage ?? "",
      maxMileage: filter.maxMileage ?? "",
    });
  }

  const currentSavedFilter: SavedSearchFilter = {
    search,
    categoryId,
    minPrice,
    maxPrice,
    city,
    featuredOnly,
    brand: parametric.brand,
    model: parametric.model,
    minYear: parametric.minYear,
    maxYear: parametric.maxYear,
    minMileage: parametric.minMileage,
    maxMileage: parametric.maxMileage,
  };

  return (
    <>
      <section className="hero-mesh relative overflow-hidden text-white">
        <div className={`relative ${siteShell} py-10 sm:py-16 lg:py-20`}>
          <div className="max-w-2xl">
            <p
              className={`mb-2 text-[0.65rem] font-semibold uppercase tracking-[0.2em] text-blue-300/90 sm:mb-3 sm:text-xs ${
                heroActive ? "home-hero-line" : "home-hero-pending"
              }`}
              style={heroActive ? ({ "--hero-delay": "0ms" } as React.CSSProperties) : undefined}
            >
              İlanMarket
            </p>
            <h1
              className={`text-2xl font-bold tracking-tight sm:text-4xl lg:text-[2.75rem] lg:leading-tight ${
                heroActive ? "home-hero-line" : "home-hero-pending"
              }`}
              style={heroActive ? ({ "--hero-delay": "80ms" } as React.CSSProperties) : undefined}
            >
              {t("home.title")}
            </h1>
            <p
              className={`mt-3 max-w-lg text-sm leading-relaxed text-slate-300 sm:mt-4 sm:text-base sm:leading-relaxed md:text-lg ${
                heroActive ? "home-hero-line" : "home-hero-pending"
              }`}
              style={heroActive ? ({ "--hero-delay": "160ms" } as React.CSSProperties) : undefined}
            >
              {t("home.subtitle")}
            </p>
          </div>
        </div>
      </section>

      <div className={`${siteShell} py-5 sm:py-8`}>
        <div className="flex flex-col gap-3 sm:gap-4">
          <div className="min-w-0 flex-1">
            <AdFilters
              search={search}
              categoryId={categoryId}
              onSearchChange={setSearch}
              onCategoryChange={setCategoryId}
              onSubmit={handleFilterSubmit}
              advanced
              minPrice={minPrice}
              maxPrice={maxPrice}
              city={city}
              featuredOnly={featuredOnly}
              onMinPriceChange={setMinPrice}
              onMaxPriceChange={setMaxPrice}
              onCityChange={setCity}
              onFeaturedOnlyChange={setFeaturedOnly}
            />
            <ParametricFilters
              categories={categories}
              categoryId={categoryId}
              values={parametric}
              onChange={setParametric}
            />
          </div>
          <SavedSearchPanel currentFilter={currentSavedFilter} onApply={applySavedSearch} />
        </div>

        {initialLoading ? (
          <AdGridSkeleton showSummary className="mt-2" />
        ) : error ? (
          <div className="mt-8 rounded-2xl border border-amber-200 bg-amber-50 px-6 py-8 text-center text-amber-800 dark:border-amber-900 dark:bg-amber-950/50 dark:text-amber-200">
            <p className="font-medium">{error}</p>
            <p className="mt-2 text-sm opacity-90">
              API: http://localhost:5050 — terminalde{" "}
              <code className="rounded bg-amber-100 px-1 dark:bg-amber-900">dotnet run --project AdvertisementApp.API</code>
            </p>
          </div>
        ) : result && result.items.length > 0 ? (
          <>
            <div className="mb-5 flex items-center justify-between gap-3">
              <p className="text-sm font-medium text-slate-600 dark:text-slate-400">
                {t("home.found", { count: result.totalCount })}
              </p>
            </div>
            <div
              className={`home-grid-layout ${gridAds}${loading ? " pointer-events-none opacity-60" : ""}`}
            >
              {result.items.map((ad, index) => (
                <div
                  key={`${staggerSeed}-${ad.id}`}
                  className={staggerActive ? "home-grid-card" : "home-grid-card-pending"}
                  style={
                    staggerActive
                      ? ({
                          "--stagger-delay": `${Math.min(index, 11) * 75}ms`,
                        } as React.CSSProperties)
                      : undefined
                  }
                >
                  <AdCard ad={ad} />
                </div>
              ))}
            </div>
            <Pagination
              page={result.page}
              totalPages={result.totalPages}
              hasPrevious={result.hasPrevious}
              hasNext={result.hasNext}
              onPageChange={setPage}
            />
          </>
        ) : (
          <EmptyStateAnimated
            title={t("home.emptyTitle")}
            description={t("home.emptyDesc")}
            actionHref="/ilan/yeni"
            actionLabel={t("nav.newAd")}
          />
        )}

        <RecentlyViewedSection compact />
      </div>
    </>
  );
}
