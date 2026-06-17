"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useEffect } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api } from "@/lib/api";

function SuccessContent() {
  const params = useSearchParams();
  const sessionId = params.get("session_id");
  const router = useRouter();
  const { t } = useLocale();
  const { showToast } = useToast();

  useEffect(() => {
    if (!sessionId) return;
    api.completeStripeSession(sessionId).then((res) => {
      if (res.success) showToast(t("featured.success"), "success");
      else showToast(res.message || t("featured.failed"), "error");
    });
  }, [sessionId, showToast, t]);

  return (
    <div className="mx-auto max-w-lg px-4 py-16 text-center">
      <h1 className="text-2xl font-bold text-slate-900 dark:text-white">{t("featured.paymentDone")}</h1>
      <p className="mt-2 text-slate-500">{t("featured.paymentDoneHint")}</p>
      <div className="mt-8 flex flex-col gap-3 sm:flex-row sm:justify-center">
        <Link
          href="/ilanlarim"
          className="rounded-xl bg-blue-600 px-6 py-3 text-sm font-semibold text-white"
        >
          {t("myAds.title")}
        </Link>
        <button
          type="button"
          onClick={() => router.push("/")}
          className="rounded-xl border border-slate-200 px-6 py-3 text-sm font-semibold dark:border-slate-600"
        >
          {t("common.backHome")}
        </button>
      </div>
    </div>
  );
}

export default function FeaturedSuccessPage() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <SuccessContent />
    </Suspense>
  );
}
