"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { PageHeader } from "@/components/PageHeader";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { linkBack, pageContainerSm, surfaceCardPad } from "@/lib/uiStyles";
import type { MarketplaceOrder, SellerEarnings } from "@/lib/types";

export default function EarningsPage() {
  const { t, locale } = useLocale();
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();
  const [data, setData] = useState<SellerEarnings | null>(null);
  const [orders, setOrders] = useState<MarketplaceOrder[]>([]);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) router.replace("/giris?redirect=/hesabim/kazanc");
  }, [isLoading, isAuthenticated, router]);

  useEffect(() => {
    if (!isAuthenticated) return;
    api.getSellerEarnings().then((r) => {
      if (r.success && r.data) setData(r.data);
    });
    api.getMyMarketplaceOrders(true).then((r) => {
      if (r.success && r.data) {
        setOrders(r.data.filter((o) => o.status === 5));
      }
    });
  }, [isAuthenticated]);

  if (isLoading || !data) return <LoadingSpinner />;

  const fmt = (n: number) => n.toLocaleString(locale, { style: "currency", currency: "TRY", maximumFractionDigits: 0 });
  const pendingOrders = orders.filter((o) => !o.sellerPaidOutAt);
  const paidOrders = orders.filter((o) => o.sellerPaidOutAt);

  return (
    <div className={pageContainerSm}>
      <PageHeader title={t("earnings.title")} subtitle={t("earnings.desc")} />
      <dl className={`mt-6 grid gap-4 sm:grid-cols-2 ${surfaceCardPad}`}>
        <div>
          <dt className="text-sm text-slate-500">{t("earnings.total")}</dt>
          <dd className="text-2xl font-bold">{fmt(data.totalCompletedAmount)}</dd>
        </div>
        <div>
          <dt className="text-sm text-slate-500">{t("earnings.pending")}</dt>
          <dd className="text-2xl font-bold text-amber-600">{fmt(data.pendingPayoutAmount)}</dd>
        </div>
        <div>
          <dt className="text-sm text-slate-500">{t("earnings.paid")}</dt>
          <dd className="text-2xl font-bold text-emerald-600">{fmt(data.paidOutAmount)}</dd>
        </div>
        <div>
          <dt className="text-sm text-slate-500">{t("earnings.orders")}</dt>
          <dd className="text-2xl font-bold">{data.completedOrderCount}</dd>
        </div>
      </dl>
      {orders.length > 0 && (
        <div className={`mt-6 ${surfaceCardPad}`}>
          <h2 className="text-sm font-semibold text-slate-900 dark:text-white">{t("earnings.payoutPending")}</h2>
          <ul className="mt-4 space-y-3 text-sm">
            {pendingOrders.map((o) => (
              <li key={o.id} className="flex flex-wrap items-center justify-between gap-2 border-b border-slate-100 pb-3 dark:border-slate-800">
                <Link href={`/siparisler/${o.id}`} className="font-medium text-blue-600 hover:underline">
                  #{o.id} · {o.advertisementTitle}
                </Link>
                <span className="text-amber-600">{fmt(o.amount)} · {t("earnings.payoutWaiting")}</span>
              </li>
            ))}
            {paidOrders.map((o) => (
              <li key={o.id} className="flex flex-wrap items-center justify-between gap-2 border-b border-slate-100 pb-3 dark:border-slate-800">
                <Link href={`/siparisler/${o.id}`} className="font-medium text-blue-600 hover:underline">
                  #{o.id} · {o.advertisementTitle}
                </Link>
                <span className="text-emerald-600">{fmt(o.amount)} · {t("earnings.payoutDone")}</span>
              </li>
            ))}
          </ul>
        </div>
      )}
      <Link href="/siparisler" className="mt-4 inline-block text-sm text-blue-600 hover:underline">
        {t("nav.orders")} →
      </Link>
      <Link href="/hesabim" className={`mt-8 block ${linkBack}`}>← {t("account.title")}</Link>
    </div>
  );
}
