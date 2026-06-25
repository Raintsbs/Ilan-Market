"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { AdCard } from "@/components/AdCard";
import { AdGridSkeleton } from "@/components/AdGridSkeleton";
import { useLocale } from "@/context/LocaleContext";
import { gridAds, pageContainer } from "@/lib/uiStyles";
import { api } from "@/lib/api";
import type { Advertisement, SeoLanding } from "@/lib/types";
import { AdvertisementStatus as AdStatus } from "@/lib/types";

type SeoLandingViewProps = {
  landing: SeoLanding;
};

export function SeoLandingView({ landing }: SeoLandingViewProps) {
  const { t } = useLocale();
  const [ads, setAds] = useState<Advertisement[]>([]);
  const [loading, setLoading] = useState(true);
  const [total, setTotal] = useState(landing.totalCount);

  useEffect(() => {
    setLoading(true);
    api
      .getAdvertisements({
        page: 1,
        pageSize: 24,
        city: landing.cityName,
        categoryId: landing.categoryId ?? undefined,
        status: AdStatus.Approved,
      })
      .then((res) => {
        if (res.success && res.data) {
          setAds(res.data.items);
          setTotal(res.data.totalCount);
        }
      })
      .finally(() => setLoading(false));
  }, [landing.cityName, landing.categoryId]);

  const titleParts = [landing.cityName];
  if (landing.categoryName) titleParts.push(landing.categoryName);
  const heading = titleParts.join(" — ");

  return (
    <div className={`${pageContainer} py-8`}>
      <nav className="mb-4 flex flex-wrap items-center gap-x-2 gap-y-1 text-sm text-slate-500 dark:text-slate-400">
        <Link href="/" className="hover:text-blue-600 dark:hover:text-blue-400">
          {t("nav.backHome")}
        </Link>
        <span aria-hidden>›</span>
        <span>{landing.cityName}</span>
        {landing.breadcrumbs.map((crumb) => (
          <span key={crumb.path} className="inline-flex items-center gap-x-2">
            <span aria-hidden>›</span>
            <Link href={`/${landing.citySlug}/${crumb.path}`} className="hover:text-blue-600">
              {crumb.name}
            </Link>
          </span>
        ))}
      </nav>

      <header className="mb-8">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-white sm:text-3xl">{heading}</h1>
        <p className="mt-2 text-slate-600 dark:text-slate-400">
          {total > 0
            ? t("seo.listingCount", { count: total })
            : t("seo.noListingsYet")}
        </p>
      </header>

      {loading ? (
        <AdGridSkeleton count={8} />
      ) : ads.length === 0 ? (
        <p className="rounded-xl border border-dashed border-slate-200 p-8 text-center text-slate-500 dark:border-slate-700">
          {t("home.emptyDesc")}
        </p>
      ) : (
        <div className={gridAds}>
          {ads.map((ad) => (
            <AdCard key={ad.id} ad={ad} />
          ))}
        </div>
      )}
    </div>
  );
}
