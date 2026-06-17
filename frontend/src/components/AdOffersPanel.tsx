"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { OfferRespondButtons } from "@/components/OfferRespondButtons";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { subscribeRealtime } from "@/lib/realtimeEvents";
import { formatDate } from "@/lib/status";
import type { MessageKey } from "@/lib/i18n/messages";
import type { Offer } from "@/lib/types";

function offerStatusLabel(status: number, t: (k: MessageKey) => string) {
  if (status === 1) return t("offers.statusAccepted");
  if (status === 2) return t("offers.statusRejected");
  return t("offers.statusPending");
}

export function AdOffersPanel({ advertisementId }: { advertisementId: number }) {
  const { t, locale } = useLocale();
  const [offers, setOffers] = useState<Offer[]>([]);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    const res = await api.getOffers(advertisementId, true);
    if (res.success && res.data) setOffers(res.data);
    setLoading(false);
  }, [advertisementId]);

  useEffect(() => {
    load();
    const unsub = subscribeRealtime("offers", (payload) => {
      const p = payload as { advertisementId?: number } | undefined;
      if (!p?.advertisementId || p.advertisementId === advertisementId) load();
    });
    return unsub;
  }, [advertisementId, load]);

  function handleOfferUpdated(updated: Offer) {
    setOffers((prev) => prev.map((o) => (o.id === updated.id ? updated : o)));
  }

  if (loading) return null;
  if (offers.length === 0) return null;

  return (
    <div className="rounded-xl border border-slate-200 bg-slate-50 p-4 dark:border-slate-700 dark:bg-slate-800/50">
      <div className="flex items-center justify-between gap-2">
        <h3 className="font-semibold text-slate-900 dark:text-white">{t("offers.onThisAd")}</h3>
        <Link href="/teklifler" className="text-xs font-medium text-blue-600 hover:underline dark:text-blue-400">
          {t("offers.viewAll")}
        </Link>
      </div>
      <ul className="mt-3 space-y-2">
        {offers.slice(0, 5).map((o) => (
          <li key={o.id} className="rounded-lg bg-white px-3 py-2 text-sm dark:bg-slate-900">
            <div className="flex flex-wrap items-start justify-between gap-2">
              <div>
                <span className="font-semibold text-slate-900 dark:text-white">
                  {Number(o.amount).toLocaleString(locale === "tr" ? "tr-TR" : "en-US")} TL
                </span>
                <span className="text-slate-500"> — {o.buyerName ?? t("offers.anonymousBuyer")}</span>
              </div>
              <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-600 dark:bg-slate-800 dark:text-slate-300">
                {offerStatusLabel(o.status, t)}
              </span>
            </div>
            {o.message && <p className="mt-1 text-slate-500 dark:text-slate-400">{o.message}</p>}
            <span className="mt-1 block text-xs text-slate-400">{formatDate(o.createdTime, locale)}</span>
            <OfferRespondButtons offer={o} onUpdated={handleOfferUpdated} className="mt-2" />
          </li>
        ))}
      </ul>
    </div>
  );
}
