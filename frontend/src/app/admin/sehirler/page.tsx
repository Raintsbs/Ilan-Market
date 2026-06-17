"use client";

import { useEffect, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { adminApi } from "@/lib/admin";
import {
  adminBtnPrimary,
  adminInput,
  adminListItem,
} from "@/lib/adminStyles";
import type { CityLocation } from "@/lib/types";

export default function AdminCitiesPage() {
  const [items, setItems] = useState<CityLocation[]>([]);
  const [city, setCity] = useState("");
  const [district, setDistrict] = useState("");

  const load = () => adminApi.getCities().then((r) => r.data && setItems(r.data));

  useEffect(() => {
    load();
  }, []);

  const add = async () => {
    if (!city.trim()) return;
    await adminApi.saveCity({ id: 0, city, district: district || undefined, isActive: true, sortOrder: 0 });
    setCity("");
    setDistrict("");
    load();
  };

  return (
    <div>
      <AdminPageHeader
        title="Şehir / İlçe"
        subtitle="Eski konum listesi — yeni ilanlar için il/ilçe API kullanılır"
      />

      <div className="mb-6 flex flex-wrap gap-2">
        <input value={city} onChange={(e) => setCity(e.target.value)} placeholder="Şehir" className={adminInput} />
        <input
          value={district}
          onChange={(e) => setDistrict(e.target.value)}
          placeholder="İlçe (opsiyonel)"
          className={adminInput}
        />
        <button type="button" onClick={add} className={adminBtnPrimary}>
          Ekle
        </button>
      </div>

      <ul className="space-y-2">
        {items.map((c) => (
          <li key={c.id} className={adminListItem}>
            <span className="text-sm text-white">
              {c.city}
              {c.district ? ` / ${c.district}` : ""}
            </span>
            <button
              type="button"
              className="text-sm font-medium text-rose-400 hover:text-rose-300"
              onClick={async () => {
                if (confirm("Silinsin mi?")) {
                  await adminApi.deleteCity(c.id);
                  load();
                }
              }}
            >
              Sil
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
}
