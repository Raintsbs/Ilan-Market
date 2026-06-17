"use client";

import { useEffect, useState } from "react";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { formatPrice } from "@/lib/listingDetails";
import { formatDate } from "@/lib/status";
import type { PriceHistoryPoint } from "@/lib/types";

export function PriceHistoryChart({ advertisementId }: { advertisementId: number }) {
  const { t, locale } = useLocale();
  const [points, setPoints] = useState<PriceHistoryPoint[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .getPriceHistory(advertisementId)
      .then((res) => {
        if (res.success && res.data) setPoints(res.data);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [advertisementId]);

  if (loading) return null;
  if (points.length < 2) return null;

  const prices = points.map((p) => p.price);
  const min = Math.min(...prices);
  const max = Math.max(...prices);
  const range = max - min || 1;
  const w = 320;
  const h = 120;
  const pad = 8;

  const coords = points.map((p, i) => {
    const x = pad + (i / Math.max(points.length - 1, 1)) * (w - pad * 2);
    const y = pad + (1 - (p.price - min) / range) * (h - pad * 2);
    return { x, y, p };
  });

  const line = coords.map((c) => `${c.x},${c.y}`).join(" ");
  const first = points[0]!.price;
  const last = points[points.length - 1]!.price;
  const delta = last - first;
  const deltaPct = first > 0 ? ((delta / first) * 100).toFixed(1) : "0";

  return (
    <section className="rounded-xl border border-slate-200 bg-white p-5 dark:border-slate-700 dark:bg-slate-900">
      <div className="flex flex-wrap items-end justify-between gap-2">
        <h2 className="text-sm font-bold uppercase tracking-wide text-slate-800 dark:text-slate-100">
          {t("priceHistory.title")}
        </h2>
        <p
          className={`text-sm font-semibold ${
            delta < 0 ? "text-emerald-600" : delta > 0 ? "text-rose-600" : "text-slate-500"
          }`}
        >
          {delta === 0
            ? t("priceHistory.unchanged")
            : delta < 0
              ? t("priceHistory.down", { pct: Math.abs(Number(deltaPct)) })
              : t("priceHistory.up", { pct: deltaPct })}
        </p>
      </div>

      <svg viewBox={`0 0 ${w} ${h}`} className="mt-4 w-full max-w-md" aria-hidden>
        <polyline
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          className="text-blue-600 dark:text-blue-400"
          points={line}
        />
        {coords.map((c, i) => (
          <circle key={i} cx={c.x} cy={c.y} r="4" className="fill-blue-600 dark:fill-blue-400" />
        ))}
      </svg>

      <ul className="mt-4 space-y-1 text-xs text-slate-500 dark:text-slate-400">
        {points.map((p, i) => (
          <li key={i} className="flex justify-between gap-4">
            <span>{formatDate(p.recordedAt, locale)}</span>
            <span className="font-medium text-slate-800 dark:text-slate-200">{formatPrice(p.price)}</span>
          </li>
        ))}
      </ul>
    </section>
  );
}
