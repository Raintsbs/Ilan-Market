"use client";

import { useCallback, useEffect, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { MapWithPins } from "@/components/MapWithPins";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { useAdsChangeListener } from "@/lib/adsSync";
import { PageHeader } from "@/components/PageHeader";
import { pageContainer } from "@/lib/uiStyles";
import type { MapListing } from "@/lib/types";

export default function MapPage() {
  const { t } = useLocale();
  const [items, setItems] = useState<MapListing[]>([]);
  const [loading, setLoading] = useState(true);

  const load = useCallback(() => {
    setLoading(true);
    api.getMapListings({}).then((res) => {
      if (res.success && res.data) setItems(res.data);
      setLoading(false);
    });
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  useAdsChangeListener(load);

  if (loading) return <LoadingSpinner />;

  return (
    <div className={pageContainer}>
      <PageHeader title={t("map.title")} subtitle={t("map.subtitle")} />
      <div className="mt-6">
        <MapWithPins items={items} />
      </div>
    </div>
  );
}
