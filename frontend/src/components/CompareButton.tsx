"use client";

import { useEffect, useState } from "react";
import { Scale } from "lucide-react";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { getCompareIds, isInCompare, toggleCompare } from "@/lib/compareList";

export function CompareButton({ advertisementId }: { advertisementId: number }) {
  const { t } = useLocale();
  const { showToast } = useToast();
  const [active, setActive] = useState(false);

  useEffect(() => {
    setActive(isInCompare(advertisementId));
    const onChange = () => setActive(isInCompare(advertisementId));
    window.addEventListener("compare-list-changed", onChange);
    return () => window.removeEventListener("compare-list-changed", onChange);
  }, [advertisementId]);

  function handleClick() {
    const before = getCompareIds().length;
    const next = toggleCompare(advertisementId);
    setActive(next.includes(advertisementId));
    if (!active && before >= 4) {
      showToast(t("compare.max"), "error");
    } else if (active) {
      showToast(t("compare.removed"), "success");
    } else {
      showToast(t("compare.added"), "success");
    }
  }

  return (
    <button
      type="button"
      onClick={handleClick}
      className={`inline-flex items-center gap-2 rounded-xl border px-3 py-2 text-sm font-semibold transition ${
        active
          ? "border-blue-300 bg-blue-50 text-blue-700 dark:border-blue-700 dark:bg-blue-950 dark:text-blue-300"
          : "border-slate-200 text-slate-700 hover:bg-slate-50 dark:border-slate-600 dark:text-slate-200 dark:hover:bg-slate-800"
      }`}
    >
      <Scale className="size-4" />
      {active ? t("compare.inList") : t("compare.add")}
    </button>
  );
}
