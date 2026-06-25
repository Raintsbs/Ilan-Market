"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { StarRating } from "@/components/StarRating";
import { ReviewComposer, type ReviewComposerState } from "@/components/ReviewComposer";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api, ApiError } from "@/lib/api";
import { formatDate } from "@/lib/status";
import { surfaceCardPad } from "@/lib/uiStyles";
import type { AdvertisementRatingSummary } from "@/lib/types";

type AdReviewsPanelProps = {
  advertisementId: number;
  sellerUserId: number;
  isOwner?: boolean;
};

export function AdReviewsPanel({ advertisementId, sellerUserId, isOwner }: AdReviewsPanelProps) {
  const { t, locale } = useLocale();
  const { isAuthenticated } = useAuth();
  const { showToast } = useToast();
  const [data, setData] = useState<AdvertisementRatingSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.getAdvertisementRating(advertisementId, page);
      if (res.success && res.data) setData(res.data);
    } finally {
      setLoading(false);
    }
  }, [advertisementId, page]);

  useEffect(() => {
    void load();
  }, [load]);

  const composerState: ReviewComposerState = (() => {
    if (isOwner) return "owner";
    if (!isAuthenticated) return "login";
    if (data?.canReview) return "ready";
    if (data?.alreadyReviewed) return "done";
    return "purchase";
  })();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!data?.reviewOrderId) return;
    setSubmitting(true);
    try {
      const res = await api.createAdvertisementReview(
        advertisementId,
        data.reviewOrderId,
        rating,
        comment || undefined,
      );
      if (res.success) {
        showToast(t("reviews.adSent"), "success");
        setComment("");
        setPage(1);
        await load();
      } else {
        showToast(res.message || t("reviews.failed"), "error");
      }
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("reviews.failed"), "error");
    } finally {
      setSubmitting(false);
    }
  }

  if (loading && !data) return <LoadingSpinner />;

  return (
    <section id="degerlendirmeler" className={`scroll-mt-24 ${surfaceCardPad}`}>
      <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t("reviews.adTitle")}</h2>
      <p className="mt-1 text-sm text-slate-500">{t("reviews.adSubtitle")}</p>

      <form onSubmit={handleSubmit} className="mt-6">
        <ReviewComposer
          title={t("reviews.writeAd")}
          state={composerState}
          rating={rating}
          onRatingChange={setRating}
          comment={comment}
          onCommentChange={setComment}
          submitting={submitting}
          loginRedirect={`/ilan/${advertisementId}#degerlendirmeler`}
          purchaseHref={`/satin-al?adId=${advertisementId}`}
        />
      </form>

      <div className="mt-8 border-t border-slate-100 pt-6 dark:border-slate-800">
        {data && data.reviewCount > 0 ? (
          <div className="flex flex-wrap items-center gap-3">
            <StarRating rating={data.averageRating} size="lg" showValue />
            <span className="text-sm text-slate-500">{t("reviews.count", { count: data.reviewCount })}</span>
          </div>
        ) : (
          <p className="text-sm text-slate-500">{t("reviews.adEmpty")}</p>
        )}

        {data && data.reviews.length > 0 && (
          <ul className="mt-6 space-y-4">
            {data.reviews.map((rev) => (
              <li key={rev.id} className="rounded-xl border border-slate-100 p-4 dark:border-slate-800">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-medium text-slate-900 dark:text-white">
                      {rev.userName || t("reviews.anonymous")}
                    </span>
                    {rev.isVerifiedPurchase && (
                      <span className="rounded-full bg-emerald-100 px-2 py-0.5 text-[10px] font-semibold uppercase text-emerald-800 dark:bg-emerald-900/40 dark:text-emerald-200">
                        {t("reviews.verifiedPurchase")}
                      </span>
                    )}
                  </div>
                  <span className="text-xs text-slate-500">{formatDate(rev.createdTime, locale)}</span>
                </div>
                <div className="mt-2">
                  <StarRating rating={rev.rating} size="sm" />
                </div>
                {rev.comment && (
                  <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">{rev.comment}</p>
                )}
              </li>
            ))}
          </ul>
        )}

        {data && data.totalPages > 1 && (
          <div className="mt-4 flex items-center gap-3">
            <button
              type="button"
              disabled={page <= 1}
              onClick={() => setPage((p) => p - 1)}
              className="rounded-lg border border-slate-200 px-3 py-1.5 text-sm disabled:opacity-40 dark:border-slate-700"
            >
              {t("common.prev")}
            </button>
            <span className="text-sm text-slate-500">
              {page} / {data.totalPages}
            </span>
            <button
              type="button"
              disabled={page >= data.totalPages}
              onClick={() => setPage((p) => p + 1)}
              className="rounded-lg border border-slate-200 px-3 py-1.5 text-sm disabled:opacity-40 dark:border-slate-700"
            >
              {t("common.next")}
            </button>
          </div>
        )}
      </div>

      {!isOwner && sellerUserId > 0 && (
        <p className="mt-6 text-center text-sm text-slate-500">
          <Link href={`/satici/${sellerUserId}#degerlendirmeler`} className="text-blue-600 hover:underline dark:text-blue-400">
            {t("reviews.sellerReviewLink")}
          </Link>
        </p>
      )}
    </section>
  );
}
