"use client";

import { useState } from "react";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { btnBrandSm, btnOutline } from "@/lib/uiStyles";
import type { Offer } from "@/lib/types";

type OfferRespondButtonsProps = {
  offer: Offer;
  onUpdated: (updated: Offer) => void;
  className?: string;
};

export function OfferRespondButtons({ offer, onUpdated, className }: OfferRespondButtonsProps) {
  const { t } = useLocale();
  const [loading, setLoading] = useState<"accept" | "reject" | null>(null);
  const [error, setError] = useState("");

  if (offer.status !== 0) return null;

  async function respond(accept: boolean) {
    setLoading(accept ? "accept" : "reject");
    setError("");
    const res = await api.respondToOffer(offer.id, accept);
    setLoading(null);
    if (res.success && res.data) {
      onUpdated(res.data);
      return;
    }
    setError(res.message || t("offers.respondError"));
  }

  return (
    <div className={className}>
      <div className="flex flex-wrap gap-2">
        <button
          type="button"
          disabled={loading !== null}
          onClick={() => respond(true)}
          className={`${btnBrandSm} bg-emerald-600 hover:bg-emerald-700 dark:bg-emerald-600 dark:hover:bg-emerald-500`}
        >
          {loading === "accept" ? "…" : t("offers.accept")}
        </button>
        <button
          type="button"
          disabled={loading !== null}
          onClick={() => respond(false)}
          className={`${btnOutline} h-9 px-3.5 text-sm`}
        >
          {loading === "reject" ? "…" : t("offers.reject")}
        </button>
      </div>
      {error && <p className="mt-2 text-xs text-red-600 dark:text-red-400">{error}</p>}
    </div>
  );
}
