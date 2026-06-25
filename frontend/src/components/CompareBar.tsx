"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { X } from "lucide-react";
import { useLocale } from "@/context/LocaleContext";
import { clearCompare, getCompareIds, removeFromCompare } from "@/lib/compareList";
import { btnBrandSm } from "@/lib/uiStyles";

export function CompareBar() {
  const { t } = useLocale();
  const [ids, setIds] = useState<number[]>([]);

  useEffect(() => {
    setIds(getCompareIds());
    const onChange = () => setIds(getCompareIds());
    window.addEventListener("compare-list-changed", onChange);
    return () => window.removeEventListener("compare-list-changed", onChange);
  }, []);

  if (ids.length === 0) return null;

  return (
    <div className="fixed bottom-4 left-1/2 z-40 w-[min(100%-2rem,28rem)] -translate-x-1/2 safe-bottom rounded-2xl border border-slate-200 bg-white/95 p-3 shadow-xl backdrop-blur dark:border-slate-700 dark:bg-slate-900/95">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className="text-sm font-medium text-slate-700 dark:text-slate-200">
          {t("compare.bar", { count: ids.length })}
        </p>
        <div className="flex items-center gap-2">
          <Link href={`/karsilastir?ids=${ids.join(",")}`} className={btnBrandSm}>
            {t("compare.open")}
          </Link>
          <button
            type="button"
            onClick={clearCompare}
            className="rounded-lg p-2 text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-800"
            aria-label={t("compare.clear")}
          >
            <X className="size-4" />
          </button>
        </div>
      </div>
      <div className="mt-2 flex flex-wrap gap-1">
        {ids.map((id) => (
          <span
            key={id}
            className="inline-flex items-center gap-1 rounded-lg bg-slate-100 px-2 py-1 text-xs font-medium dark:bg-slate-800"
          >
            #{id}
            <button type="button" onClick={() => removeFromCompare(id)} aria-label="Remove">
              <X className="size-3" />
            </button>
          </span>
        ))}
      </div>
    </div>
  );
}
