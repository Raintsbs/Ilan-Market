"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { subscribeRealtime } from "@/lib/realtimeEvents";
import { formatDate } from "@/lib/status";
import type { MessageKey } from "@/lib/i18n/messages";
import { OfferRespondButtons } from "@/components/OfferRespondButtons";
import { PageHeader } from "@/components/PageHeader";
import { listItemCard, pageContainerMd } from "@/lib/uiStyles";
import type { Offer } from "@/lib/types";

function offerStatusLabel(status: number, t: (k: MessageKey) => string) {
  if (status === 1) return t("offers.statusAccepted");
  if (status === 2) return t("offers.statusRejected");
  return t("offers.statusPending");
}

export default function OffersInboxPage() {
  const router = useRouter();
  const { t, locale } = useLocale();
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const [items, setItems] = useState<Offer[]>([]);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    const res = await api.getIncomingOffers();
    if (res.success && res.data) setItems(res.data);
    setLoading(false);
  }, []);

  function handleOfferUpdated(updated: Offer) {
    setItems((prev) => prev.map((o) => (o.id === updated.id ? updated : o)));
  }

  useEffect(() => {
    if (!authLoading && !isAuthenticated) router.replace("/giris?redirect=/teklifler");
  }, [authLoading, isAuthenticated, router]);

  useEffect(() => {
    if (!isAuthenticated) return;
    load();
    const unsub = subscribeRealtime("offers", () => load());
    const id = window.setInterval(load, 8000);
    return () => {
      unsub();
      clearInterval(id);
    };
  }, [isAuthenticated, load]);

  if (authLoading) return <LoadingSpinner />;
  if (!isAuthenticated) return null;

  return (
    <div className={pageContainerMd}>
      <PageHeader title={t("offers.inboxTitle")} subtitle={t("offers.inboxSubtitle")} />

      {loading ? (
        <LoadingSpinner />
      ) : items.length === 0 ? (
        <p className="mt-10 text-center text-slate-500">{t("offers.empty")}</p>
      ) : (
        <ul className="mt-8 space-y-3">
          {items.map((offer) => (
            <li
              key={offer.id}
              className={listItemCard}
            >
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <Link
                    href={`/ilan/${offer.advertisementId}`}
                    className="font-semibold text-blue-600 hover:underline dark:text-blue-400"
                  >
                    {offer.advertisementTitle ?? t("offers.adFallback")}
                  </Link>
                  <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">
                    {offer.buyerName ?? t("offers.anonymousBuyer")}
                  </p>
                </div>
                <span className="text-lg font-bold text-slate-900 dark:text-white">
                  {Number(offer.amount).toLocaleString(locale === "tr" ? "tr-TR" : "en-US")} TL
                </span>
              </div>
              {offer.message && (
                <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{offer.message}</p>
              )}
              <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-xs text-slate-400">
                <span>{formatDate(offer.createdTime, locale)}</span>
                <span className="rounded-full bg-slate-100 px-2 py-0.5 font-medium text-slate-600 dark:bg-slate-800 dark:text-slate-300">
                  {offerStatusLabel(offer.status, t)}
                </span>
              </div>
              <OfferRespondButtons offer={offer} onUpdated={handleOfferUpdated} className="mt-3" />
              <Link
                href={
                  offer.messageThreadId
                    ? `/mesajlar/${offer.messageThreadId}`
                    : `/mesajlar?ad=${offer.advertisementId}`
                }
                className="mt-3 inline-block text-sm font-medium text-blue-600 hover:underline dark:text-blue-400"
              >
                {t("contact.message")} →
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
