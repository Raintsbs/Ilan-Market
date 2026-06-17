"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { ContentHtml } from "@/components/ContentHtml";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import type { PublicStaticPage } from "@/lib/types";

export default function StaticPageView() {
  const params = useParams();
  const slug = String(params.slug ?? "");
  const { t } = useLocale();
  const [page, setPage] = useState<PublicStaticPage | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!slug) return;
    api.getStaticPage(slug).then((res) => {
      if (res.success && res.data) setPage(res.data);
      else setError(res.message || t("staticPage.notFound"));
      setLoading(false);
    });
  }, [slug, t]);

  if (loading) return <LoadingSpinner />;
  if (error || !page) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-16 text-center">
        <p className="text-slate-600 dark:text-slate-400">{error || t("staticPage.notFound")}</p>
        <Link href="/" className="mt-4 inline-block text-blue-600 hover:underline">
          {t("nav.backHome")}
        </Link>
      </div>
    );
  }

  return (
    <article className="mx-auto max-w-3xl px-4 py-10 sm:px-6">
      <h1 className="text-3xl font-bold text-slate-900 dark:text-white">{page.title}</h1>
      <div className="mt-8">
        <ContentHtml html={page.content} />
      </div>
    </article>
  );
}
