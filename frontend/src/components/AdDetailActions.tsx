"use client";

import Link from "next/link";
import { useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api, ApiError } from "@/lib/api";
import { formFieldClass } from "@/lib/formStyles";
import { parseListingDetails } from "@/lib/listingDetails";
import { AdvertisementStatus, type Advertisement } from "@/lib/types";
import { CopyButton } from "./CopyButton";
import { VerifiedBadge } from "./VerifiedBadge";

interface AdDetailActionsProps {
  ad: Advertisement;
  isOwner: boolean;
  sellerVerified?: boolean;
}

export function AdDetailActions({ ad, isOwner, sellerVerified }: AdDetailActionsProps) {
  const { t } = useLocale();
  const { isAuthenticated } = useAuth();
  const { showToast } = useToast();
  const [offerOpen, setOfferOpen] = useState(false);
  const [reportOpen, setReportOpen] = useState(false);
  const [amount, setAmount] = useState("");
  const [offerMsg, setOfferMsg] = useState("");
  const [reportReason, setReportReason] = useState("spam");
  const [reportDetails, setReportDetails] = useState("");
  const [loading, setLoading] = useState(false);
  const [phone, setPhone] = useState<string | null>(null);
  const [phoneLoading, setPhoneLoading] = useState(false);

  const isLive = ad.status === AdvertisementStatus.Approved && ad.isActive;
  const buyPrice = parseListingDetails(ad.listingDetails).price;
  const canBuy = !isOwner && isLive && typeof buyPrice === "number" && buyPrice > 0;
  const shareUrl = typeof window !== "undefined" ? window.location.href : "";
  const waText = encodeURIComponent(`${ad.title} - ${shareUrl}`);
  const waLink = `https://wa.me/?text=${waText}`;

  function requireAuth(action: () => void) {
    if (!isAuthenticated) {
      window.location.href = `/giris?redirect=/ilan/${ad.id}`;
      return;
    }
    action();
  }

  async function submitOffer() {
    const val = Number(amount);
    if (!val || val <= 0) return;
    setLoading(true);
    try {
      await api.createOffer(ad.id, val, offerMsg || undefined);
      showToast(t("offer.sent"), "success");
      setOfferOpen(false);
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("offer.failed"), "error");
    } finally {
      setLoading(false);
    }
  }

  async function submitReport() {
    setLoading(true);
    try {
      await api.reportListing(ad.id, reportReason, reportDetails || undefined);
      showToast(t("report.sent"), "success");
      setReportOpen(false);
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("report.failed"), "error");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="flex flex-col gap-3">
      {ad.userDisplayName && (
        <div className="rounded-xl border border-slate-200 bg-slate-50 p-4 dark:border-slate-700 dark:bg-slate-800/50">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{t("ad.owner")}</p>
          <div className="mt-1 flex flex-wrap items-center gap-2">
            <p className="font-semibold text-slate-900 dark:text-white">{ad.userDisplayName}</p>
            {sellerVerified && <VerifiedBadge />}
          </div>
        </div>
      )}

      {typeof ad.viewCount === "number" && (
        <p className="text-xs text-slate-500 dark:text-slate-400">
          {t("analytics.views", { count: ad.viewCount })}
        </p>
      )}

      {canBuy && (
        <button
          type="button"
          onClick={() => requireAuth(() => (window.location.href = `/satin-al?adId=${ad.id}`))}
          className="w-full rounded-xl bg-emerald-600 px-4 py-3 text-center text-sm font-bold text-white hover:bg-emerald-700"
        >
          {t("buy.button")}
        </button>
      )}

      {!isOwner && (
        <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
          <button
            type="button"
            onClick={() => requireAuth(() => (window.location.href = `/mesajlar?ad=${ad.id}`))}
            className="rounded-xl border border-blue-200 bg-blue-50 px-3 py-2.5 text-sm font-semibold text-blue-700 hover:bg-blue-100 dark:border-blue-800 dark:bg-blue-950 dark:text-blue-300"
          >
            {t("contact.message")}
          </button>
          <button
            type="button"
            disabled={phoneLoading}
            onClick={() =>
              requireAuth(async () => {
                if (phone) return;
                setPhoneLoading(true);
                try {
                  const res = await api.revealPhone(ad.id);
                  if (res.success && res.data) setPhone(res.data.phoneNumber);
                  else showToast(res.message || t("featured.failed"), "error");
                } catch (e) {
                  const msg = e instanceof ApiError ? e.message : t("featured.failed");
                  showToast(msg.includes("limit") ? t("phone.rateLimit") : msg, "error");
                } finally {
                  setPhoneLoading(false);
                }
              })
            }
            className="rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-sm font-semibold text-slate-800 hover:bg-slate-50 dark:border-slate-600 dark:bg-slate-900 dark:text-slate-200"
          >
            {phone ? `${t("phone.revealed")}: ${phone}` : t("phone.show")}
          </button>
          <button
            type="button"
            onClick={() => requireAuth(() => setOfferOpen(true))}
            className="rounded-lg border border-blue-200 bg-blue-50 px-3 py-2.5 text-sm font-semibold text-blue-700 hover:bg-blue-100 dark:border-blue-800 dark:bg-blue-950"
          >
            {t("offer.make")}
          </button>
          <a
            href={waLink}
            target="_blank"
            rel="noopener noreferrer"
            className="col-span-2 rounded-xl border border-green-200 bg-green-50 px-3 py-2.5 text-center text-sm font-semibold text-green-800 hover:bg-green-100 dark:border-green-900 dark:bg-green-950 dark:text-green-300"
          >
            {t("contact.whatsapp")}
          </a>
          <CopyButton
            text={shareUrl}
            className="col-span-1 w-full"
            onCopied={() => showToast(t("share.copied"), "success")}
            onError={() => showToast(t("share.failed"), "error")}
          />
          <button
            type="button"
            onClick={() => requireAuth(() => setReportOpen(true))}
            className="rounded-xl border border-rose-200 px-3 py-2 text-sm font-medium text-rose-600 dark:border-rose-900"
          >
            {t("report.title")}
          </button>
        </div>
      )}

      {isOwner && isLive && (
        <Link
          href={`/one-cikan?adId=${ad.id}`}
          className="rounded-xl border border-amber-200 bg-amber-50 px-4 py-2.5 text-center text-sm font-semibold text-amber-900 hover:bg-amber-100 dark:border-amber-900 dark:bg-amber-950 dark:text-amber-200"
        >
          {t("featured.promote")}
        </Link>
      )}

      {offerOpen && (
        <div className="rounded-xl border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900">
          <p className="font-semibold text-slate-900 dark:text-white">{t("offer.make")}</p>
          <input
            type="number"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            placeholder={t("offer.amount")}
            className={`mt-2 ${formFieldClass}`}
          />
          <textarea
            value={offerMsg}
            onChange={(e) => setOfferMsg(e.target.value)}
            placeholder={t("offer.note")}
            rows={2}
            className={`mt-2 ${formFieldClass}`}
          />
          <div className="mt-3 flex gap-2">
            <button type="button" onClick={submitOffer} disabled={loading} className="flex-1 rounded-lg bg-blue-600 py-2 text-sm font-semibold text-white disabled:opacity-60">
              {t("offer.send")}
            </button>
            <button type="button" onClick={() => setOfferOpen(false)} className="rounded-lg border border-slate-200 px-4 py-2 text-sm dark:border-slate-600">
              {t("common.cancel")}
            </button>
          </div>
        </div>
      )}

      {reportOpen && (
        <div className="rounded-xl border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900">
          <p className="font-semibold text-slate-900 dark:text-white">{t("report.title")}</p>
          <select
            value={reportReason}
            onChange={(e) => setReportReason(e.target.value)}
            className={`mt-2 ${formFieldClass}`}
          >
            <option value="spam">{t("report.spam")}</option>
            <option value="fake">{t("report.fake")}</option>
            <option value="offensive">{t("report.offensive")}</option>
            <option value="other">{t("report.other")}</option>
          </select>
          <textarea
            value={reportDetails}
            onChange={(e) => setReportDetails(e.target.value)}
            rows={2}
            className={`mt-2 ${formFieldClass}`}
          />
          <div className="mt-3 flex gap-2">
            <button type="button" onClick={submitReport} disabled={loading} className="flex-1 rounded-lg bg-rose-600 py-2 text-sm font-semibold text-white disabled:opacity-60">
              {t("report.send")}
            </button>
            <button type="button" onClick={() => setReportOpen(false)} className="rounded-lg border border-slate-200 px-4 py-2 text-sm dark:border-slate-600 dark:text-slate-200">
              {t("common.cancel")}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
