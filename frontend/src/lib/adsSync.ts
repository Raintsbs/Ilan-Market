"use client";

import { useEffect } from "react";

/** Sekmeler arası ilan listesi senkronu (admin silince ana site yenilensin). */
export const ADS_SYNC_KEY = "ilanmarket_ads_version";

export function notifyAdsChanged() {
  if (typeof window === "undefined") return;
  const version = String(Date.now());
  localStorage.setItem(ADS_SYNC_KEY, version);
  window.dispatchEvent(new CustomEvent("ads-changed", { detail: version }));
}

export function useAdsChangeListener(onRefresh: () => void) {
  useEffect(() => {
    const refresh = () => onRefresh();

    const onStorage = (e: StorageEvent) => {
      if (e.key === ADS_SYNC_KEY) refresh();
    };

    window.addEventListener("ads-changed", refresh);
    window.addEventListener("storage", onStorage);
    return () => {
      window.removeEventListener("ads-changed", refresh);
      window.removeEventListener("storage", onStorage);
    };
  }, [onRefresh]);
}
