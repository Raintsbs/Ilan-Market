"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { PageHeader } from "@/components/PageHeader";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { formatPrice } from "@/lib/listingDetails";
import { formatDate } from "@/lib/status";
import { linkBack, pageContainerMd, surfaceCard } from "@/lib/uiStyles";
import type { MarketplaceOrder } from "@/lib/types";

export default function OrdersPage() {
  const router = useRouter();
  const { t, locale } = useLocale();
  const { isAuthenticated, isLoading } = useAuth();
  const [tab, setTab] = useState<"buyer" | "seller">("buyer");
  const [orders, setOrders] = useState<MarketplaceOrder[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) router.replace("/giris?redirect=/siparisler");
  }, [isLoading, isAuthenticated, router]);

  useEffect(() => {
    if (!isAuthenticated) return;
    setLoading(true);
    api.getMyMarketplaceOrders(tab === "seller").then((res) => {
      if (res.success && res.data) setOrders(res.data);
      setLoading(false);
    });
  }, [isAuthenticated, tab]);

  if (isLoading || loading) return <LoadingSpinner />;

  return (
    <div className={pageContainerMd}>
      <PageHeader title={t("orders.title")} />
      <div className="mt-4 flex flex-col gap-2 sm:flex-row">
        <button
          type="button"
          onClick={() => setTab("buyer")}
          className={`flex-1 rounded-lg px-4 py-2.5 text-sm font-semibold sm:flex-none ${tab === "buyer" ? "bg-blue-600 text-white" : "border border-slate-200 text-slate-700 dark:border-slate-600 dark:text-slate-300"}`}
        >
          {t("orders.asBuyer")}
        </button>
        <button
          type="button"
          onClick={() => setTab("seller")}
          className={`flex-1 rounded-lg px-4 py-2.5 text-sm font-semibold sm:flex-none ${tab === "seller" ? "bg-blue-600 text-white" : "border border-slate-200 text-slate-700 dark:border-slate-600 dark:text-slate-300"}`}
        >
          {t("orders.asSeller")}
        </button>
      </div>

      {orders.length === 0 ? (
        <p className="mt-10 text-center text-slate-500">{t("orders.empty")}</p>
      ) : (
        <ul className="mt-8 space-y-4">
          {orders.map((o) => (
            <li key={o.id}>
              <Link
                href={`/siparisler/${o.id}`}
                className={`block p-5 ${surfaceCard} transition hover:border-blue-200 dark:hover:border-blue-800`}
              >
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="font-semibold text-slate-900 dark:text-white">
                      {o.advertisementTitle || `#${o.advertisementId}`}
                    </p>
                    <p className="mt-1 text-sm text-slate-500">
                      {formatDate(o.createdTime, locale)} · {o.statusLabel}
                    </p>
                  </div>
                  <p className="text-lg font-bold text-emerald-700 dark:text-emerald-400">
                    {formatPrice(o.amount)}
                  </p>
                </div>
              </Link>
            </li>
          ))}
        </ul>
      )}

      <Link href="/" className={`mt-8 ${linkBack}`}>
        ← {t("common.backHome")}
      </Link>
    </div>
  );
}
