"use client";

import Link from "next/link";
import { useLocale } from "@/context/LocaleContext";
import {
  formatPrice,
  getListingDetailRows,
  getLocationLine,
  parseListingDetails,
  type ListingDetails,
} from "@/lib/listingDetails";
import { formatDate } from "@/lib/status";

interface ListingDetailsPanelProps {
  listingDetails?: ListingDetails | null;
  categoryName: string;
  adId: number;
  createdTime: string;
  description: string;
  content: string;
}

export function ListingDetailsPanel({
  listingDetails,
  categoryName,
  adId,
  createdTime,
  description,
  content,
}: ListingDetailsPanelProps) {
  const { t, locale } = useLocale();
  const details = parseListingDetails(listingDetails);
  const location = getLocationLine(details);
  const rows = getListingDetailRows(details, categoryName, t, {
    yes: t("common.yes"),
    no: t("common.no"),
  });

  const urlLabels = new Set([
    t("row.expertReport"),
    t("row.floorPlan"),
    t("row.video"),
    t("row.virtualTour"),
  ]);

  const tableRows: { label: string; value: string }[] = [
    { label: t("ad.listingNo"), value: String(adId) },
    { label: t("ad.listingDate"), value: formatDate(createdTime, locale) },
    ...rows,
  ];

  return (
    <div className="space-y-6">
      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white dark:border-slate-700 dark:bg-slate-900">
        <div className="border-b border-slate-200 bg-slate-50 px-4 py-3 dark:border-slate-700 dark:bg-slate-800">
          <h2 className="text-sm font-bold uppercase tracking-wide text-slate-800 dark:text-slate-100">
            {t("ad.detailsTable")}
          </h2>
        </div>
        <dl className="divide-y divide-slate-100 dark:divide-slate-800">
          {tableRows.map((row) => (
            <div
              key={row.label}
              className="grid grid-cols-[minmax(140px,38%)_1fr] gap-3 px-4 py-3 text-sm sm:grid-cols-[200px_1fr]"
            >
              <dt className="font-medium text-slate-500 dark:text-slate-400">{row.label}</dt>
              <dd className="text-slate-900 dark:text-slate-100">
                {urlLabels.has(row.label) && /^https?:\/\//i.test(row.value) ? (
                  <Link href={row.value} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline dark:text-blue-400">
                    {t("common.openLink")}
                  </Link>
                ) : (
                  row.value
                )}
              </dd>
            </div>
          ))}
          {location && (
            <div className="grid grid-cols-[minmax(140px,38%)_1fr] gap-3 px-4 py-3 text-sm sm:grid-cols-[200px_1fr]">
              <dt className="font-medium text-slate-500 dark:text-slate-400">{t("ad.location")}</dt>
              <dd className="text-slate-900 dark:text-slate-100">{location}</dd>
            </div>
          )}
        </dl>
      </div>

      <div>
        <h2 className="text-sm font-bold uppercase tracking-wide text-slate-500 dark:text-slate-400">
          {t("ad.description")}
        </h2>
        <p className="mt-2 text-slate-700 dark:text-slate-300">{description}</p>
        {content && content !== description && (
          <div className="mt-4 whitespace-pre-wrap rounded-xl bg-slate-50 p-4 text-sm text-slate-700 dark:bg-slate-800 dark:text-slate-300">
            {content}
          </div>
        )}
      </div>
    </div>
  );
}

export function ListingPriceHeader({
  listingDetails,
  title,
}: {
  listingDetails?: ListingDetails | null;
  title: string;
}) {
  const { t, locale } = useLocale();
  const details = parseListingDetails(listingDetails);
  const location = getLocationLine(details);

  return (
    <div>
      <h1 className="text-xl font-bold text-slate-900 dark:text-white sm:text-2xl">{title}</h1>
      <p className="mt-2 text-3xl font-bold text-slate-900 dark:text-white">
        {formatPrice(details.price, t("price.notSet"), locale)}
      </p>
      {location && (
        <p className="mt-2 flex items-center gap-1.5 text-sm text-slate-600 dark:text-slate-400">
          <svg className="h-4 w-4 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
            />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          {location}
        </p>
      )}
      <div className="mt-3 flex flex-wrap gap-2">
        {details.condition && (
          <span className="rounded-lg bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-700 dark:bg-slate-800 dark:text-slate-200">
            {details.condition}
          </span>
        )}
        {details.brand && (
          <span className="rounded-lg bg-blue-50 px-2.5 py-1 text-xs font-medium text-blue-800 dark:bg-blue-950 dark:text-blue-200">
            {details.brand}
            {details.model ? ` · ${details.model}` : ""}
          </span>
        )}
        {details.sellerType && (
          <span className="rounded-lg bg-amber-50 px-2.5 py-1 text-xs font-medium text-amber-900 dark:bg-amber-950 dark:text-amber-200">
            {details.sellerType}
          </span>
        )}
        {details.swap && (
          <span className="rounded-lg bg-teal-50 px-2.5 py-1 text-xs font-medium text-teal-800 dark:bg-teal-950 dark:text-teal-300">
            {t("listing.swapLabel")}
          </span>
        )}
      </div>
    </div>
  );
}
