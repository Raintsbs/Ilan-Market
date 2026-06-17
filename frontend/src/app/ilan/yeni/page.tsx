"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { AdForm } from "@/components/AdForm";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useAuth } from "@/context/AuthContext";
import { PageHeader } from "@/components/PageHeader";
import { useLocale } from "@/context/LocaleContext";
import { linkBack, pageContainer, surfaceCard } from "@/lib/uiStyles";

export default function NewAdPage() {
  const router = useRouter();
  const { t } = useLocale();
  const { isAuthenticated, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace("/giris");
    }
  }, [isAuthenticated, isLoading, router]);

  if (isLoading) return <LoadingSpinner />;
  if (!isAuthenticated) return null;

  return (
    <div className={`${pageContainer} max-w-4xl`}>
      <Link href="/ilanlarim" className={linkBack}>
        ← {t("newAd.back")}
      </Link>
      <PageHeader title={t("newAd.title")} subtitle={t("newAd.subtitle")} className="mt-4" />
      <div className={`mt-8 p-6 sm:p-8 ${surfaceCard}`}>
        <AdForm
          onSuccess={(id, message) => {
            if (id > 0) {
              router.push(`/ilan/${id}`);
            } else {
              router.push(`/ilanlarim?created=1${message ? `&msg=${encodeURIComponent(message)}` : ""}`);
            }
          }}
        />
      </div>
    </div>
  );
}
