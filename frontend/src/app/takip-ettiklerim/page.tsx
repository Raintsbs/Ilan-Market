"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { PageHeader } from "@/components/PageHeader";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { formatDate } from "@/lib/status";
import { linkBack, pageContainerMd, surfaceCard } from "@/lib/uiStyles";
import type { SellerFollow } from "@/lib/types";

export default function FollowingPage() {
  const { t, locale } = useLocale();
  const { isAuthenticated, isLoading } = useAuth();
  const [items, setItems] = useState<SellerFollow[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isAuthenticated) return;
    api.getFollowedSellers().then((r) => {
      if (r.success && r.data) setItems(r.data);
      setLoading(false);
    });
  }, [isAuthenticated]);

  if (isLoading || loading) return <LoadingSpinner />;

  return (
    <div className={pageContainerMd}>
      <PageHeader title={t("nav.following")} />
      <div className="mt-6 space-y-3">
        {items.map((s) => (
          <div key={s.sellerUserId} className={`flex flex-wrap items-center justify-between gap-3 p-4 ${surfaceCard}`}>
            <div>
              <Link
                href={s.storeSlug ? `/magaza/${s.storeSlug}` : `/satici/${s.sellerUserId}`}
                className="font-semibold text-blue-600 hover:underline dark:text-blue-400"
              >
                {s.displayName}
              </Link>
              <p className="text-xs text-slate-500">
                {s.activeListingCount} {t("seller.listings").toLowerCase()} · {formatDate(s.followedAt, locale)}
              </p>
            </div>
          </div>
        ))}
        {items.length === 0 && (
          <p className="text-center text-slate-500">{t("follow.empty")}</p>
        )}
      </div>
      <Link href="/hesabim" className={`mt-8 ${linkBack}`}>
        ← {t("account.title")}
      </Link>
    </div>
  );
}
