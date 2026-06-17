"use client";

import Link from "next/link";
import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { subscribeRealtime } from "@/lib/realtimeEvents";
import type { AppNotification } from "@/lib/types";

export function NotificationBell() {
  const { isAuthenticated } = useAuth();
  const { t } = useLocale();
  const [open, setOpen] = useState(false);
  const [unread, setUnread] = useState(0);
  const [items, setItems] = useState<AppNotification[]>([]);
  const [badgeBounce, setBadgeBounce] = useState(false);
  const prevUnread = useRef(0);
  const panelRef = useRef<HTMLDivElement>(null);

  const load = useCallback(async () => {
    if (!isAuthenticated) return;
    try {
      const [countRes, listRes] = await Promise.all([
        api.getUnreadNotificationCount(),
        api.getNotifications(),
      ]);
      if (countRes.success && countRes.data !== undefined) {
        if (countRes.data > prevUnread.current) setBadgeBounce(true);
        prevUnread.current = countRes.data;
        setUnread(countRes.data);
      }
      if (listRes.success && listRes.data) {
        setItems(listRes.data.slice(0, 6));
      }
    } catch {
      /* API kapalıyken header bloklanmasın */
    }
  }, [isAuthenticated]);

  useEffect(() => {
    load();
    const pollMs = document.hidden ? 30000 : 8000;
    const id = window.setInterval(load, pollMs);
    const unsub = subscribeRealtime("notification", () => load());
    function onFocus() {
      load();
    }
    function onVisibility() {
      if (!document.hidden) load();
    }
    window.addEventListener("focus", onFocus);
    document.addEventListener("visibilitychange", onVisibility);
    return () => {
      clearInterval(id);
      unsub();
      window.removeEventListener("focus", onFocus);
      document.removeEventListener("visibilitychange", onVisibility);
    };
  }, [load]);

  useEffect(() => {
    if (!open) return;
    function onDocClick(e: MouseEvent) {
      if (panelRef.current && !panelRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", onDocClick);
    return () => document.removeEventListener("mousedown", onDocClick);
  }, [open]);

  if (!isAuthenticated) return null;

  function typeAccent(type: string) {
    if (type === "ad_approved") return "border-l-blue-500";
    if (type === "ad_rejected") return "border-l-rose-500";
    return "border-l-blue-500";
  }

  return (
    <div className="relative" ref={panelRef}>
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        aria-label={t("notifications.title")}
        aria-expanded={open}
        className="relative rounded-xl p-2 text-slate-600 transition hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800"
      >
        <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
          />
        </svg>
        {unread > 0 && (
          <span
            className={`absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-rose-500 px-1 text-[10px] font-bold text-white ${
              badgeBounce ? "animate-badge-bounce" : ""
            }`}
            onAnimationEnd={() => setBadgeBounce(false)}
          >
            {unread > 9 ? "9+" : unread}
          </span>
        )}
      </button>

      {open && (
        <div className="animate-dropdown-in absolute right-0 top-full z-50 mt-2 w-80 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-xl dark:border-slate-700 dark:bg-slate-900">
          <div className="flex items-center justify-between border-b border-slate-100 px-4 py-3 dark:border-slate-800">
            <span className="text-sm font-semibold text-slate-900 dark:text-white">{t("notifications.title")}</span>
            <Link
              href="/bildirimler"
              onClick={() => setOpen(false)}
              className="text-xs font-medium text-blue-600 hover:underline dark:text-blue-400"
            >
              {t("notifications.viewAll")}
            </Link>
          </div>
          <ul className="max-h-72 overflow-y-auto py-1">
            {items.length === 0 ? (
              <li className="px-4 py-6 text-center text-sm text-slate-500">{t("notifications.empty")}</li>
            ) : (
              items.map((n) => (
                <li key={n.id}>
                  <Link
                    href={n.link || "/bildirimler"}
                    onClick={() => {
                      void api.markNotificationRead(n.id);
                      setOpen(false);
                    }}
                    className={`block border-l-4 px-4 py-3 text-left transition hover:bg-slate-50 dark:hover:bg-slate-800 ${typeAccent(n.type)} ${
                      !n.isRead ? "bg-blue-50/50 dark:bg-blue-950/40" : ""
                    }`}
                  >
                    <p className="text-sm font-medium text-slate-900 dark:text-white">{n.title}</p>
                    <p className="mt-0.5 line-clamp-2 text-xs text-slate-500 dark:text-slate-400">{n.body}</p>
                  </Link>
                </li>
              ))
            )}
          </ul>
        </div>
      )}
    </div>
  );
}
