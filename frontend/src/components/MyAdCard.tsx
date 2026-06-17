"use client";

import Link from "next/link";
import { useState } from "react";
import { AdCard } from "@/components/AdCard";
import { StatusBadge } from "@/components/StatusBadge";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api, ApiError } from "@/lib/api";
import { AdvertisementStatus, type AdAnalytics, type Advertisement } from "@/lib/types";

export function MyAdCard({ ad }: { ad: Advertisement }) {
  const { t } = useLocale();
  const { showToast } = useToast();
  const [busy, setBusy] = useState(false);
  const [analytics, setAnalytics] = useState<AdAnalytics | null>(null);
  const [loadingAnalytics, setLoadingAnalytics] = useState(false);
  const isLive = ad.status === AdvertisementStatus.Approved && ad.isActive;
  const isRejected = ad.status === AdvertisementStatus.Rejected;
  const isPending = ad.status === AdvertisementStatus.Pending;

  return (
    <div className="flex flex-col gap-2">
      {(isRejected || isPending) && (
        <div
          className={`rounded-xl border px-4 py-3 text-sm ${
            isRejected
              ? "border-rose-200 bg-rose-50 text-rose-800 dark:border-rose-900 dark:bg-rose-950/60 dark:text-rose-200"
              : "border-amber-200 bg-amber-50 text-amber-900 dark:border-amber-900 dark:bg-amber-950/50 dark:text-amber-200"
          }`}
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <StatusBadge status={ad.status} isActive={ad.isActive} />
            {isRejected && (
              <span className="text-xs font-medium">{t("myAds.rejectedHint")}</span>
            )}
            {isPending && (
              <span className="text-xs font-medium">{t("myAds.pendingHint")}</span>
            )}
          </div>
          {isRejected && ad.rejectReason && (
            <p className="mt-2 text-xs leading-relaxed opacity-90">
              <span className="font-semibold">{t("myAds.rejectReason")}: </span>
              {ad.rejectReason}
            </p>
          )}
        </div>
      )}
      <AdCard ad={ad} showStatus />
      {isLive && (
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            disabled={busy}
            onClick={async () => {
              setBusy(true);
              try {
                const res = await api.bumpAdvertisement(ad.id);
                showToast(res.message || t("myAds.bumpOk"), res.success ? "success" : "error");
              } catch (e) {
                showToast(e instanceof ApiError ? e.message : t("featured.failed"), "error");
              } finally {
                setBusy(false);
              }
            }}
            className="flex-1 rounded-lg border border-blue-200 bg-blue-50 px-2 py-1.5 text-xs font-semibold text-blue-700 dark:border-blue-800 dark:bg-blue-950 dark:text-blue-300"
          >
            {t("myAds.bump")}
          </button>
          <button
            type="button"
            disabled={busy}
            onClick={async () => {
              setBusy(true);
              try {
                const res = await api.extendAdvertisement(ad.id, 30);
                showToast(res.message || t("myAds.extendOk"), res.success ? "success" : "error");
              } catch (e) {
                showToast(e instanceof ApiError ? e.message : t("featured.failed"), "error");
              } finally {
                setBusy(false);
              }
            }}
            className="flex-1 rounded-lg border border-slate-200 px-2 py-1.5 text-xs font-semibold text-slate-700 dark:border-slate-600 dark:text-slate-300"
          >
            {t("myAds.extend")}
          </button>
        </div>
      )}
      <Link
        href={`/ilan/${ad.id}/duzenle`}
        className="text-center text-xs font-medium text-blue-600 hover:underline dark:text-blue-400"
      >
        {t("ad.edit")} →
      </Link>
      {(ad.viewCount ?? 0) > 0 && (
        <p className="text-center text-xs text-slate-500">
          {ad.viewCount} {t("seller.views").toLowerCase()}
        </p>
      )}
      {isLive && (
        <div className="text-center">
          {!analytics ? (
            <button
              type="button"
              disabled={loadingAnalytics}
              onClick={async () => {
                setLoadingAnalytics(true);
                const res = await api.getAdAnalytics(ad.id);
                setLoadingAnalytics(false);
                if (res.success && res.data) setAnalytics(res.data);
              }}
              className="text-xs font-medium text-slate-600 hover:underline dark:text-slate-400"
            >
              {loadingAnalytics ? "…" : t("seller.statsTitle")}
            </button>
          ) : (
            <p className="text-xs text-slate-500">
              {analytics.viewCount} {t("seller.views").toLowerCase()} · {analytics.offerCount}{" "}
              {t("seller.statOffers").toLowerCase()} · {analytics.messageThreadCount}{" "}
              {t("seller.statMessages").toLowerCase()}
            </p>
          )}
        </div>
      )}
    </div>
  );
}
