"use client";

import { useEffect, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { adminApi } from "@/lib/admin";
import { adminBtnPrimary, adminCardPad, adminInput } from "@/lib/adminStyles";
import type { Coupon } from "@/lib/types";

export default function AdminCouponsPage() {
  const [items, setItems] = useState<Coupon[]>([]);
  const [form, setForm] = useState<Partial<Coupon>>({ code: "", discountAmount: 0, maxUses: 100, isActive: true });

  const load = () => adminApi.getCoupons().then((r) => { if (r.success && r.data) setItems(r.data); });
  useEffect(() => { load(); }, []);

  async function save(e: React.FormEvent) {
    e.preventDefault();
    await adminApi.saveCoupon(form as Coupon);
    setForm({ code: "", discountAmount: 0, maxUses: 100, isActive: true });
    load();
  }

  return (
    <div>
      <AdminPageHeader title="Kuponlar" subtitle="İndirim kodları yönetimi" />
      <form onSubmit={save} className={`mt-6 grid gap-3 sm:grid-cols-2 ${adminCardPad}`}>
        <input placeholder="KOD" value={form.code ?? ""} onChange={(e) => setForm({ ...form, code: e.target.value })} className={adminInput} />
        <input type="number" placeholder="TL indirim" value={form.discountAmount ?? 0} onChange={(e) => setForm({ ...form, discountAmount: Number(e.target.value) })} className={adminInput} />
        <input type="number" placeholder="%" value={form.discountPercent ?? ""} onChange={(e) => setForm({ ...form, discountPercent: Number(e.target.value) || undefined })} className={adminInput} />
        <input type="number" placeholder="Max kullanım" value={form.maxUses ?? 100} onChange={(e) => setForm({ ...form, maxUses: Number(e.target.value) })} className={adminInput} />
        <button type="submit" className={adminBtnPrimary}>Kaydet</button>
      </form>
      <div className="mt-6 space-y-2">
        {items.map((c) => (
          <div key={c.id} className={`flex justify-between ${adminCardPad}`}>
            <span className="font-mono text-white">{c.code}</span>
            <span className="text-slate-400">{c.usedCount}/{c.maxUses} · {c.discountAmount} TL</span>
          </div>
        ))}
      </div>
    </div>
  );
}
