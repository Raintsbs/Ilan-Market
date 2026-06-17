"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { AnimatedGlowingSearchBar } from "@/components/ui/animated-glowing-search-bar";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { PageHeader } from "@/components/PageHeader";
import { alertWarning, categoryCard, pageContainer } from "@/lib/uiStyles";
import type { CategoryTreeNode } from "@/lib/types";

export default function CategoriesPage() {
  const { t } = useLocale();
  const [categories, setCategories] = useState<CategoryTreeNode[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");

  useEffect(() => {
    api
      .getCategoryTree()
      .then((res) => {
        if (res.success && res.data) {
          setCategories(res.data);
        } else {
          setError(res.message || t("categories.loadError"));
        }
      })
      .catch(() => setError(t("home.apiError")))
      .finally(() => setLoading(false));
  }, [t]);

  const filteredCategories = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return categories;
    return categories.filter(
      (cat) =>
        cat.name.toLowerCase().includes(q) ||
        (cat.description?.toLowerCase().includes(q) ?? false),
    );
  }, [categories, search]);

  return (
    <div className={pageContainer}>
      <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <PageHeader title={t("categories.title")} subtitle={t("categories.pick")} className="mb-0 min-w-0 flex-1" />

        {!loading && !error && categories.length > 0 && (
          <AnimatedGlowingSearchBar
            variant="subtle"
            value={search}
            onChange={setSearch}
            placeholder={t("categories.searchPlaceholder")}
            id="category-search"
            className="w-full sm:w-64 lg:w-72 shrink-0"
          />
        )}
      </div>

      {loading ? (
        <LoadingSpinner label={t("categories.loading")} />
      ) : error ? (
        <div className={alertWarning}>{error}</div>
      ) : categories.length === 0 ? (
        <p className="text-center text-slate-500 dark:text-slate-400">{t("categories.noCategories")}</p>
      ) : filteredCategories.length === 0 ? (
        <p className="text-center text-slate-500 dark:text-slate-400">{t("categories.noSearchResults")}</p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {filteredCategories.map((cat) => (
            <Link
              key={cat.id}
              href={`/?categoryId=${cat.id}`}
              className={categoryCard}
            >
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-blue-50 text-lg font-bold text-blue-600 transition group-hover:bg-blue-600 group-hover:text-white dark:bg-blue-950">
                {cat.name.charAt(0).toUpperCase()}
              </div>
              <h2 className="mt-4 text-lg font-semibold text-slate-900 group-hover:text-blue-700 dark:text-white dark:group-hover:text-blue-400">
                {cat.name}
              </h2>
              {cat.description && (
                <p className="mt-2 line-clamp-2 text-sm text-slate-500 dark:text-slate-400">{cat.description}</p>
              )}
              {(cat.children?.length ?? 0) > 0 && (
                <p className="mt-2 text-xs text-slate-400">
                  {cat.children!.length} alt kategori
                </p>
              )}
              <span className="mt-4 inline-flex items-center gap-1 text-sm font-medium text-blue-600 dark:text-blue-400">
                {t("categories.browse")}
                <svg className="h-4 w-4 transition group-hover:translate-x-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                </svg>
              </span>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
