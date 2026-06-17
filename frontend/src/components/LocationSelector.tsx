"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { formFieldClass } from "@/lib/formStyles";
import { useLocale } from "@/context/LocaleContext";

type Loc = { id: number; name: string };

export type LocationNames = {
  city: string;
  district: string;
  neighborhood: string;
};

type LocationSelectorProps = {
  value: LocationNames;
  onChange: (v: LocationNames) => void;
  required?: boolean;
};

export function LocationSelector({ value, onChange, required }: LocationSelectorProps) {
  const { t } = useLocale();
  const [provinces, setProvinces] = useState<Loc[]>([]);
  const [districts, setDistricts] = useState<Loc[]>([]);
  const [neighborhoods, setNeighborhoods] = useState<Loc[]>([]);
  const [provinceId, setProvinceId] = useState<number | "">("");
  const [districtId, setDistrictId] = useState<number | "">("");
  const [neighborhoodId, setNeighborhoodId] = useState<number | "">("");
  const [loadingDistricts, setLoadingDistricts] = useState(false);
  const [loadingNeighborhoods, setLoadingNeighborhoods] = useState(false);

  useEffect(() => {
    api.getProvinces().then((res) => {
      if (res.success && res.data) setProvinces(res.data);
    });
  }, []);

  // İsimden il eşlemesi (düzenleme / geri yükleme)
  useEffect(() => {
    if (!value.city || provinces.length === 0) return;
    const match = provinces.find((p) => p.name === value.city);
    if (match) setProvinceId((prev) => (prev === match.id ? prev : match.id));
  }, [value.city, provinces]);

  useEffect(() => {
    if (!provinceId) {
      setDistricts([]);
      setDistrictId("");
      setNeighborhoods([]);
      setNeighborhoodId("");
      return;
    }
    setLoadingDistricts(true);
    api.getDistricts(Number(provinceId)).then((res) => {
      if (res.success && res.data) setDistricts(res.data);
      setLoadingDistricts(false);
    });
  }, [provinceId]);

  useEffect(() => {
    if (!value.district || districts.length === 0) return;
    const match = districts.find((d) => d.name === value.district);
    if (match) setDistrictId((prev) => (prev === match.id ? prev : match.id));
  }, [value.district, districts]);

  useEffect(() => {
    if (!districtId) {
      setNeighborhoods([]);
      setNeighborhoodId("");
      return;
    }
    setLoadingNeighborhoods(true);
    api.getNeighborhoods(Number(districtId)).then((res) => {
      if (res.success && res.data) setNeighborhoods(res.data);
      setLoadingNeighborhoods(false);
    });
  }, [districtId]);

  useEffect(() => {
    if (!value.neighborhood || neighborhoods.length === 0) return;
    const match = neighborhoods.find((n) => n.name === value.neighborhood);
    if (match) setNeighborhoodId((prev) => (prev === match.id ? prev : match.id));
  }, [value.neighborhood, neighborhoods]);

  function onProvince(id: string) {
    const p = provinces.find((x) => String(x.id) === id);
    const nextId = id ? Number(id) : "";
    setProvinceId(nextId);
    setDistrictId("");
    setNeighborhoodId("");
    onChange({
      city: p?.name ?? "",
      district: "",
      neighborhood: "",
    });
  }

  function onDistrict(id: string) {
    const d = districts.find((x) => String(x.id) === id);
    const nextId = id ? Number(id) : "";
    setDistrictId(nextId);
    setNeighborhoodId("");
    onChange({
      ...value,
      district: d?.name ?? "",
      neighborhood: "",
    });
  }

  function onNeighborhood(id: string) {
    const n = neighborhoods.find((x) => String(x.id) === id);
    setNeighborhoodId(id ? Number(id) : "");
    onChange({
      ...value,
      neighborhood: n?.name ?? "",
    });
  }

  return (
    <div className="grid gap-4 sm:grid-cols-3">
      <div>
        <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
          {t("listing.city")} {required && "*"}
        </label>
        <select
          required={required}
          value={provinceId === "" ? "" : String(provinceId)}
          onChange={(e) => onProvince(e.target.value)}
          className={formFieldClass}
        >
          <option value="">{provinces.length === 0 ? "…" : t("listing.select")}</option>
          {provinces.map((p) => (
            <option key={p.id} value={p.id}>{p.name}</option>
          ))}
        </select>
      </div>
      <div>
        <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
          {t("listing.district")} {required && "*"}
        </label>
        <select
          required={required}
          disabled={!provinceId || loadingDistricts}
          value={districtId === "" ? "" : String(districtId)}
          onChange={(e) => onDistrict(e.target.value)}
          className={formFieldClass}
        >
          <option value="">{loadingDistricts ? "…" : t("listing.select")}</option>
          {districts.map((d) => (
            <option key={d.id} value={d.id}>{d.name}</option>
          ))}
        </select>
      </div>
      <div>
        <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">
          {t("listing.neighborhood")}
        </label>
        <select
          disabled={!districtId || loadingNeighborhoods}
          value={neighborhoodId === "" ? "" : String(neighborhoodId)}
          onChange={(e) => onNeighborhood(e.target.value)}
          className={formFieldClass}
        >
          <option value="">{loadingNeighborhoods ? "…" : t("listing.select")}</option>
          {neighborhoods.map((n) => (
            <option key={n.id} value={n.id}>{n.name}</option>
          ))}
        </select>
      </div>
    </div>
  );
}
