"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { AdCard } from "@/components/AdCard";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { PageHeader } from "@/components/PageHeader";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { clearRecentlyViewed, getRecentlyViewed } from "@/lib/recentlyViewed";
import { btnOutline, gridAds3, linkBack, pageContainerMd } from "@/lib/uiStyles";
import type { Advertisement } from "@/lib/types";

export default function RecentlyViewedPage() {
  const { t } = useLocale();
  const [ads, setAds] = useState<Advertisement[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      const recent = getRecentlyViewed();
      if (recent.length === 0) {
        setAds([]);
        setLoading(false);
        return;
      }
      const ids = recent.map((r) => r.id);
      const res = await api.getAdvertisementsBatch(ids).catch(() => null);
      if (res?.success && res.data) {
        const order = new Map(ids.map((id, i) => [id, i]));
        setAds([...res.data].sort((a, b) => (order.get(a.id) ?? 0) - (order.get(b.id) ?? 0)));
      }
      setLoading(false);
    }
    load();
  }, []);

  if (loading) return <LoadingSpinner />;

  return (
    <div className={pageContainerMd}>
      <div className="flex flex-wrap items-end justify-between gap-4">
        <PageHeader title={t("recent.title")} subtitle={t("recent.subtitle")} className="mb-0" />
        {ads.length > 0 && (
          <button
            type="button"
            onClick={() => {
              clearRecentlyViewed();
              setAds([]);
            }}
            className={btnOutline}
          >
            {t("recent.clear")}
          </button>
        )}
      </div>

      {ads.length === 0 ? (
        <p className="mt-12 text-center text-slate-500">{t("recent.empty")}</p>
      ) : (
        <div className={`mt-8 ${gridAds3}`}>
          {ads.map((ad) => (
            <AdCard key={ad.id} ad={ad} />
          ))}
        </div>
      )}

      <Link href="/" className={`mt-8 ${linkBack}`}>
        ← {t("common.backHome")}
      </Link>
    </div>
  );
}
