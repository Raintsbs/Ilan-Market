"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { AdCard } from "@/components/AdCard";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { SellerRatingBlock } from "@/components/SellerRatingBlock";
import { StarRating } from "@/components/StarRating";
import { FollowSellerButton } from "@/components/FollowSellerButton";
import { VerifiedBadge } from "@/components/VerifiedBadge";
import { SafeImage } from "@/components/SafeImage";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { getImageUrl } from "@/lib/image";
import { formatDate } from "@/lib/status";
import { PageHeader } from "@/components/PageHeader";
import { linkBack, pageContainerMd, surfaceCard } from "@/lib/uiStyles";
import type { Advertisement, SellerProfile } from "@/lib/types";

export default function StorePage() {
  const params = useParams();
  const slug = String(params.slug ?? "");
  const { t, locale } = useLocale();
  const { user } = useAuth();
  const [profile, setProfile] = useState<SellerProfile | null>(null);
  const [listings, setListings] = useState<Advertisement[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!slug) {
      setLoading(false);
      return;
    }
    api.getSellerBySlug(slug).then(async (profileRes) => {
      if (profileRes.success && profileRes.data) {
        setProfile(profileRes.data);
        const adsRes = await api.getAdvertisements({
          sellerUserId: profileRes.data.userId,
          pageSize: 24,
        });
        if (adsRes.success && adsRes.data) setListings(adsRes.data.items);
      }
      setLoading(false);
    });
  }, [slug]);

  if (loading) return <LoadingSpinner />;
  if (!profile) {
    return (
      <div className="mx-auto max-w-lg px-4 py-16 text-center text-slate-500 dark:text-slate-400">
        {t("ad.notFound")}
      </div>
    );
  }

  const banner = profile.storeBannerPath ? getImageUrl(profile.storeBannerPath) : null;
  const avatar = profile.profileImagePath ? getImageUrl(profile.profileImagePath) : null;

  return (
    <div className={pageContainerMd}>
      <Link href="/" className={linkBack}>
        {t("nav.backHome")}
      </Link>

      {banner && (
        <div className="relative mt-4 h-40 overflow-hidden rounded-2xl bg-slate-200 dark:bg-slate-800 sm:h-52">
          <SafeImage src={banner} alt="" fill className="object-cover" />
        </div>
      )}

      <div className={`${surfaceCard} mt-6 p-6`}>
        <div className="flex flex-wrap items-start gap-4">
          {avatar && (
            <div className="relative h-16 w-16 overflow-hidden rounded-xl border border-slate-200 dark:border-slate-700">
              <SafeImage src={avatar} alt="" fill className="object-cover" />
            </div>
          )}
          <div className="flex-1">
            <PageHeader
              title={profile.displayName}
              subtitle={
                profile.isCorporateStore && profile.companyName
                  ? profile.companyName
                  : undefined
              }
            />
            <div className="mt-2 flex flex-wrap items-center gap-2">
              {profile.isVerified && <VerifiedBadge />}
              <FollowSellerButton sellerUserId={profile.userId} isSelf={user?.userId === profile.userId} />
              {(profile.averageRating ?? 0) > 0 && (
                <StarRating rating={profile.averageRating!} size="sm" showValue />
              )}
              {(profile.reviewCount ?? 0) > 0 && (
                <span className="text-xs text-slate-500">
                  {t("reviews.count", { count: profile.reviewCount ?? 0 })}
                </span>
              )}
              {profile.isCorporateStore && (
                <span className="rounded-full bg-blue-100 px-2 py-0.5 text-xs font-medium text-blue-800 dark:bg-blue-900/40 dark:text-blue-200">
                  Kurumsal mağaza
                </span>
              )}
            </div>
            {profile.storeDescription && (
              <p className="mt-3 text-sm text-slate-600 dark:text-slate-300">{profile.storeDescription}</p>
            )}
          </div>
        </div>

        <dl className="mt-6 grid grid-cols-2 gap-4 text-sm sm:grid-cols-4">
          <div>
            <dt className="text-slate-500 dark:text-slate-400">{t("seller.listings")}</dt>
            <dd className="font-semibold text-slate-900 dark:text-white">{profile.activeListingCount}</dd>
          </div>
          <div>
            <dt className="text-slate-500 dark:text-slate-400">{t("seller.views")}</dt>
            <dd className="font-semibold text-slate-900 dark:text-white">{profile.totalViews}</dd>
          </div>
          <div>
            <dt className="text-slate-500 dark:text-slate-400">{t("store.completedSales")}</dt>
            <dd className="font-semibold text-slate-900 dark:text-white">{profile.completedOrderCount ?? 0}</dd>
          </div>
          <div>
            <dt className="text-slate-500 dark:text-slate-400">{t("seller.memberSince")}</dt>
            <dd className="font-semibold text-slate-900 dark:text-white">
              {formatDate(profile.memberSince, locale)}
            </dd>
          </div>
        </dl>
      </div>

      <div className={`mt-8 p-6 ${surfaceCard}`}>
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t("reviews.title")}</h2>
        <div className="mt-4">
          <SellerRatingBlock
            sellerUserId={profile.userId}
            completedOrderCount={profile.completedOrderCount}
            showComposer
          />
        </div>
      </div>

      <h2 className="mt-10 text-lg font-semibold text-slate-900 dark:text-white">{t("seller.listings")}</h2>
      <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {listings.map((ad) => (
          <AdCard key={ad.id} ad={ad} />
        ))}
      </div>
    </div>
  );
}
