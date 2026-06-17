"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { AdCard } from "@/components/AdCard";
import { PriceAlertToggle } from "@/components/PriceAlertToggle";
import { AdGridSkeleton } from "@/components/AdGridSkeleton";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api, ApiError } from "@/lib/api";
import { useAdsChangeListener } from "@/lib/adsSync";
import { PageHeader } from "@/components/PageHeader";
import { alertWarning, pageContainer } from "@/lib/uiStyles";
import type { Advertisement } from "@/lib/types";

export default function FavoritesPage() {
  const router = useRouter();
  const { t } = useLocale();
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const [items, setItems] = useState<Advertisement[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const res = await api.getFavorites();
      if (res.success && res.data) setItems(res.data);
      else setError(res.message || t("favorites.loadError"));
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.status === 401
            ? t("favorites.sessionExpired")
            : err.message
          : t("favorites.apiRestart"),
      );
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => {
    if (!authLoading && !isAuthenticated) router.replace("/giris");
  }, [authLoading, isAuthenticated, router]);

  useEffect(() => {
    if (isAuthenticated) load();
  }, [isAuthenticated, load]);

  useAdsChangeListener(() => {
    if (isAuthenticated) load();
  });

  if (authLoading) return <LoadingSpinner />;
  if (!isAuthenticated) return null;

  return (
    <div className={pageContainer}>
      <PageHeader title={t("favorites.title")} subtitle={t("favorites.subtitle")} />

      {loading ? (
        <AdGridSkeleton className="mt-8" />
      ) : error ? (
        <div className={`mt-8 ${alertWarning}`}>
          {error}
        </div>
      ) : items.length > 0 ? (
        <div className="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {items.map((ad) => (
            <div key={ad.id}>
              <AdCard ad={ad} />
              <PriceAlertToggle advertisementId={ad.id} />
            </div>
          ))}
        </div>
      ) : (
        <div className="mt-12 text-center">
          <p className="text-slate-500 dark:text-slate-400">{t("favorites.empty")}</p>
          <Link href="/" className="mt-3 inline-block text-blue-600 hover:underline dark:text-blue-400">
            {t("favorites.browse")}
          </Link>
        </div>
      )}
    </div>
  );
}
