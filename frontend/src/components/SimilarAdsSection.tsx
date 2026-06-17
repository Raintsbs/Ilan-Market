"use client";

import { useEffect, useState } from "react";
import { AdCard } from "@/components/AdCard";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import type { Advertisement } from "@/lib/types";

type SimilarAdsSectionProps = {
  advertisementId: number;
};

export function SimilarAdsSection({ advertisementId }: SimilarAdsSectionProps) {
  const { t } = useLocale();
  const [items, setItems] = useState<Advertisement[]>([]);

  useEffect(() => {
    api
      .getSimilarAdvertisements(advertisementId, 8)
      .then((res) => {
        if (res.success && res.data) setItems(res.data);
      })
      .catch(() => {
        /* optional: API restart gerekebilir (/similar 404) */
      });
  }, [advertisementId]);

  if (items.length === 0) return null;

  return (
    <section className="mt-12">
      <div>
        <h2 className="text-xl font-bold text-slate-900 dark:text-white">{t("similar.title")}</h2>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t("similar.subtitle")}</p>
      </div>
      <div className="mt-6 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {items.map((ad) => (
          <AdCard key={ad.id} ad={ad} />
        ))}
      </div>
    </section>
  );
}
