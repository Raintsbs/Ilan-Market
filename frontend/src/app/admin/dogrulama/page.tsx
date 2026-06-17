"use client";

import { useEffect, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { adminApi } from "@/lib/admin";
import { getImageUrl } from "@/lib/image";
import { adminBtnPrimary, adminBtnDanger, adminCardPad } from "@/lib/adminStyles";
import { formatDate } from "@/lib/status";
import type { VerificationRequest } from "@/lib/types";

export default function AdminVerificationPage() {
  const [items, setItems] = useState<VerificationRequest[]>([]);

  const load = () => {
    adminApi.getPendingVerifications().then((r) => {
      if (r.success && r.data) setItems(r.data);
    });
  };

  useEffect(() => {
    load();
  }, []);

  const review = async (id: number, approve: boolean) => {
    const note = prompt("Not (opsiyonel):") ?? undefined;
    await adminApi.reviewVerification(id, approve, note);
    load();
  };

  return (
    <div>
      <AdminPageHeader title="Kimlik Doğrulama" subtitle="Bekleyen satıcı doğrulama başvuruları" />
      <div className="mt-6 space-y-4">
        {items.map((v) => (
          <div key={v.id} className={adminCardPad}>
            <p className="font-medium text-white">{v.userEmail}</p>
            <p className="text-sm text-slate-400">{v.documentType} · {formatDate(v.createdAt, "tr")}</p>
            {v.filePath && (
            <a href={getImageUrl(v.filePath) ?? "#"} target="_blank" rel="noreferrer" className="mt-2 inline-block text-sm text-blue-400">
              Belgeyi görüntüle
            </a>
            )}
            <div className="mt-4 flex gap-2">
              <button type="button" onClick={() => review(v.id, true)} className={adminBtnPrimary}>Onayla</button>
              <button type="button" onClick={() => review(v.id, false)} className={adminBtnDanger}>Reddet</button>
            </div>
          </div>
        ))}
        {items.length === 0 && <p className="text-slate-500">Bekleyen başvuru yok.</p>}
      </div>
    </div>
  );
}
