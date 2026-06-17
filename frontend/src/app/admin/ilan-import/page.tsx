"use client";

import { useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { adminApi } from "@/lib/admin";
import { adminBtnPrimary, adminCardPad } from "@/lib/adminStyles";

export default function AdminImportPage() {
  const [csv, setCsv] = useState("userId,categoryId,title,description,price,city\n1,1,Örnek,Açıklama,50000,Ankara");
  const [log, setLog] = useState<string[]>([]);

  async function run(e: React.FormEvent) {
    e.preventDefault();
    const res = await adminApi.importAds(csv);
    if (res.success && res.data) {
      setLog([`${res.data.created} oluşturuldu, ${res.data.failed} hata`, ...res.data.errors]);
    }
  }

  return (
    <div>
      <AdminPageHeader title="Toplu İlan İçe Aktar" subtitle="Admin CSV import" />
      <form onSubmit={run} className={`mt-6 ${adminCardPad}`}>
        <textarea value={csv} onChange={(e) => setCsv(e.target.value)} rows={10} className="w-full rounded-lg border border-slate-700 bg-slate-900 p-3 font-mono text-sm text-white" />
        <button type="submit" className={`mt-4 ${adminBtnPrimary}`}>İçe aktar</button>
      </form>
      {log.length > 0 && (
        <ul className="mt-4 list-disc pl-5 text-sm text-slate-300">{log.map((l) => <li key={l}>{l}</li>)}</ul>
      )}
    </div>
  );
}
