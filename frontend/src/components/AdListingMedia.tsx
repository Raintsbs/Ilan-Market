"use client";

import Link from "next/link";
import { PanoramaViewer } from "@/components/PanoramaViewer";
import { useLocale } from "@/context/LocaleContext";
import type { ListingDetails } from "@/lib/listingDetails";
import { resolveCategoryProfile } from "@/lib/categoryProfile";
import { getVideoUrl } from "@/lib/image";
import { surfaceElevated } from "@/lib/uiStyles";

function youtubeEmbedUrl(url: string): string | null {
  try {
    const u = new URL(url);
    if (u.hostname.includes("youtu.be")) {
      const id = u.pathname.slice(1);
      return id ? `https://www.youtube.com/embed/${id}` : null;
    }
    if (u.hostname.includes("youtube.com")) {
      const id = u.searchParams.get("v");
      return id ? `https://www.youtube.com/embed/${id}` : null;
    }
  } catch {
    return null;
  }
  return null;
}

type AdListingMediaProps = {
  listingDetails: ListingDetails;
  categoryName: string;
  videoPath?: string | null;
  panoramaPath?: string | null;
};

export function AdListingMedia({
  listingDetails,
  categoryName,
  videoPath,
  panoramaPath,
}: AdListingMediaProps) {
  const { t } = useLocale();
  const profile = resolveCategoryProfile(categoryName);
  const videoEmbed = listingDetails.videoUrl ? youtubeEmbedUrl(listingDetails.videoUrl) : null;
  const localVideoUrl = getVideoUrl(videoPath);
  const hasTour = !!listingDetails.virtualTourUrl?.trim();
  const expertUrl = listingDetails.expertReportUrl?.trim();
  const floorPlanUrl = listingDetails.floorPlanUrl?.trim();

  if (!videoEmbed && !localVideoUrl && !panoramaPath && !hasTour && !expertUrl && !floorPlanUrl) {
    return null;
  }

  return (
    <section className="space-y-4">
      {localVideoUrl && (
        <div className={`overflow-hidden ${surfaceElevated}`}>
          <div className="border-b border-slate-100 px-4 py-3 dark:border-slate-800">
            <h2 className="text-sm font-bold uppercase tracking-wide text-slate-500">{t("media.localVideo")}</h2>
          </div>
          <video src={localVideoUrl} controls className="aspect-video w-full bg-black" />
        </div>
      )}

      {panoramaPath && <PanoramaViewer path={panoramaPath} title={t("media.panorama")} />}

      {videoEmbed && (
        <div className={`overflow-hidden ${surfaceElevated}`}>
          <div className="border-b border-slate-100 px-4 py-3 dark:border-slate-800">
            <h2 className="text-sm font-bold uppercase tracking-wide text-slate-500">{t("media.video")}</h2>
          </div>
          <div className="aspect-video w-full bg-black">
            <iframe
              src={videoEmbed}
              title={t("media.video")}
              className="h-full w-full"
              allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
              allowFullScreen
            />
          </div>
        </div>
      )}

      <div className="flex flex-wrap gap-3">
        {hasTour && (
          <Link
            href={listingDetails.virtualTourUrl!}
            target="_blank"
            rel="noopener noreferrer"
            className={`inline-flex items-center gap-2 rounded-xl border border-blue-200 bg-blue-50 px-4 py-3 text-sm font-semibold text-blue-800 transition hover:bg-blue-100 dark:border-blue-800 dark:bg-blue-950 dark:text-blue-200 dark:hover:bg-blue-900`}
          >
            <span aria-hidden>360°</span>
            {t("media.openVirtualTour")}
          </Link>
        )}
        {expertUrl && profile?.showVehicle && (
          <Link
            href={expertUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center rounded-xl border border-slate-200 px-4 py-3 text-sm font-semibold text-slate-800 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-200 dark:hover:bg-slate-800"
          >
            {t("media.expertReport")}
          </Link>
        )}
        {floorPlanUrl && profile?.showEstate && (
          <Link
            href={floorPlanUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center rounded-xl border border-slate-200 px-4 py-3 text-sm font-semibold text-slate-800 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-200 dark:hover:bg-slate-800"
          >
            {t("media.floorPlan")}
          </Link>
        )}
      </div>
    </section>
  );
}
