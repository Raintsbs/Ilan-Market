"use client";

import { useCallback, useEffect, useState } from "react";
import { useLocale } from "@/context/LocaleContext";
import { SafeImage } from "./SafeImage";

interface ImageGalleryProps {
  urls: string[];
  alt: string;
  favoriteSlot?: React.ReactNode;
}

export function ImageGallery({ urls, alt, favoriteSlot }: ImageGalleryProps) {
  const { t } = useLocale();
  const [index, setIndex] = useState(0);
  const [lightbox, setLightbox] = useState(false);
  const [zoom, setZoom] = useState(false);

  const go = useCallback(
    (dir: -1 | 1) => {
      setIndex((i) => (i + dir + urls.length) % urls.length);
      setZoom(false);
    },
    [urls.length],
  );

  useEffect(() => {
    if (!lightbox) return;
    function onKey(e: KeyboardEvent) {
      if (e.key === "Escape") setLightbox(false);
      if (e.key === "ArrowLeft") go(-1);
      if (e.key === "ArrowRight") go(1);
    }
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [lightbox, go]);

  if (urls.length === 0) {
    return (
      <div className="flex min-h-[min(55vh,420px)] items-center justify-center rounded-xl bg-slate-100 text-slate-400">
        {t("ad.noImage")}
      </div>
    );
  }

  const current = urls[index];

  return (
    <>
      <div className="space-y-3">
        <div className="group relative min-h-[min(70vh,560px)] w-full overflow-hidden rounded-xl bg-slate-900">
          <SafeImage
            src={current}
            alt={`${alt} - ${index + 1}`}
            fill
            className="cursor-zoom-in object-cover object-center"
            onClick={() => setLightbox(true)}
          />
          {favoriteSlot && (
            <div className="absolute right-3 top-3 z-10">{favoriteSlot}</div>
          )}
          {urls.length > 1 && (
            <>
              <button
                type="button"
                onClick={() => go(-1)}
                className="absolute left-2 top-1/2 z-10 -translate-y-1/2 rounded-full bg-black/50 p-2 text-white backdrop-blur hover:bg-black/70"
                aria-label={t("ad.prev")}
              >
                ‹
              </button>
              <button
                type="button"
                onClick={() => go(1)}
                className="absolute right-2 top-1/2 z-10 -translate-y-1/2 rounded-full bg-black/50 p-2 text-white backdrop-blur hover:bg-black/70"
                aria-label={t("ad.next")}
              >
                ›
              </button>
              <span className="absolute bottom-3 right-3 z-10 rounded-full bg-black/50 px-2.5 py-1 text-xs font-medium text-white">
                {index + 1} / {urls.length}
              </span>
            </>
          )}
          <button
            type="button"
            onClick={() => setLightbox(true)}
            className="absolute bottom-3 left-3 z-10 rounded-lg bg-white/95 px-2.5 py-1 text-xs font-medium text-slate-800 shadow"
          >
            {t("ad.enlarge")}
          </button>
        </div>

        {urls.length > 1 && (
          <div className="flex gap-2 overflow-x-auto pb-1">
            {urls.map((url, i) => (
              <button
                key={`${url}-${i}`}
                type="button"
                onClick={() => setIndex(i)}
                className={`relative h-16 w-24 shrink-0 overflow-hidden rounded-lg border-2 transition ${
                  i === index ? "border-blue-600" : "border-slate-200 opacity-80 hover:opacity-100"
                }`}
              >
                <SafeImage src={url} alt="" fill className="object-cover object-center" />
              </button>
            ))}
          </div>
        )}
      </div>

      {lightbox && (
        <div
          className="fixed inset-0 z-[100] flex items-center justify-center bg-black/90 p-4"
          role="dialog"
          aria-modal
          onClick={() => setLightbox(false)}
        >
          <button
            type="button"
            className="absolute right-4 top-4 z-10 rounded-full bg-white/10 px-3 py-1 text-sm text-white hover:bg-white/20"
            onClick={() => setLightbox(false)}
          >
            {t("ad.close")} ✕
          </button>
          {urls.length > 1 && (
            <>
              <button
                type="button"
                className="absolute left-4 top-1/2 z-10 -translate-y-1/2 rounded-full bg-white/10 p-3 text-2xl text-white"
                onClick={(e) => {
                  e.stopPropagation();
                  go(-1);
                }}
              >
                ‹
              </button>
              <button
                type="button"
                className="absolute right-4 top-1/2 z-10 -translate-y-1/2 rounded-full bg-white/10 p-3 text-2xl text-white"
                onClick={(e) => {
                  e.stopPropagation();
                  go(1);
                }}
              >
                ›
              </button>
            </>
          )}
          <button
            type="button"
            className="absolute bottom-4 left-1/2 z-10 -translate-x-1/2 rounded-full bg-white/10 px-4 py-2 text-sm text-white"
            onClick={(e) => {
              e.stopPropagation();
              setZoom((z) => !z);
            }}
          >
            {zoom ? t("ad.zoomOut") : t("ad.zoomIn")}
          </button>
          <div
            className={`relative max-h-[85vh] max-w-[95vw] transition-transform duration-200 ${
              zoom ? "scale-150" : "scale-100"
            }`}
            onClick={(e) => e.stopPropagation()}
          >
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src={current}
              alt={alt}
              className="max-h-[85vh] w-auto max-w-full object-contain"
              onClick={(e) => {
                e.stopPropagation();
                setZoom((z) => !z);
              }}
            />
          </div>
        </div>
      )}
    </>
  );
}
