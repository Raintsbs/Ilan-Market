"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { AdDetailActions } from "@/components/AdDetailActions";
import { AdOffersPanel } from "@/components/AdOffersPanel";
import { FavoriteButton } from "@/components/FavoriteButton";
import { ImageGallery } from "@/components/ImageGallery";
import { ListingDetailsPanel, ListingPriceHeader } from "@/components/ListingDetailsPanel";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { CompareButton } from "@/components/CompareButton";
import { AdListingMedia } from "@/components/AdListingMedia";
import { AuctionPanel } from "@/components/AuctionPanel";
import { TramerQueryPanel } from "@/components/TramerQueryPanel";
import { PriceHistoryChart } from "@/components/PriceHistoryChart";
import { VehicleLoanCalculator } from "@/components/VehicleLoanCalculator";
import { resolveCategoryProfile } from "@/lib/categoryProfile";
import { SimilarAdsSection } from "@/components/SimilarAdsSection";
import { AdReviewsPanel } from "@/components/AdReviewsPanel";
import { ListingQaPanel } from "@/components/ListingQaPanel";
import { SellerRatingBlock } from "@/components/SellerRatingBlock";
import { StarRating } from "@/components/StarRating";
import { addRecentlyViewed } from "@/lib/recentlyViewed";
import { StatusBadge } from "@/components/StatusBadge";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api, ApiError } from "@/lib/api";
import { useAdsChangeListener } from "@/lib/adsSync";
import { getAdvertisementImageUrls } from "@/lib/image";
import { parseListingDetails } from "@/lib/listingDetails";
import { formatDate } from "@/lib/status";
import { btnBrand, linkBack, surfaceElevated } from "@/lib/uiStyles";
import type { Advertisement } from "@/lib/types";

type AdDetailClientProps = {
  id: number;
};

