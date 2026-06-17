"use client";

import Link from "next/link";
import { useLocale } from "@/context/LocaleContext";

export type MyAdsTab = "all" | "pending" | "approved" | "rejected";

export interface MyAdCounts {
  all: number;
  pending: number;
  approved: number;
  rejected: number;
}

interface MyAdsStatusTabsProps {
  active: MyAdsTab;
  counts: MyAdCounts | null;
}

const tabs = [
  { id: "all" as const, labelKey: "myAds.tabAll" as const },
  { id: "pending" as const, labelKey: "myAds.tabPending" as const },
  { id: "approved" as const, labelKey: "myAds.tabLive" as const },
  { id: "rejected" as const, labelKey: "myAds.tabRejected" as const },
];

function countFor(tab: MyAdsTab, counts: MyAdCounts | null): number | null {
  if (!counts) return null;
  switch (tab) {
    case "pending":
      return counts.pending;
    case "approved":
      return counts.approved;
    case "rejected":
      return counts.rejected;
    default:
      return counts.all;
  }
}

export function MyAdsStatusTabs({ active, counts }: MyAdsStatusTabsProps) {
  const { t } = useLocale();

  return (
    <div className="mt-6 flex flex-wrap gap-1 rounded-2xl border border-slate-200/80 bg-slate-100/80 p-1 dark:border-slate-700 dark:bg-slate-800/50">
      {tabs.map((tab) => {
        const n = countFor(tab.id, counts);
        const isActive = active === tab.id;
        return (
          <Link
            key={tab.id}
            href={tab.id === "all" ? "/ilanlarim" : `/ilanlarim?tab=${tab.id}`}
            className={`inline-flex flex-1 items-center justify-center gap-2 rounded-xl px-3 py-2.5 text-sm font-semibold transition sm:flex-none sm:px-4 ${
              isActive
                ? "bg-white text-slate-900 shadow-sm ring-1 ring-slate-200/80 dark:bg-slate-900 dark:text-white dark:ring-slate-600"
                : "text-slate-700 hover:text-slate-950 dark:text-slate-400 dark:hover:text-white"
            }`}
          >
            {t(tab.labelKey)}
            {n != null && (
              <span
                className={`rounded-full px-2 py-0.5 text-xs tabular-nums ${
                  isActive
                    ? "bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-200"
                    : "bg-slate-200/80 text-slate-600 dark:bg-slate-700 dark:text-slate-300"
                }`}
              >
                {n}
              </span>
            )}
          </Link>
        );
      })}
    </div>
  );
}

export function tabToStatus(tab: MyAdsTab): number | undefined {
  switch (tab) {
    case "pending":
      return 0;
    case "approved":
      return 1;
    case "rejected":
      return 2;
    default:
      return undefined;
  }
}
