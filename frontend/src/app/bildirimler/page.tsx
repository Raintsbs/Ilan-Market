"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { PushSubscribeButton } from "@/components/PushSubscribeButton";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { PageHeader } from "@/components/PageHeader";
import { pageContainerSm } from "@/lib/uiStyles";
import type { AppNotification } from "@/lib/types";

export default function NotificationsPage() {
  const router = useRouter();
  const { t } = useLocale();
  const { isAuthenticated, isLoading } = useAuth();
  const [items, setItems] = useState<AppNotification[]>([]);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    const res = await api.getNotifications();
    if (res.success && res.data) setItems(res.data);
    setLoading(false);
  }, []);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) router.replace("/giris?redirect=/bildirimler");
  }, [isLoading, isAuthenticated, router]);

  useEffect(() => {
    if (isAuthenticated) load();
  }, [isAuthenticated, load]);

  async function markRead(id: number, link?: string) {
    await api.markNotificationRead(id);
    if (link) router.push(link);
    load();
  }

  if (isLoading || loading) return <LoadingSpinner />;

  return (
    <div className={pageContainerSm}>
      <PageHeader title={t("notifications.title")} />
      <div className="mt-4">
        <PushSubscribeButton />
      </div>
      <ul className="mt-6 space-y-2">
        {items.map((n) => (
          <li key={n.id}>
            <button
              type="button"
              onClick={() => markRead(n.id, n.link)}
              className={`w-full rounded-xl border p-4 text-left border-l-4 ${
                n.type === "ad_approved"
                  ? "border-l-blue-500"
                  : n.type === "ad_rejected"
                    ? "border-l-rose-500"
                    : "border-l-blue-500"
              } ${
                n.isRead
                  ? "border-slate-200 bg-white opacity-70 dark:border-slate-700 dark:bg-slate-900"
                  : "border-blue-200 bg-blue-50/50 dark:border-blue-800 dark:bg-blue-950/60"
              }`}
            >
              <p className="font-semibold text-slate-900 dark:text-white">{n.title}</p>
              <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">{n.body}</p>
            </button>
          </li>
        ))}
      </ul>
      {items.length === 0 && <p className="mt-8 text-center text-slate-500">{t("notifications.empty")}</p>}
    </div>
  );
}
