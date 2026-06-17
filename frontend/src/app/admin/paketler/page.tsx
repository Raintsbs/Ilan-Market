"use client";

import { useEffect, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { adminApi, isAdmin } from "@/lib/admin";
import {
  adminBtnPrimary,
  adminBtnSecondary,
  adminInputFull,
  adminListItem,
  adminModalOverlay,
  adminModalPanel,
} from "@/lib/adminStyles";
import { useAdminAuth } from "@/context/AdminAuthContext";
import type { AdminAdPackage } from "@/lib/types";

const empty: AdminAdPackage = {
  id: 0,
  code: "",
  name: "",
  price: 0,
  featuredDays: 7,
  isActive: true,
};

export default function AdminPackagesPage() {
  const { user } = useAdminAuth();
  const canEdit = isAdmin(user?.roles);
  const [items, setItems] = useState<AdminAdPackage[]>([]);
  const [form, setForm] = useState<AdminAdPackage | null>(null);

  const load = () => adminApi.getAdPackages().then((r) => r.data && setItems(r.data));

  useEffect(() => {
    load();
  }, []);

  const save = async () => {
    if (!form || !canEdit) return;
    const res = await adminApi.saveAdPackage(form, form.id > 0 ? form.id : undefined);
    if (res.success) {
      setForm(null);
      load();
    } else alert(res.message || "Kayıt başarısız");
  };

  const deactivate = async (id: number) => {
    if (!canEdit || !confirm("Paket pasifleştirilsin mi?")) return;
    const res = await adminApi.deleteAdPackage(id);
    if (res.success) load();
    else alert(res.message || "İşlem başarısız");
  };

  return (
    <div>
      <AdminPageHeader
        title="Öne çıkarma paketleri"
        subtitle="Fiyat ve süre yönetimi"
        actions={
          canEdit ? (
            <button type="button" onClick={() => setForm({ ...empty })} className={adminBtnPrimary}>
              Yeni paket
            </button>
          ) : undefined
        }
      />

      <div className="space-y-2">
        {items.map((p) => (
          <div key={p.id} className={`${adminListItem} flex flex-wrap items-center justify-between gap-3`}>
            <div>
              <p className="font-medium text-white">
                {p.name}{" "}
                <span className="text-slate-500">({p.code})</span>
                {!p.isActive && <span className="ml-2 text-xs text-amber-400">pasif</span>}
              </p>
              <p className="text-sm text-slate-400">
                {p.price.toLocaleString("tr-TR")} TL · {p.featuredDays} gün
              </p>
            </div>
            {canEdit && (
              <div className="flex gap-2">
                <button type="button" onClick={() => setForm({ ...p })} className={adminBtnSecondary}>
                  Düzenle
                </button>
                {p.isActive && (
                  <button type="button" onClick={() => deactivate(p.id)} className={adminBtnSecondary}>
                    Pasifleştir
                  </button>
                )}
              </div>
            )}
          </div>
        ))}
      </div>

      {form && (
        <div className={adminModalOverlay}>
          <div className={adminModalPanel}>
            <h2 className="text-lg font-semibold text-white">
              {form.id > 0 ? "Paket düzenle" : "Yeni paket"}
            </h2>
            <div className="mt-4 space-y-3">
              <input
                className={adminInputFull}
                placeholder="Kod (ör. featured-7)"
                value={form.code}
                onChange={(e) => setForm({ ...form, code: e.target.value })}
              />
              <input
                className={adminInputFull}
                placeholder="Ad"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
              />
              <input
                type="number"
                className={adminInputFull}
                placeholder="Fiyat (TL)"
                value={form.price}
                onChange={(e) => setForm({ ...form, price: Number(e.target.value) })}
              />
              <input
                type="number"
                className={adminInputFull}
                placeholder="Öne çıkarma günü"
                value={form.featuredDays}
                onChange={(e) => setForm({ ...form, featuredDays: Number(e.target.value) })}
              />
              <label className="flex items-center gap-2 text-sm text-slate-300">
                <input
                  type="checkbox"
                  checked={form.isActive}
                  onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
                />
                Aktif
              </label>
            </div>
            <div className="mt-6 flex justify-end gap-2">
              <button type="button" onClick={() => setForm(null)} className={adminBtnSecondary}>
                Vazgeç
              </button>
              <button type="button" onClick={save} className={adminBtnPrimary}>
                Kaydet
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
