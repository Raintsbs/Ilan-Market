"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api, ApiError } from "@/lib/api";
import { subscribeRealtime } from "@/lib/realtimeEvents";
import { formFieldClass } from "@/lib/formStyles";
import { btnBrandSm, linkBack, pageContainerSm, surfaceCard } from "@/lib/uiStyles";
import type { ChatMessage } from "@/lib/types";

export default function ThreadPage() {
  const params = useParams();
  const router = useRouter();
  const threadId = Number(params.id);
  const { t } = useLocale();
  const { showToast } = useToast();
  const { isAuthenticated, isLoading } = useAuth();
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [body, setBody] = useState("");
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    const res = await api.getThreadMessages(threadId);
    if (res.success && res.data) setMessages(res.data);
    setLoading(false);
  }, [threadId]);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) router.replace(`/giris?redirect=/mesajlar/${threadId}`);
  }, [isLoading, isAuthenticated, router, threadId]);

  useEffect(() => {
    if (isAuthenticated) load();
  }, [isAuthenticated, load]);

  useEffect(() => {
    if (!isAuthenticated) return;
    const unsub = subscribeRealtime("messages", (payload) => {
      const tid =
        payload && typeof payload === "object" && "threadId" in payload
          ? Number((payload as { threadId: number }).threadId)
          : 0;
      if (!tid || tid === threadId) load();
    });
    const interval = setInterval(load, 5000);
    return () => {
      unsub();
      clearInterval(interval);
    };
  }, [isAuthenticated, load, threadId]);

  async function send() {
    if (!body.trim()) return;
    const text = body.trim();
    try {
      await api.sendMessage(0, text, threadId);
      setBody("");
      await load();
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("messages.sendFailed"), "error");
      await load();
    }
  }

  if (isLoading || loading) return <LoadingSpinner />;

  return (
    <div className={`${pageContainerSm} flex flex-col`} style={{ minHeight: "70vh" }}>
      <div className="flex items-center justify-between">
        <Link href="/mesajlar" className={linkBack}>
          ← {t("messages.back")}
        </Link>
        <span className="text-xs font-medium text-slate-400">{t("messages.live")}</span>
      </div>
      <div className={`mt-4 flex-1 space-y-3 overflow-y-auto p-4 ${surfaceCard}`}>
        {messages.map((m) => (
          <div
            key={m.id}
            className={`max-w-[85%] rounded-xl px-3 py-2 text-sm ${
              m.isMine
                ? "ml-auto bg-blue-600 text-white"
                : "bg-slate-100 text-slate-900 dark:bg-slate-800 dark:text-white"
            }`}
          >
            {m.body}
          </div>
        ))}
      </div>
      <div className="mt-4 flex gap-2">
        <input
          value={body}
          onChange={(e) => setBody(e.target.value)}
          className={`flex-1 ${formFieldClass}`}
          placeholder={t("messages.placeholder")}
        />
        <button type="button" onClick={send} className={btnBrandSm}>
          {t("messages.send")}
        </button>
      </div>
    </div>
  );
}
