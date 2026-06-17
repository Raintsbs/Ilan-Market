"use client";

import Link from "next/link";
import { Suspense } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useCallback, useEffect, useState } from "react";
import { MyAdCard } from "@/components/MyAdCard";
import { AdFilters } from "@/components/AdFilters";
import { AdGridSkeleton } from "@/components/AdGridSkeleton";
import { MyAdsStatusTabs, tabToStatus, type MyAdsTab } from "@/components/MyAdsStatusTabs";
import { Pagination } from "@/components/Pagination";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api, ApiError } from "@/lib/api";
import { useAdsChangeListener } from "@/lib/adsSync";
import type { Advertisement, Category, MyAdCounts, PagedResult } from "@/lib/types";
import { btnBrand, btnOutline, pageSubtitle, pageTitle } from "@/lib/uiStyles";

function parseTab(raw: string | null): MyAdsTab {
  if (raw === "pending" || raw === "approved" || raw === "rejected") return raw;
  return "all";
}

function MyAdsPageContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const tab = parseTab(searchParams.get("tab"));
  const { t } = useLocale();
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const [search, setSearch] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [appliedCategoryId, setAppliedCategoryId] = useState("");
  const [page, setPage] = useState(1);
  const [categories, setCategories] = useState<Category[]>([]);
  const [counts, setCounts] = useState<MyAdCounts | null>(null);
  const [result, setResult] = useState<PagedResult<Advertisement> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [needsLogin, setNeedsLogin] = useState(false);

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.replace("/giris?redirect=/ilanlarim");
    }
  }, [authLoading, isAuthenticated, router]);

  useEffect(() => {
    setPage(1);
  }, [tab]);

  useEffect(() => {
    api.getCategories().then((res) => {
      if (res.success && res.data) setCategories(res.data);
    });
  }, []);

  const loadCounts = useCallback(() => {
    if (!isAuthenticated) return;
    api.getMyAdCounts().then((res) => {
      if (res.success && res.data) setCounts(res.data);
    });
  }, [isAuthenticated]);

  const load = useCallback(async () => {
    if (!isAuthenticated) return;
    setLoading(true);
    setError("");
    setNeedsLogin(false);
    try {
      const status = tabToStatus(tab);
      const res = await api.getMyAdvertisements({
        search: appliedSearch || undefined,
        categoryId: appliedCategoryId ? Number(appliedCategoryId) : undefined,
        status,
        page,
        pageSize: 12,
      });
      if (res.success && res.data) {
        setResult(res.data);
      } else {
        setError(res.message || t("myAds.loadError"));
        setResult(null);
      }
    } catch (err) {
      setResult(null);
      if (err instanceof ApiError) {
        if (err.status === 401) {
          setError(t("myAds.sessionExpired"));
          setNeedsLogin(true);
        } else if (err.status === 404) {
          setError(t("myAds.apiNotFound"));
        } else {
          setError(err.message);
        }
      } else {
        setError(t("myAds.loadError"));
      }
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, appliedSearch, appliedCategoryId, page, tab, t]);

  useEffect(() => {
    if (isAuthenticated) {
      loadCounts();
      load();
    }
  }, [isAuthenticated, load, loadCounts]);

  useAdsChangeListener(() => {
    if (isAuthenticated) {
      loadCounts();
      load();
    }
  });

  if (authLoading) return <LoadingSpinner />;
  if (!isAuthenticated) return null;

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-2 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className={pageTitle}>{t("myAds.title")}</h1>
          <p className={pageSubtitle}>{t("myAds.onlyYours")}</p>
        </div>
        <div className="flex shrink-0 flex-wrap gap-2">
          <Link href="/siparisler" className={`${btnOutline} h-10 px-4 py-2.5`}>
            {t("nav.orders")}
          </Link>
          <Link href="/ilan/yeni" className={`${btnBrand} btn-ripple`}>
            + {t("myAds.newShort")}
          </Link>
        </div>
      </div>

      <MyAdsStatusTabs active={tab} counts={counts} />

      {counts && counts.all > counts.approved && tab === "all" && (
        <p className="mt-3 text-sm text-slate-500 dark:text-slate-400">
          {t("myAds.publicVisibleHint", { live: counts.approved, all: counts.all })}
        </p>
      )}

      <AdFilters
        search={search}
        categoryId={categoryId}
        onSearchChange={setSearch}
        onCategoryChange={setCategoryId}
        onSubmit={(e) => {
          e.preventDefault();
          setPage(1);
          setAppliedSearch(search);
          setAppliedCategoryId(categoryId);
        }}
      />

      {error && (
        <div className="mt-6 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700 dark:border-rose-900 dark:bg-rose-950 dark:text-rose-300">
          {error}
          {needsLogin && (
            <Link href="/giris?redirect=/ilanlarim" className="ml-2 font-semibold underline">
              {t("auth.loginShort")}
            </Link>
          )}
        </div>
      )}

      {loading ? (
        <AdGridSkeleton className="mt-8" />
      ) : result && result.items.length > 0 ? (
        <>
          <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {result.items.map((ad) => (
              <MyAdCard key={ad.id} ad={ad} />
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
        <div className="mt-12 text-center">
          <p className="text-slate-500 dark:text-slate-400">{t("myAds.noAdsYet")}</p>
          <Link href="/ilan/yeni" className="mt-3 inline-block text-blue-600 hover:underline dark:text-blue-400">
            {t("myAds.createFirst")}
          </Link>
        </div>
      )}
    </div>
  );
}

export default function MyAdsPage() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <MyAdsPageContent />
    </Suspense>
  );
}
