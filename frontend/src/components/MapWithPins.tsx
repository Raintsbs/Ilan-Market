"use client";

import Link from "next/link";
import { useEffect, useRef } from "react";
import { useLocale } from "@/context/LocaleContext";
import type { MapListing } from "@/lib/types";

type MapWithPinsProps = {
  items: MapListing[];
};

export function MapWithPins({ items }: MapWithPinsProps) {
  const { t } = useLocale();
  const mapRef = useRef<HTMLDivElement>(null);
  const mapInstance = useRef<{ remove: () => void } | null>(null);

  useEffect(() => {
    if (!mapRef.current || items.length === 0) return;

    let cancelled = false;

    async function init() {
      const L = (await import("leaflet")).default;
      await import("leaflet/dist/leaflet.css");

      if (cancelled || !mapRef.current) return;

      if (mapInstance.current) {
        mapInstance.current.remove();
        mapInstance.current = null;
      }

      const avgLat = items.reduce((s, i) => s + i.lat, 0) / items.length;
      const avgLng = items.reduce((s, i) => s + i.lng, 0) / items.length;

      const map = L.map(mapRef.current).setView([avgLat, avgLng], 7);
      L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "&copy; OpenStreetMap",
      }).addTo(map);

      const icon = L.divIcon({
        className: "",
        html: '<div style="width:14px;height:14px;border-radius:50%;background:#4f46e5;border:2px solid white;box-shadow:0 1px 4px rgba(0,0,0,.3)"></div>',
        iconSize: [14, 14],
        iconAnchor: [7, 7],
      });

      for (const item of items) {
        L.marker([item.lat, item.lng], { icon })
          .addTo(map)
          .bindPopup(
            `<strong>${item.title}</strong><br/>${item.city}${item.price != null ? `<br/>${item.price.toLocaleString("tr-TR")} TL` : ""}<br/><a href="/ilan/${item.id}">${t("map.open")}</a>`,
          );
      }

      mapInstance.current = map;
    }

    void init();

    return () => {
      cancelled = true;
      if (mapInstance.current) {
        mapInstance.current.remove();
        mapInstance.current = null;
      }
    };
  }, [items, t]);

  if (items.length === 0) {
    return (
      <p className="flex min-h-[400px] items-center justify-center rounded-2xl border border-slate-200 text-slate-500 dark:border-slate-700">
        {t("map.noCoords")}
      </p>
    );
  }

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <div
        ref={mapRef}
        className="min-h-[400px] overflow-hidden rounded-2xl border border-slate-200 dark:border-slate-700"
      />
      <ul className="max-h-[500px] space-y-2 overflow-y-auto">
        {items.map((item) => (
          <li key={item.id}>
            <Link
              href={`/ilan/${item.id}`}
              className="block rounded-xl border border-slate-200 p-3 hover:border-blue-200 dark:border-slate-700"
            >
              <p className="font-semibold text-slate-900 dark:text-white">{item.title}</p>
              <p className="text-sm text-slate-500">
                {item.city}
                {item.district ? ` / ${item.district}` : ""}
                {item.price != null ? ` · ${item.price.toLocaleString("tr-TR")} TL` : ""}
              </p>
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
