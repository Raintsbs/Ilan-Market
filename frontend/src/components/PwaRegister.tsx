"use client";

import { useEffect } from "react";
import { useAuth } from "@/context/AuthContext";
import { api } from "@/lib/api";

export function PwaRegister() {
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    if (!("serviceWorker" in navigator)) return;
    if (process.env.NODE_ENV === "development") {
      void navigator.serviceWorker.getRegistrations().then((regs) => {
        regs.forEach((r) => void r.unregister());
      });
      return;
    }
    navigator.serviceWorker.register("/sw.js").catch(() => {});
  }, []);

  useEffect(() => {
    if (!isAuthenticated || !("serviceWorker" in navigator)) return;
    const delay = window.setTimeout(() => {
      void (async () => {
        try {
          const reg = await Promise.race([
            navigator.serviceWorker.ready,
            new Promise<never>((_, reject) =>
              setTimeout(() => reject(new Error("sw timeout")), 3000),
            ),
          ]);
          const sub = await reg.pushManager?.getSubscription();
          if (sub) {
            const json = sub.toJSON();
            if (json.endpoint && json.keys?.p256dh && json.keys?.auth) {
              await api.registerPushSubscription(
                json.endpoint,
                json.keys.p256dh,
                json.keys.auth,
              );
            }
          }
        } catch {
          /* push optional */
        }
      })();
    }, 3000);
    return () => clearTimeout(delay);
  }, [isAuthenticated]);

  return null;
}
