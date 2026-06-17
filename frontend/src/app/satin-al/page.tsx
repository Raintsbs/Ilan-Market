"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useEffect, useState } from "react";
import { ArrowLeft, ShieldCheck } from "lucide-react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { CreditCardForm } from "@/components/ui/credit-card-form";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api, ApiError } from "@/lib/api";
import { formatPrice, parseListingDetails } from "@/lib/listingDetails";
import { pageContainerSm } from "@/lib/uiStyles";
import type { Advertisement } from "@/lib/types";

type PaymentMethod = "param_guvende" | "card";
type Step = "method" | "payment";

function BuyContent() {
  const params = useSearchParams();
  const adId = Number(params.get("adId") || 0);
  const router = useRouter();
  const { t } = useLocale();
  const { showToast } = useToast();
  const { isAuthenticated, isLoading } = useAuth();
  const [ad, setAd] = useState<Advertisement | null>(null);
  const [loading, setLoading] = useState(true);
  const [step, setStep] = useState<Step>("method");
  const [method, setMethod] = useState<PaymentMethod>("param_guvende");
  const [orderId, setOrderId] = useState<number | null>(null);
  const [paying, setPaying] = useState(false);
  const [creating, setCreating] = useState(false);

  const details = ad ? parseListingDetails(ad.listingDetails) : null;
  const price = details?.price;

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace(`/giris?redirect=${encodeURIComponent(`/satin-al?adId=${adId}`)}`);
      return;
    }
    if (!isLoading && isAuthenticated && !adId) router.replace("/");
  }, [isLoading, isAuthenticated, adId, router]);

  useEffect(() => {
    if (!adId) return;
    api.getAdvertisement(adId).then((res) => {
      if (res.success && res.data) setAd(res.data);
      setLoading(false);
    });
  }, [adId]);

  async function startCheckout() {
    if (!adId) return;
    setCreating(true);
    try {
      const res = await api.createMarketplaceOrder(adId, method);
      if (!res.success || !res.data) {
        showToast(res.message || t("buy.failed"), "error");
        return;
      }
      setOrderId(res.data.id);
      setStep("payment");
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("buy.failed"), "error");
    } finally {
      setCreating(false);
    }
  }

  async function completePayment(cardHolder?: string, cardNumber?: string) {
    if (!orderId) return;
    setPaying(true);
    try {
      const last4 = cardNumber?.replace(/\s/g, "").slice(-4);
      const res = await api.payMarketplaceOrder(orderId, {
        cardHolder,
        cardNumberLast4: last4,
      });
      if (res.success && (res.data?.checkoutUrl || res.data?.stripeCheckoutUrl)) {
        window.location.href = res.data.checkoutUrl ?? res.data.stripeCheckoutUrl!;
        return;
      }
      if (res.success && res.data?.order) {
        const msg = res.data.isDemo
          ? (res.data.message || t("buy.demoSuccess"))
          : (res.data.message || t("buy.success"));
        showToast(msg, res.data.isDemo ? "info" : "success");
        router.push(`/siparisler/${orderId}`);
      } else {
        showToast(res.message || t("buy.failed"), "error");
      }
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("buy.failed"), "error");
    } finally {
      setPaying(false);
    }
  }

  if (loading || isLoading) return <LoadingSpinner />;

  if (!ad || !price || price <= 0) {
    return (
      <div className={`${pageContainerSm} py-16 text-center`}>
        <p className="text-slate-600 dark:text-slate-400">{t("buy.noPrice")}</p>
        <Link href={adId ? `/ilan/${adId}` : "/"} className="mt-4 inline-block text-blue-600 hover:underline">
          ← {t("common.backHome")}
        </Link>
      </div>
    );
  }

  return (
    <div className={pageContainerSm}>
      <div className="py-8 md:py-12">
        {step === "payment" && (
          <button
            type="button"
            onClick={() => {
              setStep("method");
              setOrderId(null);
            }}
            className="mb-6 inline-flex items-center gap-2 text-sm font-medium text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-white"
          >
            <ArrowLeft className="size-4" />
            {t("featured.backToPlans")}
          </button>
        )}

        <div className="mx-auto max-w-3xl text-center">
          <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white">{t("buy.title")}</h1>
          <p className="mt-2 text-[var(--muted-foreground)]">{t("buy.subtitle")}</p>
        </div>

        <div className="mx-auto mt-8 max-w-xl rounded-2xl border border-slate-200 bg-[var(--card)] p-5 dark:border-slate-700">
          <p className="text-sm text-[var(--muted-foreground)]">{t("buy.summary")}</p>
          <p className="mt-1 font-semibold text-slate-900 dark:text-white">{ad.title}</p>
          <div className="mt-4 flex items-center justify-between gap-4">
            <span className="text-sm text-[var(--muted-foreground)]">{t("buy.amount")}</span>
            <span className="text-2xl font-bold">{formatPrice(price)}</span>
          </div>
          <div className="mt-2 flex items-center justify-end gap-1.5 text-xs font-medium text-emerald-600 dark:text-emerald-400">
            <ShieldCheck className="size-4" />
            {t("buy.secure")}
          </div>
        </div>

        {step === "method" ? (
          <div className="mx-auto mt-8 max-w-xl space-y-4">
            <p className="text-sm font-medium text-slate-700 dark:text-slate-300">{t("buy.method")}</p>
            <label className="flex cursor-pointer gap-3 rounded-xl border border-slate-200 p-4 has-[:checked]:border-emerald-500 has-[:checked]:bg-emerald-50/50 dark:border-slate-700 dark:has-[:checked]:bg-emerald-950/30">
              <input
                type="radio"
                name="payMethod"
                checked={method === "param_guvende"}
                onChange={() => setMethod("param_guvende")}
                className="mt-1"
              />
              <div>
                <p className="font-semibold text-slate-900 dark:text-white">{t("buy.paramGuvende")}</p>
                <p className="mt-1 text-sm text-slate-500">{t("buy.paramGuvendeHint")}</p>
              </div>
            </label>
            <label className="flex cursor-pointer gap-3 rounded-xl border border-slate-200 p-4 has-[:checked]:border-blue-500 has-[:checked]:bg-blue-50/50 dark:border-slate-700 dark:has-[:checked]:bg-blue-950/30">
              <input
                type="radio"
                name="payMethod"
                checked={method === "card"}
                onChange={() => setMethod("card")}
                className="mt-1"
              />
              <div>
                <p className="font-semibold text-slate-900 dark:text-white">{t("buy.card")}</p>
              </div>
            </label>
            <button
              type="button"
              disabled={creating}
              onClick={startCheckout}
              className="w-full rounded-xl bg-emerald-600 py-3 text-sm font-bold text-white hover:bg-emerald-700 disabled:opacity-60"
            >
              {creating ? "…" : t("buy.continue")}
            </button>
          </div>
        ) : method === "param_guvende" ? (
          <div className="mx-auto mt-8 max-w-xl">
            <p className="mb-4 text-center text-sm text-slate-600 dark:text-slate-400">{t("buy.paramGuvendeHint")}</p>
            <p className="mb-4 text-center text-xs text-amber-700 dark:text-amber-400">{t("buy.demoHint")}</p>
            <button
              type="button"
              disabled={paying}
              onClick={() => completePayment()}
              className="w-full rounded-xl bg-emerald-600 py-3 text-sm font-bold text-white hover:bg-emerald-700 disabled:opacity-60"
            >
              {paying ? "…" : t("featured.paySubmit")}
            </button>
          </div>
        ) : (
          <div className="mt-8">
            <CreditCardForm
              maskMiddle
              submitting={paying}
              defaultHolder=""
              labels={{
                cardBrand: "İlanMarket",
                cardHolder: t("featured.cardHolderLabel"),
                expires: t("featured.cardExpiresLabel"),
                cvvBack: "CVV",
                number: t("featured.cardNumberLabel"),
                holder: t("featured.cardHolderLabel"),
                expiration: t("featured.cardExpirationLabel"),
                cvv: "CVV",
                month: t("featured.cardMonthLabel"),
                year: t("featured.cardYearLabel"),
                submit: t("featured.paySubmit"),
                submitDisabled: t("featured.paySubmitDisabled"),
                numberInvalid: t("featured.cardNumberInvalid"),
              }}
              onSubmit={(state) => completePayment(state.holder, state.number)}
            />
          </div>
        )}

        <p className="mt-6 text-center text-xs text-[var(--muted-foreground)]">
          <Link href={`/ilan/${adId}`} className="text-blue-600 hover:underline dark:text-blue-400">
            {t("featured.viewAd")} →
          </Link>
        </p>
      </div>
    </div>
  );
}

export default function BuyPage() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <BuyContent />
    </Suspense>
  );
}
