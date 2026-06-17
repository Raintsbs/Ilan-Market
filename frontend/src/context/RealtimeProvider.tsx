"use client";

import { useEffect } from "react";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { dispatchRealtime, type RealtimeChannel } from "@/lib/realtimeEvents";
import { startRealtimeConnection, stopRealtimeConnection } from "@/lib/realtime";

function isRealtimeChannel(value: string): value is RealtimeChannel {
  return value === "notification" || value === "messages" || value === "offers";
}

export function RealtimeProvider({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
  const { t } = useLocale();
  const { showToast } = useToast();

  useEffect(() => {
    if (isLoading || !isAuthenticated) {
      void stopRealtimeConnection();
      return;
    }

    let cancelled = false;

    const retryMs = 15_000;
    let retryTimer: ReturnType<typeof setTimeout> | undefined;

    const connect = async () => {
      if (cancelled) return;
      const ok = await startRealtimeConnection((channel, payload) => {
        if (cancelled || !isRealtimeChannel(channel)) return;
        dispatchRealtime(channel, payload);

        if (channel === "notification" && payload && typeof payload === "object") {
          const p = payload as { title?: string; body?: string };
          if (p.title) showToast(p.title, "success");
        }
        if (channel === "offers") {
          showToast(t("realtime.newOffer"), "success");
        }
        if (channel === "messages") {
          showToast(t("realtime.newMessage"), "success");
        }
      });
      if (!ok && !cancelled) {
        /* API kapalı veya hub yok — bildirim polling devam eder */
        retryTimer = setTimeout(connect, retryMs);
      }
    };

    void connect();

    return () => {
      cancelled = true;
      if (retryTimer) clearTimeout(retryTimer);
      void stopRealtimeConnection();
    };
  }, [isAuthenticated, isLoading, showToast, t]);

  return <>{children}</>;
}
