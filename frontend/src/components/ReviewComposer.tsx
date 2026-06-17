"use client";

import Link from "next/link";
import { MessageSquare, Star } from "lucide-react";
import { InteractiveStarRating } from "@/components/StarRating";
import { useLocale } from "@/context/LocaleContext";
import { formFieldClass } from "@/lib/formStyles";
import { btnBrand } from "@/lib/uiStyles";

export type ReviewComposerState =
  | "ready"
  | "login"
  | "purchase"
  | "owner"
  | "done";

type ReviewComposerProps = {
  title: string;
  state: ReviewComposerState;
  rating: number;
  onRatingChange: (value: number) => void;
  comment: string;
  onCommentChange: (value: string) => void;
  submitting?: boolean;
  loginRedirect?: string;
  purchaseHref?: string;
};

export function ReviewComposer({
  title,
  state,
  rating,
  onRatingChange,
  comment,
  onCommentChange,
  submitting = false,
  loginRedirect,
  purchaseHref,
}: ReviewComposerProps) {
  const { t } = useLocale();
  const canSubmit = state === "ready";

  return (
    <div className="rounded-2xl border border-amber-200/80 bg-gradient-to-br from-amber-50/80 to-white p-5 dark:border-amber-900/40 dark:from-amber-950/20 dark:to-slate-900">
      <div className="flex items-center gap-2">
        <Star className="size-5 fill-amber-400 text-amber-400" />
        <h3 className="font-semibold text-slate-900 dark:text-white">{title}</h3>
      </div>

      <div className={`mt-4 ${!canSubmit ? "pointer-events-none opacity-60" : ""}`}>
        <p className="mb-2 text-sm font-medium text-slate-700 dark:text-slate-300">
          {t("reviews.rating")}
        </p>
        <InteractiveStarRating value={rating} onChange={onRatingChange} />
        <textarea
          value={comment}
          onChange={(e) => onCommentChange(e.target.value)}
          placeholder={t("reviews.commentPlaceholder")}
          rows={4}
          disabled={!canSubmit}
          className={`mt-4 ${formFieldClass}`}
        />
      </div>

      {state === "login" && (
        <p className="mt-3 text-sm text-slate-600 dark:text-slate-400">
          {t("reviews.loginRequired")}{" "}
          <Link
            href={`/giris?redirect=${encodeURIComponent(loginRedirect ?? "/")}`}
            className="font-semibold text-blue-600 hover:underline dark:text-blue-400"
          >
            {t("auth.loginShort")}
          </Link>
        </p>
      )}

      {state === "purchase" && (
        <p className="mt-3 text-sm text-slate-600 dark:text-slate-400">
          {t("reviews.purchaseRequired")}{" "}
          {purchaseHref && (
            <Link href={purchaseHref} className="font-semibold text-blue-600 hover:underline dark:text-blue-400">
              {t("reviews.purchaseLink")}
            </Link>
          )}
        </p>
      )}

      {state === "owner" && (
        <p className="mt-3 text-sm text-slate-600 dark:text-slate-400">{t("reviews.ownerCannot")}</p>
      )}

      {state === "done" && (
        <p className="mt-3 text-sm text-emerald-700 dark:text-emerald-400">{t("reviews.alreadyDone")}</p>
      )}

      {canSubmit && (
        <button
          type="submit"
          disabled={submitting}
          className={`mt-4 inline-flex items-center gap-2 ${btnBrand}`}
        >
          <MessageSquare className="size-4" />
          {submitting ? "…" : t("reviews.submit")}
        </button>
      )}
    </div>
  );
}
