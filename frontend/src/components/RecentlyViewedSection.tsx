"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { AdCard } from "@/components/AdCard";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { getRecentlyViewed } from "@/lib/recentlyViewed";
import { linkBack } from "@/lib/uiStyles";
import type { Advertisement } from "@/lib/types";

export function RecentlyViewedSection({ compact = false }: { compact?: boolean }) {
  const { t } = useLocale();
  const [ads, setAds] = useState<Advertisement[]>([]);

  useEffect(() => {
    async function load() {
      const recent = getRecentlyViewed();
      if (recent.length === 0) {
        setAds([]);
        return;
      }
      const ids = recent.map((r) => r.id).slice(0, compact ? 4 : 8);
      const res = await api.getAdvertisementsBatch(ids).catch(() => null);
      if (res?.success && res.data) {
        const order = new Map(ids.map((id, i) => [id, i]));
        setAds([...res.data].sort((a, b) => (order.get(a.id) ?? 0) - (order.get(b.id) ?? 0)));
      }
    }
    load();
    const onChange = () => load();
    window.addEventListener("recently-viewed-changed", onChange);
    return () => window.removeEventListener("recently-viewed-changed", onChange);
  }, [compact]);

  if (ads.length === 0) return null;

  return (
    <section className={compact ? "mt-10" : "mt-8"}>
      <div className="mb-4 flex items-center justify-between gap-2">
        <h2 className="text-xl font-bold text-slate-900 dark:text-white">{t("recent.title")}</h2>
        {!compact && (
          <Link href="/son-goruntulenen" className={linkBack}>
            {t("recent.viewAll")} →
          </Link>
        )}
      </div>
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {ads.map((ad) => (
          <AdCard key={ad.id} ad={ad} />
        ))}
      </div>
    </section>
  );
}