export function AdDetailClient({ id }: AdDetailClientProps) {
  const router = useRouter();
  const { t, locale } = useLocale();
  const { user, isAuthenticated } = useAuth();
  const [ad, setAd] = useState<Advertisement | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    if (!id || Number.isNaN(id)) {
      setError(t("ad.invalid"));
      setLoading(false);
      return;
    }
    try {
      const res = await api.getAdvertisement(id);
      if (res.success && res.data) setAd(res.data);
      else setError(res.message || t("ad.notFound"));
    } catch {
      setError(t("ad.loadError"));
    } finally {
      setLoading(false);
    }
  }, [id, t]);

  useEffect(() => {
    load();
  }, [load]);

  useAdsChangeListener(load);

  useEffect(() => {
    if (!ad?.id) return;
    void api.recordAdView(ad.id);
    const details = parseListingDetails(ad.listingDetails);
    addRecentlyViewed({
      id: ad.id,
      title: ad.title,
      imagePath: ad.imagePath,
      price: details.price,
    });
  }, [ad?.id, ad?.title, ad?.imagePath, ad?.listingDetails]);

  const isOwner = isAuthenticated && user && ad && user.userId === ad.userId;
  const imageUrls = ad ? getAdvertisementImageUrls(ad) : [];
  const listingDetails = ad ? parseListingDetails(ad.listingDetails) : null;
  const categoryProfile = ad ? resolveCategoryProfile(ad.categoryName) : null;

  async function handleDelete() {
    if (!ad || !confirm(t("confirm.deleteAd"))) return;
    setDeleting(true);
    try {
      await api.deleteAdvertisement(ad.id);
      router.push("/ilanlarim");
    } catch (err) {
      alert(err instanceof ApiError ? err.message : t("ad.deleteError"));
    } finally {
      setDeleting(false);
    }
  }

  if (loading) return <LoadingSpinner />;
  if (error || !ad) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <p className="text-lg text-slate-600 dark:text-slate-400">{error || t("ad.notFound")}</p>
        <Link href="/" className="mt-4 inline-block text-blue-600 hover:underline dark:text-blue-400">
          {t("common.backHome")}
        </Link>
      </div>
    );
  }

  return (
    <article className="mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-2 text-sm text-slate-500">
        <Link href="/" className={linkBack}>
          ← {t("nav.backAds")}
        </Link>
        <span>
          {ad.categoryName} · {formatDate(ad.createdTime, locale)}
        </span>
      </div>

      <div className={`overflow-hidden ${surfaceElevated}`}>
        <div className="grid lg:grid-cols-[1fr_340px] lg:divide-x lg:divide-slate-100 dark:lg:divide-slate-800">
          <div className="p-3 sm:p-4">
            <ImageGallery
              urls={imageUrls}
              alt={ad.title}
              favoriteSlot={<FavoriteButton advertisementId={ad.id} variant="icon" />}
            />
          </div>

          <aside className="flex flex-col gap-4 p-6 lg:p-8">
            <div className="flex items-start justify-between gap-2">
              <span className="text-sm font-semibold text-blue-600">{ad.categoryName}</span>
              <StatusBadge status={ad.status} isActive={ad.isActive} />
            </div>

            <ListingPriceHeader listingDetails={listingDetails} title={ad.title} />

            <div className="flex flex-wrap gap-2">
              <FavoriteButton advertisementId={ad.id} />
              <CompareButton advertisementId={ad.id} />
            </div>

            {!isOwner && (
              <div className="space-y-2">
                <Link
                  href={`/satici/${ad.userId}`}
                  className="text-sm font-medium text-blue-600 hover:underline dark:text-blue-400"
                >
                  {t("ad.viewSeller")} →
                </Link>
                {((ad.sellerAverageRating ?? 0) > 0 || (ad.sellerReviewCount ?? 0) > 0) && (
                  <div className="flex flex-wrap items-center gap-2 text-sm text-slate-600 dark:text-slate-400">
                    <span>{t("reviews.sellerRating")}:</span>
                    <StarRating rating={ad.sellerAverageRating ?? 0} size="sm" showValue />
                    {(ad.sellerReviewCount ?? 0) > 0 && (
                      <span className="text-xs text-slate-500">
                        ({t("reviews.count", { count: ad.sellerReviewCount ?? 0 })})
                      </span>
                    )}
                  </div>
                )}
                <a
                  href="#degerlendirmeler"
                  className="inline-flex text-sm font-semibold text-amber-700 hover:text-amber-800 dark:text-amber-400"
                >
                  ★ {t("reviews.scrollToReview")}
                </a>
              </div>
            )}

            <AdDetailActions ad={ad} isOwner={!!isOwner} sellerVerified={ad.sellerIsVerified} />

            {isOwner && <AdOffersPanel advertisementId={ad.id} />}

            {isOwner && (
              <div className="mt-auto flex flex-col gap-2 border-t border-slate-100 pt-4 dark:border-slate-800">
                <Link
                  href={`/ilan/${ad.id}/duzenle`}
                  className={`${btnBrand} w-full text-center`}
                >
                  {t("ad.edit")}
                </Link>
                <button
                  type="button"
                  onClick={handleDelete}
                  disabled={deleting}
                  className="rounded-xl border border-rose-200 px-5 py-2.5 text-sm font-semibold text-rose-600 hover:bg-rose-50 disabled:opacity-60 dark:border-rose-800 dark:text-rose-400 dark:hover:bg-rose-950"
                >
                  {deleting ? t("ad.deleting") : t("ad.delete")}
                </button>
              </div>
            )}
          </aside>
        </div>

        <div className="space-y-6 border-t border-slate-100 p-6 sm:p-8 dark:border-slate-800">
          {(ad.auction || ad.listingType === 2) && (
            <AuctionPanel
              advertisementId={ad.id}
              isOwner={!!isOwner}
              initial={ad.auction}
            />
          )}
          {listingDetails && (
            <AdListingMedia
              listingDetails={listingDetails}
              categoryName={ad.categoryName}
              videoPath={ad.videoPath}
              panoramaPath={ad.panoramaPath}
            />
          )}
          {categoryProfile?.showVehicle && (
            <TramerQueryPanel
              initialResult={ad.tramerResult}
              readOnly={!isOwner}
            />
          )}
          {categoryProfile?.showVehicle && listingDetails && (
            <VehicleLoanCalculator price={listingDetails.price} />
          )}
          <PriceHistoryChart advertisementId={ad.id} />
          <ListingDetailsPanel
            listingDetails={listingDetails}
            categoryName={ad.categoryName}
            adId={ad.id}
            createdTime={ad.createdTime}
            description={ad.description}
            content={ad.content}
          />
          <AdReviewsPanel advertisementId={ad.id} sellerUserId={ad.userId} isOwner={!!isOwner} />
          <ListingQaPanel advertisementId={ad.id} isOwner={!!isOwner} />
        </div>
      </div>

      <SimilarAdsSection advertisementId={ad.id} />
    </article>
  );
}
