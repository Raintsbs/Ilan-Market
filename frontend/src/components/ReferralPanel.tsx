"use client";

import { useEffect, useState } from "react";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api } from "@/lib/api";
import { surfaceCard } from "@/lib/uiStyles";
import type { ReferralStats } from "@/lib/types";

export function ReferralPanel() {
  const { t } = useLocale();
  const { showToast } = useToast();
  const [stats, setStats] = useState<ReferralStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .getReferralStats()
      .then((r) => {
        if (r.success && r.data) setStats(r.data);
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <section className={`mt-8 p-6 ${surfaceCard}`}>
        <p className="text-sm text-slate-500">…</p>
      </section>
    );
  }

  if (!stats) return null;

  function copy() {
    void navigator.clipboard.writeText(stats!.shareUrl);
    showToast(t("referral.copied"), "success");
  }

  return (
    <section className={`mt-8 p-6 ${surfaceCard}`}>
      <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t("referral.title")}</h2>
      <p className="mt-1 text-sm text-slate-500">{t("referral.subtitle")}</p>
      <dl className="mt-4 space-y-2 text-sm">
        <div className="flex justify-between">
          <dt className="text-slate-500">{t("referral.code")}</dt>
          <dd className="font-mono font-bold">{stats.referralCode}</dd>
        </div>
        <div className="flex justify-between">
          <dt className="text-slate-500">{t("referral.invited")}</dt>
          <dd className="font-semibold">{stats.referredUserCount}</dd>
        </div>
      </dl>
      <button
        type="button"
        onClick={copy}
        className="mt-4 rounded-xl bg-blue-600 px-4 py-2 text-sm font-semibold text-white"
      >
        {t("referral.copy")}
      </button>
    </section>
  );
}
