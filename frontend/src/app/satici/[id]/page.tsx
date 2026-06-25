"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { AdCard } from "@/components/AdCard";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { SellerRatingBlock } from "@/components/SellerRatingBlock";
import { FollowSellerButton } from "@/components/FollowSellerButton";
import { VerifiedBadge } from "@/components/VerifiedBadge";
import { useLocale } from "@/context/LocaleContext";
import { useAuth } from "@/context/AuthContext";
import { api } from "@/lib/api";
import { formatDate } from "@/lib/status";
import { PageHeader } from "@/components/PageHeader";
import { linkBack, pageContainerMd, surfaceCardPad } from "@/lib/uiStyles";
import type { Advertisement, SellerAnalytics, SellerProfile } from "@/lib/types";

export default function SellerProfilePage() {
  const params = useParams();
  const userId = Number(params.id);
  const { t, locale } = useLocale();
  const { user } = useAuth();
  const [profile, setProfile] = useState<SellerProfile | null>(null);
  const [listings, setListings] = useState<Advertisement[]>([]);
  const [analytics, setAnalytics] = useState<SellerAnalytics | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!userId || Number.isNaN(userId)) {
      setLoading(false);
      return;
    }
    Promise.all([
      api.getSellerProfile(userId),
      api.getAdvertisements({ sellerUserId: userId, pageSize: 24 }),
      api.getSellerAnalytics(userId),
    ]).then(([profileRes, adsRes, statsRes]) => {
      if (profileRes.success && profileRes.data) setProfile(profileRes.data);
      if (adsRes.success && adsRes.data) setListings(adsRes.data.items);
      if (statsRes.success && statsRes.data) setAnalytics(statsRes.data);
      setLoading(false);
    });
  }, [userId]);

  if (loading) return <LoadingSpinner />;
  if (!profile) {
    return (
      <div className="mx-auto max-w-lg px-4 py-16 text-center text-slate-500">
        {t("ad.notFound")}
      </div>
    );
  }

  return (
    <div className={pageContainerMd}>
      <PageHeader title={t("seller.title")} />
      <div className={`mt-6 ${surfaceCardPad}`}>
        <div className="flex items-center gap-2">
          <p className="text-xl font-semibold text-slate-900 dark:text-white">{profile.displayName}</p>
          {profile.isVerified && <VerifiedBadge />}
        </div>
        <div className="mt-3">
          <FollowSellerButton sellerUserId={userId} isSelf={user?.userId === userId} />
        </div>
        {profile.isVerified && (
          <p className="mt-1 text-sm text-blue-600 dark:text-blue-400">{t("seller.verified")}</p>
        )}
        <dl className="mt-6 space-y-3 text-sm">
          <div className="flex justify-between">
            <dt className="text-slate-500">{t("seller.listings")}</dt>
            <dd className="font-semibold">{profile.activeListingCount}</dd>
          </div>
          <div className="flex justify-between">
            <dt className="text-slate-500">{t("seller.views")}</dt>
            <dd className="font-semibold">{profile.totalViews.toLocaleString(locale)}</dd>
          </div>
          <div className="flex justify-between">
            <dt className="text-slate-500">{t("store.completedSales")}</dt>
            <dd className="font-semibold">{profile.completedOrderCount ?? 0}</dd>
          </div>
          <div className="flex justify-between">
            <dt className="text-slate-500">{t("seller.memberSince")}</dt>
            <dd className="font-semibold">{formatDate(profile.memberSince, locale)}</dd>
          </div>
        </dl>
      </div>

      <div className={`mt-8 ${surfaceCardPad}`}>
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t("reviews.title")}</h2>
        <div className="mt-4">
          <SellerRatingBlock sellerUserId={userId} completedOrderCount={profile.completedOrderCount} showComposer />
        </div>
      </div>

      {analytics && (
        <div className={`mt-8 ${surfaceCardPad}`}>
          <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t("seller.statsTitle")}</h2>
          <dl className="mt-4 grid grid-cols-1 gap-4 text-sm sm:grid-cols-2">
            <div>
              <dt className="text-slate-500">{t("seller.totalOffers")}</dt>
              <dd className="text-xl font-bold">{analytics.totalOffers}</dd>
            </div>
            <div>
              <dt className="text-slate-500">{t("seller.totalMessages")}</dt>
              <dd className="text-xl font-bold">{analytics.totalMessageThreads}</dd>
            </div>
          </dl>
          {analytics.topAds.length > 0 && (
            <div className="mt-6">
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">{t("seller.topAds")}</p>
              <ul className="mt-3 space-y-2">
                {analytics.topAds.map((row) => (
                  <li key={row.advertisementId}>
                    <Link
                      href={`/ilan/${row.advertisementId}`}
                      className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-slate-100 px-3 py-2 text-sm hover:bg-slate-50 dark:border-slate-700 dark:hover:bg-slate-800"
                    >
                      <span className="font-medium text-slate-900 dark:text-white">{row.title}</span>
                      <span className="text-xs text-slate-500">
                        {row.viewCount} {t("seller.views").toLowerCase()} · {row.offerCount} {t("seller.statOffers")} · {row.messageThreadCount} {t("seller.statMessages")}
                      </span>
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}

      {listings.length > 0 && (
        <div className="mt-10 grid gap-6 sm:grid-cols-2">
          {listings.map((ad) => (
            <AdCard key={ad.id} ad={ad} />
          ))}
        </div>
      )}
      <Link href="/" className={`mt-6 ${linkBack}`}>
        ← {t("common.backHome")}
      </Link>
    </div>
  );
}
