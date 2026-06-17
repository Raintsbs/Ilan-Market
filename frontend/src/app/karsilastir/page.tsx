"use client";

import Link from "next/link";
import Image from "next/image";
import { useSearchParams } from "next/navigation";
import { Suspense, useEffect, useMemo, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { PageHeader } from "@/components/PageHeader";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { resolveCategoryProfile } from "@/lib/categoryProfile";
import { getCompareIds } from "@/lib/compareList";
import { formatPrice, getListingDetailRows, parseListingDetails } from "@/lib/listingDetails";
import { getImageUrl } from "@/lib/image";
import { linkBack, pageContainerMd, surfaceCard } from "@/lib/uiStyles";
import type { Advertisement } from "@/lib/types";

function CompareContent() {
  const params = useSearchParams();
  const { t } = useLocale();
  const [ads, setAds] = useState<Advertisement[]>([]);
  const [loading, setLoading] = useState(true);

  const ids = useMemo(() => {
    const fromUrl = (params.get("ids") ?? "")
      .split(",")
      .map((s) => Number(s.trim()))
      .filter((n) => n > 0);
    return fromUrl.length > 0 ? fromUrl.slice(0, 4) : getCompareIds();
  }, [params]);

  useEffect(() => {
    if (ids.length === 0) {
      setAds([]);
      setLoading(false);
      return;
    }
    api
      .getAdvertisementsBatch(ids)
      .then((res) => {
        if (res.success && res.data) setAds(res.data);
      })
      .finally(() => setLoading(false));
  }, [ids.join(",")]);

  const rows = useMemo(() => {
    if (ads.length === 0) return [];
    const profiles = ads.map((a) => resolveCategoryProfile(a.categoryName));
    const kind = profiles.find((p) => p?.kind === "vehicle" || p?.kind === "estate")?.kind ?? "other";

    const base: { key: string; label: string; get: (ad: Advertisement) => string }[] = [
      {
        key: "price",
        label: t("listing.price"),
        get: (ad) => {
          const p = parseListingDetails(ad.listingDetails).price;
          return p != null ? formatPrice(p) : "—";
        },
      },
      {
        key: "city",
        label: t("listing.city"),
        get: (ad) => parseListingDetails(ad.listingDetails).city || "—",
      },
      {
        key: "year",
        label: t("listing.year"),
        get: (ad) => parseListingDetails(ad.listingDetails).year || "—",
      },
      {
        key: "mileage",
        label: t("row.mileage"),
        get: (ad) => parseListingDetails(ad.listingDetails).mileage || "—",
      },
      {
        key: "rooms",
        label: t("row.rooms"),
        get: (ad) => parseListingDetails(ad.listingDetails).roomCount || "—",
      },
    ];

    if (kind === "vehicle") {
      base.push(
        { key: "brand", label: t("listing.brand"), get: (ad) => parseListingDetails(ad.listingDetails).brand || "—" },
        { key: "model", label: t("listing.model"), get: (ad) => parseListingDetails(ad.listingDetails).model || "—" },
        { key: "year", label: t("listing.year"), get: (ad) => parseListingDetails(ad.listingDetails).year || "—" },
        { key: "mileage", label: t("row.mileage"), get: (ad) => parseListingDetails(ad.listingDetails).mileage || "—" },
        { key: "fuel", label: t("row.fuel"), get: (ad) => parseListingDetails(ad.listingDetails).fuelType || "—" },
        { key: "transmission", label: t("row.transmission"), get: (ad) => parseListingDetails(ad.listingDetails).transmission || "—" },
      );
    } else if (kind === "estate") {
      base.push(
        { key: "rooms", label: t("row.rooms"), get: (ad) => parseListingDetails(ad.listingDetails).roomCount || "—" },
        { key: "sqm", label: t("row.sqm"), get: (ad) => parseListingDetails(ad.listingDetails).squareMeters || "—" },
        { key: "floor", label: t("row.floor"), get: (ad) => parseListingDetails(ad.listingDetails).floor || "—" },
        { key: "heating", label: t("row.heating"), get: (ad) => parseListingDetails(ad.listingDetails).heating || "—" },
      );
    } else {
      const first = ads[0];
      if (first) {
        const detailRows = getListingDetailRows(
          parseListingDetails(first.listingDetails),
          first.categoryName ?? "",
          t,
          { yes: t("common.yes"), no: t("common.no") },
        );
        detailRows.slice(0, 6).forEach((r) => {
          base.push({
            key: r.label,
            label: r.label,
            get: (ad) =>
              getListingDetailRows(parseListingDetails(ad.listingDetails), ad.categoryName ?? "", t, {
                yes: t("common.yes"),
                no: t("common.no"),
              }).find((x) => x.label === r.label)?.value ?? "—",
          });
        });
      }
    }

    return base;
  }, [ads, t]);

  if (loading) return <LoadingSpinner />;

  return (
    <div className={pageContainerMd}>
      <PageHeader title={t("compare.pageTitle")} subtitle={t("compare.pageSubtitle")} />

      {ads.length === 0 ? (
        <p className="mt-8 text-center text-slate-500">{t("compare.empty")}</p>
      ) : (
        <div className={`mt-8 overflow-x-auto ${surfaceCard}`}>
          <table className="min-w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 dark:border-slate-700">
                <th className="sticky left-0 bg-white p-4 text-left dark:bg-slate-900">{t("compare.feature")}</th>
                {ads.map((ad) => {
                  const img = ad.imagePath ? getImageUrl(ad.imagePath) : null;
                  return (
                    <th key={ad.id} className="min-w-[180px] p-4 text-left align-top">
                      {img && (
                        <div className="relative mb-2 h-24 w-full overflow-hidden rounded-lg bg-slate-100">
                          <Image src={img} alt="" fill className="object-cover" unoptimized />
                        </div>
                      )}
                      <Link href={`/ilan/${ad.id}`} className="font-semibold text-blue-600 hover:underline dark:text-blue-400">
                        {ad.title}
                      </Link>
                    </th>
                  );
                })}
              </tr>
            </thead>
            <tbody>
              {rows.map((row) => (
                <tr key={row.key} className="border-b border-slate-100 dark:border-slate-800">
                  <td className="sticky left-0 bg-white p-4 font-medium text-slate-600 dark:bg-slate-900 dark:text-slate-400">
                    {row.label}
                  </td>
                  {ads.map((ad) => (
                    <td key={ad.id} className="p-4 text-slate-900 dark:text-slate-100">
                      {row.get(ad)}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Link href="/" className={`mt-8 ${linkBack}`}>
        ← {t("common.backHome")}
      </Link>
    </div>
  );
}

export default function ComparePage() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <CompareContent />
    </Suspense>
  );
}
