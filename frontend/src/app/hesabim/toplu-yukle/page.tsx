"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { PageHeader } from "@/components/PageHeader";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api } from "@/lib/api";
import { formFieldClass } from "@/lib/formStyles";
import { linkBack, pageContainerSm, surfaceCardPad } from "@/lib/uiStyles";

export default function BulkUploadPage() {
  const { t } = useLocale();
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();
  const { showToast } = useToast();
  const [csv, setCsv] = useState("categoryId,title,description,price,city\n1,Örnek ilan,Açıklama,100000,İstanbul");
  const [result, setResult] = useState<string[]>([]);
  const [uploading, setUploading] = useState(false);

  if (!isLoading && !isAuthenticated) {
    router.replace("/giris?redirect=/hesabim/toplu-yukle");
    return null;
  }

  async function upload(e: React.FormEvent) {
    e.preventDefault();
    setUploading(true);
    const res = await api.importSellerCsv(csv);
    setUploading(false);
    if (res.success && res.data) {
      showToast(`${res.data.created} ${t("bulk.created")}`, "success");
      setResult(res.data.errors);
    } else showToast(res.message || t("bulk.failed"), "error");
  }

  return (
    <div className={pageContainerSm}>
      <PageHeader title={t("bulk.title")} subtitle={t("bulk.desc")} />
      <p className="mt-2 text-sm text-slate-500">{t("bulk.format")}</p>
      <form onSubmit={upload} className={`mt-6 ${surfaceCardPad}`}>
        <textarea value={csv} onChange={(e) => setCsv(e.target.value)} rows={12} className={formFieldClass} />
        <button type="submit" disabled={uploading} className="mt-4 rounded-xl bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white disabled:opacity-60">
          {uploading ? "…" : t("bulk.upload")}
        </button>
      </form>
      {result.length > 0 && (
        <ul className="mt-4 list-disc space-y-1 pl-5 text-sm text-rose-600">
          {result.map((e) => (
            <li key={e}>{e}</li>
          ))}
        </ul>
      )}
      <Link href="/hesabim" className={`mt-8 ${linkBack}`}>← {t("account.title")}</Link>
    </div>
  );
}
