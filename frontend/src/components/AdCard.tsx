"use client";

import Link from "next/link";
import { getAdvertisementImageUrls } from "@/lib/image";
import { useLocale } from "@/context/LocaleContext";
import { formatPrice, parseListingDetails } from "@/lib/listingDetails";
import { formatDate } from "@/lib/status";
import type { Advertisement } from "@/lib/types";
import { StarRating } from "@/components/StarRating";
import { SafeImage } from "./SafeImage";
import { StatusBadge } from "./StatusBadge";

export function AdCard({
  ad,
  showStatus = false,
}: {
  ad: Advertisement;
  showStatus?: boolean;
}) {
  const imageUrls = getAdvertisementImageUrls(ad);
  const imageUrl = imageUrls[0] ?? null;
  const { t, locale } = useLocale();
  const details = parseListingDetails(ad.listingDetails);
  const location = [details.city, details.district].filter(Boolean).join(" · ");

  return (
    <Link
      href={`/ilan/${ad.id}`}
      className="ad-card-lift group flex flex-col overflow-hidden rounded-2xl bg-white ring-1 ring-slate-200/80 dark:bg-slate-900 dark:ring-slate-700/80"
    >
      <div className="relative aspect-[5/4] overflow-hidden bg-slate-100 dark:bg-slate-800">
        {ad.isFeatured && (
          <span className="absolute right-3 top-3 z-10 rounded-full bg-amber-400/95 px-2.5 py-1 text-[10px] font-bold uppercase tracking-wide text-amber-950 shadow-sm backdrop-blur-sm">
            {t("filter.featuredOnly")}
          </span>
        )}
        {imageUrl ? (
          <SafeImage
            src={imageUrl}
            alt={ad.title}
            fill
            className="object-cover transition-transform duration-500 ease-out group-hover:scale-[1.03]"
          />
        ) : (
          <div className="flex h-full items-center justify-center text-slate-300">
            <svg className="h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={1}
                d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
              />
            </svg>
          </div>
        )}
        <div className="absolute inset-x-0 bottom-0 h-16 bg-gradient-to-t from-black/50 to-transparent opacity-0 transition-opacity group-hover:opacity-100" />
      </div>
      <div className="flex flex-1 flex-col gap-1.5 p-4">
        <p className="text-xl font-bold tracking-tight text-slate-900 dark:text-white">
          {formatPrice(details.price, t("price.notSet"), locale)}
        </p>
        <h3 className="line-clamp-2 text-sm font-medium leading-snug text-slate-800 group-hover:text-blue-700 dark:text-slate-200 dark:group-hover:text-blue-400">
          {ad.title}
        </h3>
        <div className="mt-auto flex flex-wrap items-center gap-x-2 gap-y-0.5 pt-2 text-xs text-slate-500 dark:text-slate-400">
          {(ad.sellerAverageRating ?? 0) > 0 && (
            <>
              <StarRating rating={ad.sellerAverageRating!} size="sm" showValue />
              <span className="text-slate-300 dark:text-slate-600">|</span>
            </>
          )}
          {location && <span>{location}</span>}
          {location && <span className="text-slate-300 dark:text-slate-600">|</span>}
          <span>#{ad.id}</span>
          <span className="text-slate-300 dark:text-slate-600">|</span>
          <span>{formatDate(ad.createdTime, locale)}</span>
        </div>
        {showStatus && (
          <div className="pt-1">
            <StatusBadge status={ad.status} isActive={ad.isActive} />
          </div>
        )}
      </div>
    </Link>
  );
}
