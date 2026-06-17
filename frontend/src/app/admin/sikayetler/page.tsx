"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { adminApi } from "@/lib/admin";
import {
  adminBtnDanger,
  adminBtnSecondary,
  adminBtnWarning,
  adminCardPad,
  adminSelect,
} from "@/lib/adminStyles";
import type { ReportItem } from "@/lib/types";

export default function AdminReportsPage() {
  const [items, setItems] = useState<ReportItem[]>([]);
  const [status, setStatus] = useState("open");

  const load = () => {
    adminApi.getReports(status).then((r) => {
      if (r.success && r.data) setItems(r.data);
    });
  };

  useEffect(() => {
    load();
  }, [status]);

  const resolve = async (id: number, action: string, rejectAd = false) => {
    const note = prompt("Not (opsiyonel):") ?? undefined;
    await adminApi.resolveReport(id, action, note, rejectAd);
    load();
  };

  return (
    <div>
      <AdminPageHeader
        title="Şikayetler"
        subtitle="Kullanıcı bildirimlerini inceleyin ve işlem yapın"
        actions={
          <select value={status} onChange={(e) => setStatus(e.target.value)} className={adminSelect}>
            <option value="open">Açık</option>
            <option value="resolved">Çözüldü</option>
            <option value="all">Tümü</option>
          </select>
        }
      />

      <div className="space-y-4">
        {items.map((r) => (
          <div key={r.id} className={adminCardPad}>
            <p className="font-medium text-white">
              <Link
                href={`/ilan/${r.advertisementId}`}
                className="text-blue-400 hover:text-blue-300"
                target="_blank"
              >
                {r.adTitle}
              </Link>
            </p>
            <p className="mt-1 text-sm text-slate-400">Sebep: {r.reason}</p>
            {r.details && <p className="mt-1 text-sm text-slate-500">{r.details}</p>}
            <p className="mt-2 text-xs text-slate-600">
              {new Date(r.createdTime).toLocaleString("tr")} · {r.status}
            </p>
            {r.status === "open" && (
              <div className="mt-4 flex flex-wrap gap-2">
                <button type="button" onClick={() => resolve(r.id, "dismiss")} className={adminBtnSecondary}>
                  Şikayeti reddet
                </button>
                <button type="button" onClick={() => resolve(r.id, "warn", false)} className={adminBtnWarning}>
                  Uyarı ver
                </button>
                <button type="button" onClick={() => resolve(r.id, "reject_ad", true)} className={adminBtnDanger}>
                  İlanı reddet
                </button>
              </div>
            )}
          </div>
        ))}
        {!items.length && (
          <p className="py-12 text-center text-sm text-slate-500">Bu filtrede kayıt yok.</p>
        )}
      </div>
    </div>
  );
}
