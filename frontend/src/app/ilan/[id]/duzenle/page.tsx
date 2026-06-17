"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { AdForm } from "@/components/AdForm";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { PageHeader } from "@/components/PageHeader";
import { linkBack, pageContainer, surfaceCard } from "@/lib/uiStyles";
import type { Advertisement } from "@/lib/types";

export default function EditAdPage() {
  const params = useParams();
  const router = useRouter();
  const { t } = useLocale();
  const id = Number(params.id);
  const { user, isAuthenticated, isLoading: authLoading } = useAuth();
  const [ad, setAd] = useState<Advertisement | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    try {
      const res = await api.getAdvertisement(id);
      if (res.success && res.data) setAd(res.data);
      else setError(res.message || t("ad.notFound"));
    } catch {
      setError(t("ad.loadError"));
    } finally {
      setLoading(false);
    }
  }, [id, t]);

  useEffect(() => {
    if (!authLoading && !isAuthenticated) router.replace("/giris");
  }, [authLoading, isAuthenticated, router]);

  useEffect(() => {
    if (isAuthenticated) load();
  }, [isAuthenticated, load]);

  useEffect(() => {
    if (ad && user && ad.userId !== user.userId) {
      setError(t("editAd.noPermission"));
    }
  }, [ad, user, t]);

  if (authLoading || loading) return <LoadingSpinner />;
  if (error || !ad) {
    return (
      <div className="mx-auto max-w-md px-4 py-16 text-center text-slate-600 dark:text-slate-400">
        {error || t("ad.notFound")}
      </div>
    );
  }

  if (user && ad.userId !== user.userId) return null;

  return (
    <div className={`${pageContainer} max-w-4xl`}>
      <Link href={`/ilan/${ad.id}`} className={linkBack}>
        ← {t("editAd.back")}
      </Link>
      <PageHeader title={t("editAd.title")} className="mt-4" />
      <div className={`mt-8 p-6 sm:p-8 ${surfaceCard}`}>
        <AdForm
          initial={ad}
          onSuccess={(updatedId) => router.push(`/ilan/${updatedId}`)}
        />
      </div>
    </div>
  );
}
