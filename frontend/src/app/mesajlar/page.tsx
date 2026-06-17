"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useCallback, useEffect, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api, ApiError } from "@/lib/api";
import { useToast } from "@/context/ToastContext";
import { subscribeRealtime } from "@/lib/realtimeEvents";
import { formFieldClass } from "@/lib/formStyles";
import { PageHeader } from "@/components/PageHeader";
import { alertInfo, btnBrandSm, listItemLink, pageContainerSm } from "@/lib/uiStyles";
import type { MessageThread } from "@/lib/types";

function MessagesContent() {
  const router = useRouter();
  const params = useSearchParams();
  const adId = params.get("ad");
  const { t } = useLocale();
  const { showToast } = useToast();
  const { isAuthenticated, isLoading } = useAuth();
  const [threads, setThreads] = useState<MessageThread[]>([]);
  const [loading, setLoading] = useState(true);
  const [firstMsg, setFirstMsg] = useState("");

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.getMessageThreads();
      if (res.success && res.data) setThreads(res.data);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) router.replace("/giris?redirect=/mesajlar");
  }, [isLoading, isAuthenticated, router]);

  useEffect(() => {
    if (!isAuthenticated) return;
    load();
    const unsub = subscribeRealtime("messages", () => load());
    const id = window.setInterval(load, 8000);
    return () => {
      unsub();
      clearInterval(id);
    };
  }, [isAuthenticated, load]);

  async function startFromAd() {
    if (!adId || !firstMsg.trim()) return;
    const adNum = Number(adId);
    const existing = threads.find((th) => th.advertisementId === adNum);
    try {
      if (existing) {
        await api.sendMessage(existing.advertisementId, firstMsg.trim(), existing.id);
        setFirstMsg("");
        router.push(`/mesajlar/${existing.id}`);
        return;
      }
      const res = await api.sendMessage(adNum, firstMsg.trim());
      if (res.success && res.data) {
        setFirstMsg("");
        router.push(`/mesajlar/${res.data.threadId}`);
      }
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("messages.sendFailed"), "error");
    }
  }

  if (isLoading || loading) return <LoadingSpinner />;
  if (!isAuthenticated) return null;

  return (
    <div className={pageContainerSm}>
      <PageHeader title={t("messages.title")} />
      {adId && (
        <div className={`mt-4 ${alertInfo}`}>
          <p className="text-sm font-medium text-blue-900 dark:text-blue-200">{t("messages.newToSeller")}</p>
          <textarea
            value={firstMsg}
            onChange={(e) => setFirstMsg(e.target.value)}
            rows={3}
            className={`mt-2 ${formFieldClass}`}
          />
          <button type="button" onClick={startFromAd} className={`mt-2 ${btnBrandSm}`}>
            {t("messages.send")}
          </button>
        </div>
      )}
      <ul className="mt-6 space-y-2">
        {threads.map((th) => (
          <li key={th.id}>
            <Link
              href={`/mesajlar/${th.id}`}
              className={listItemLink}
            >
              <p className="font-semibold text-slate-900 dark:text-white">{th.advertisementTitle}</p>
              <p className="text-sm text-slate-500">{th.otherUserName}</p>
              {th.lastMessage && <p className="mt-1 truncate text-sm text-slate-600">{th.lastMessage}</p>}
              {th.unreadCount > 0 && (
                <span className="mt-2 inline-block rounded-full bg-blue-600 px-2 py-0.5 text-xs text-white">
                  {th.unreadCount}
                </span>
              )}
            </Link>
          </li>
        ))}
      </ul>
      {threads.length === 0 && !adId && (
        <p className="mt-8 text-center text-slate-500">{t("messages.empty")}</p>
      )}
    </div>
  );
}

export default function MessagesPage() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <MessagesContent />
    </Suspense>
  );
}
