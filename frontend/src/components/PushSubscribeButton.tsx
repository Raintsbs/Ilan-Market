"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { btnOutline } from "@/lib/uiStyles";

export function PushSubscribeButton() {
  const { t } = useLocale();
  const { isAuthenticated } = useAuth();
  const [supported, setSupported] = useState(false);
  const [subscribed, setSubscribed] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setSupported(typeof window !== "undefined" && "serviceWorker" in navigator && "PushManager" in window);
  }, []);

  if (!isAuthenticated || !supported || process.env.NODE_ENV === "development") return null;

  async function subscribe() {
    setLoading(true);
    try {
      const config = await api.getAuthPublicConfig();
      const vapidKey = config.data?.webPushVapidPublicKey;
      if (!vapidKey) return;

      const reg = await navigator.serviceWorker.ready;
      let sub = await reg.pushManager.getSubscription();
      if (!sub) {
        const key = urlBase64ToUint8Array(vapidKey);
        sub = await reg.pushManager.subscribe({
          userVisibleOnly: true,
          applicationServerKey: key.buffer as ArrayBuffer,
        });
      }
      const json = sub.toJSON();
      if (json.endpoint && json.keys?.p256dh && json.keys?.auth) {
        await api.registerPushSubscription(json.endpoint, json.keys.p256dh, json.keys.auth);
        setSubscribed(true);
      }
    } catch {
      /* optional */
    } finally {
      setLoading(false);
    }
  }

  if (subscribed) {
    return (
      <p className="text-xs text-emerald-600">{t("push.subscribed")}</p>
    );
  }

  return (
    <button type="button" disabled={loading} onClick={subscribe} className={`${btnOutline} text-sm`}>
      {loading ? "..." : t("push.enable")}
    </button>
  );
}

function urlBase64ToUint8Array(base64String: string) {
  const padding = "=".repeat((4 - (base64String.length % 4)) % 4);
  const base64 = (base64String + padding).replace(/-/g, "+").replace(/_/g, "/");
  const raw = atob(base64);
  const arr = new Uint8Array(raw.length);
  for (let i = 0; i < raw.length; ++i) arr[i] = raw.charCodeAt(i);
  return arr;
}
