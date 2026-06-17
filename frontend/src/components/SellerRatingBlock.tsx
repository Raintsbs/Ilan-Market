"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { StarRating } from "@/components/StarRating";
import { ReviewComposer, type ReviewComposerState } from "@/components/ReviewComposer";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api, ApiError } from "@/lib/api";
import { formatDate } from "@/lib/status";
import type { SellerRatingSummary } from "@/lib/types";

type SellerRatingBlockProps = {
  sellerUserId: number;
  completedOrderCount?: number;
  compact?: boolean;
  showComposer?: boolean;
  isSelf?: boolean;
};

export function SellerRatingBlock({
  sellerUserId,
  completedOrderCount,
  compact,
  showComposer = false,
  isSelf,
}: SellerRatingBlockProps) {
  const { t, locale } = useLocale();
  const { isAuthenticated, user } = useAuth();
  const { showToast } = useToast();
  const [rating, setRating] = useState<SellerRatingSummary | null>(null);
  const [page, setPage] = useState(1);
  const [stars, setStars] = useState(5);
  const [comment, setComment] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const self = isSelf ?? (user?.userId === sellerUserId);

  useEffect(() => {
    api.getSellerRating(sellerUserId, page).then((res) => {
      if (res.success && res.data) setRating(res.data);
    });
  }, [sellerUserId, page]);

  async function reload() {
    const res = await api.getSellerRating(sellerUserId, page);
    if (res.success && res.data) setRating(res.data);
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!rating?.reviewOrderId) return;
    setSubmitting(true);
    try {
      const res = await api.createSellerReview(sellerUserId, rating.reviewOrderId, stars, comment || undefined);
      if (res.success) {
        showToast(t("reviews.sent"), "success");
        setComment("");
        await reload();
      } else {
        showToast(res.message || t("reviews.failed"), "error");
      }
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("reviews.failed"), "error");
    } finally {
      setSubmitting(false);
    }
  }

  const composerState: ReviewComposerState = (() => {
    if (self) return "owner";
    if (!isAuthenticated) return "login";
    if (rating?.canReview) return "ready";
    if (rating?.alreadyReviewed) return "done";
    return "purchase";
  })();

  if (!rating) return null;

  if (compact) {
    if (rating.reviewCount === 0) return null;
    return (
      <Link
        href={`/satici/${sellerUserId}#degerlendirmeler`}
        className="inline-flex items-center gap-1.5 text-xs text-slate-600 hover:text-blue-600 dark:text-slate-400"
      >
        <StarRating rating={rating.averageRating} size="sm" showValue />
        <span>({rating.reviewCount})</span>
      </Link>
    );
  }

  return (
    <div id="degerlendirmeler" className="scroll-mt-24 space-y-4">
      {showComposer && (
        <form onSubmit={handleSubmit}>
          <ReviewComposer
            title={t("reviews.writeSeller")}
            state={composerState}
            rating={stars}
            onRatingChange={setStars}
            comment={comment}
            onCommentChange={setComment}
            submitting={submitting}
            loginRedirect={`/satici/${sellerUserId}#degerlendirmeler`}
            purchaseHref="/siparisler"
          />
        </form>
      )}

      <div className="flex flex-wrap items-center gap-3">
        {rating.reviewCount > 0 ? (
          <>
            <StarRating rating={rating.averageRating} size="lg" showValue />
            <span className="text-sm text-slate-500">{t("reviews.count", { count: rating.reviewCount })}</span>
          </>
        ) : (
          <p className="text-sm text-slate-500">{t("reviews.sellerEmpty")}</p>
        )}
        {completedOrderCount != null && completedOrderCount > 0 && (
          <span className="text-sm text-slate-500">
            · {t("store.completedSales")}: {completedOrderCount}
          </span>
        )}
      </div>

      {rating.recentReviews.length > 0 && (
        <ul className="space-y-4">
          {rating.recentReviews.map((rev) => (
            <li key={rev.id} className="border-t border-slate-100 pt-4 dark:border-slate-700">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium text-slate-900 dark:text-white">
                    {rev.buyerName || t("reviews.anonymous")}
                  </span>
                  {rev.isVerifiedPurchase && (
                    <span className="rounded-full bg-emerald-100 px-2 py-0.5 text-[10px] font-semibold uppercase text-emerald-800 dark:bg-emerald-900/40 dark:text-emerald-200">
                      {t("reviews.verifiedPurchase")}
                    </span>
                  )}
                </div>
                <span className="text-xs text-slate-500">{formatDate(rev.createdTime, locale)}</span>
              </div>
              <div className="mt-1">
                <StarRating rating={rev.rating} size="sm" />
              </div>
              {rev.comment && (
                <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">{rev.comment}</p>
              )}
            </li>
          ))}
        </ul>
      )}

      {rating.totalPages > 1 && (
        <div className="flex items-center gap-3">
          <button
            type="button"
            disabled={page <= 1}
            onClick={() => setPage((p) => p - 1)}
            className="rounded-lg border border-slate-200 px-3 py-1.5 text-sm disabled:opacity-40 dark:border-slate-700"
          >
            {t("common.prev")}
          </button>
          <span className="text-sm text-slate-500">
            {page} / {rating.totalPages}
          </span>
          <button
            type="button"
            disabled={page >= rating.totalPages}
            onClick={() => setPage((p) => p + 1)}
            className="rounded-lg border border-slate-200 px-3 py-1.5 text-sm disabled:opacity-40 dark:border-slate-700"
          >
            {t("common.next")}
          </button>
        </div>
      )}
    </div>
  );
}
