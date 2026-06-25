"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { PageHeader } from "@/components/PageHeader";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api } from "@/lib/api";
import { formFieldClass } from "@/lib/formStyles";
import { linkBack, pageContainerSm, surfaceCardPad } from "@/lib/uiStyles";

export default function StoreSettingsPage() {
  const { t } = useLocale();
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();
  const { showToast } = useToast();
  const [storeSlug, setStoreSlug] = useState("");
  const [companyName, setCompanyName] = useState("");
  const [storeDescription, setStoreDescription] = useState("");
  const [isCorporate, setIsCorporate] = useState(false);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) router.replace("/giris?redirect=/hesabim/magaza");
    else if (isAuthenticated) {
      api.getStoreSettings().then((res) => {
        if (res.success && res.data) {
          setStoreSlug(res.data.storeSlug ?? "");
          setCompanyName(res.data.companyName ?? "");
          setStoreDescription(res.data.storeDescription ?? "");
          setIsCorporate(res.data.isCorporateStore ?? false);
        }
      });
    }
  }, [isLoading, isAuthenticated, router]);

  async function save(e: React.FormEvent) {
    e.preventDefault();
    setSaving(true);
    const res = await api.updateStoreSettings({
      storeSlug: storeSlug || undefined,
      companyName,
      storeDescription,
      isCorporateStore: isCorporate,
    });
    setSaving(false);
    if (res.success) showToast(t("store.saved"), "success");
    else showToast(res.message || t("account.error"), "error");
  }

  if (isLoading) return <LoadingSpinner />;

  return (
    <div className={pageContainerSm}>
      <PageHeader title={t("store.settings")} subtitle={t("store.settingsDesc")} />
      <form onSubmit={save} className={`mt-6 space-y-4 ${surfaceCardPad}`}>
        <div>
          <label className="text-sm font-medium">{t("store.slug")}</label>
          <input value={storeSlug} onChange={(e) => setStoreSlug(e.target.value)} className={`mt-1 ${formFieldClass}`} placeholder="magaza-adi" />
        </div>
        <div>
          <label className="text-sm font-medium">{t("store.company")}</label>
          <input value={companyName} onChange={(e) => setCompanyName(e.target.value)} className={`mt-1 ${formFieldClass}`} />
        </div>
        <div>
          <label className="text-sm font-medium">{t("store.description")}</label>
          <textarea value={storeDescription} onChange={(e) => setStoreDescription(e.target.value)} rows={4} className={`mt-1 ${formFieldClass}`} />
        </div>
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={isCorporate} onChange={(e) => setIsCorporate(e.target.checked)} />
          {t("store.corporate")}
        </label>
        <button type="submit" disabled={saving} className="rounded-xl bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white disabled:opacity-60">
          {saving ? "…" : t("account.save")}
        </button>
      </form>
      {storeSlug && (
        <p className="mt-4 text-sm text-slate-500">
          {t("store.preview")}: <Link href={`/magaza/${storeSlug}`} className="text-blue-600">{`/magaza/${storeSlug}`}</Link>
        </p>
      )}
      <Link href="/hesabim" className={`mt-8 ${linkBack}`}>← {t("account.title")}</Link>
    </div>
  );
}
