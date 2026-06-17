"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useEffect, useMemo, useState } from "react";
import { ArrowLeft, ShieldCheck } from "lucide-react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { CreditCardForm } from "@/components/ui/credit-card-form";
import { Pricing2, type FeaturedPricingPlan } from "@/components/ui/pricing2";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api } from "@/lib/api";
import { pageContainerSm } from "@/lib/uiStyles";
import type { MessageKey } from "@/lib/i18n/messages";
import type { AdPackage } from "@/lib/types";

type Step = "plans" | "payment";

function buildPlans(
  packages: AdPackage[],
  t: (key: MessageKey, vars?: Record<string, string | number>) => string,
): FeaturedPricingPlan[] {
  return packages.map((pkg) => ({
    id: pkg.code || String(pkg.id),
    packageId: pkg.id,
    name: t("featured.packageName", { days: pkg.featuredDays }),
    description:
      pkg.featuredDays >= 30 ? t("featured.packageDescLong") : t("featured.packageDescShort"),
    priceLabel: `${pkg.price.toLocaleString("tr-TR")} TL`,
    price: pkg.price,
    periodLabel: t("featured.days", { count: pkg.featuredDays }),
    featuredDays: pkg.featuredDays,
    popular: pkg.featuredDays >= 30,
    features: [
      { text: t("featured.featureHomeTop") },
      { text: t("featured.featureBadge") },
      { text: t("featured.featureMoreViews") },
      ...(pkg.featuredDays >= 30 ? [{ text: t("featured.featureLongBoost") }] : []),
    ],
  }));
}

function FeaturedContent() {
  const params = useSearchParams();
  const adId = Number(params.get("adId") || 0);
  const router = useRouter();
  const { t } = useLocale();
  const { showToast } = useToast();
  const { isAuthenticated, isLoading } = useAuth();
  const [packages, setPackages] = useState<AdPackage[]>([]);
  const [loading, setLoading] = useState(true);
  const [step, setStep] = useState<Step>("plans");
  const [selectedPlan, setSelectedPlan] = useState<FeaturedPricingPlan | null>(null);
  const [purchaseId, setPurchaseId] = useState<number | null>(null);
  const [processingId, setProcessingId] = useState<number | null>(null);
  const [paying, setPaying] = useState(false);
  const [couponCode, setCouponCode] = useState("");
  const [couponDiscount, setCouponDiscount] = useState(0);
  const [couponMessage, setCouponMessage] = useState<string | null>(null);

  const plans = useMemo(() => buildPlans(packages, t), [packages, t]);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace(`/giris?redirect=${encodeURIComponent(`/one-cikan${adId ? `?adId=${adId}` : ""}`)}`);
      return;
    }
    if (!isLoading && isAuthenticated && !adId) {
      router.replace("/");
    }
  }, [isLoading, isAuthenticated, adId, router]);

  useEffect(() => {
    const variant =
      typeof window !== "undefined"
        ? (localStorage.getItem("pkg_ab_variant") ??
          (() => {
            const v = Math.random() < 0.5 ? "A" : "B";
            localStorage.setItem("pkg_ab_variant", v);
            return v;
          })())
        : "A";
    api.getAdPackages(variant).then((res) => {
      if (res.success && res.data) {
        setPackages(res.data.packages);
        void api.logPackageExperiment({ variant: res.data.variant, event: "view" });
      }
      setLoading(false);
    });
  }, []);

  async function startPurchase(plan: FeaturedPricingPlan) {
    if (!adId) {
      showToast(t("featured.pickAd"), "error");
      return;
    }
    setSelectedPlan(plan);
    setCouponCode("");
    setCouponDiscount(0);
    setCouponMessage(null);
    setPurchaseId(null);
    setStep("payment");
  }

  async function applyCoupon() {
    if (!selectedPlan || !couponCode.trim()) return;
    const res = await api.validateCoupon(couponCode.trim(), selectedPlan.price);
    if (res.success && res.data?.valid) {
      setCouponDiscount(res.data.discountAmount);
      setCouponMessage(t("coupon.discount", { amount: res.data.discountAmount.toLocaleString("tr-TR") }));
    } else {
      setCouponDiscount(0);
      setCouponMessage(res.data?.message || t("coupon.invalid"));
    }
  }

  async function completePayment() {
    if (!selectedPlan || !adId) return;
    setPaying(true);
    try {
      const checkout = await api.startCheckout(
        selectedPlan.packageId,
        adId,
        couponCode.trim() || undefined,
      );
      if (!checkout.success || !checkout.data) {
        showToast(checkout.message || t("featured.failed"), "error");
        return;
      }
      const { checkoutUrl, purchaseId: pid, isDemo, paymentProvider } = checkout.data;
      if (paymentProvider === "coupon" || (isDemo && checkoutUrl.includes("basarili"))) {
        showToast(t("featured.success"), "success");
        router.push(`/ilan/${adId}`);
        return;
      }
      if (!isDemo && checkoutUrl.startsWith("http")) {
        showToast(t("featured.stripeRedirect"), "success");
        window.location.href = checkoutUrl;
        return;
      }
      const done = await api.completeCheckout(pid);
      if (done.success) {
        showToast(t("featured.success"), "success");
        router.push(`/ilan/${adId}`);
      } else {
        showToast(done.message || t("featured.failed"), "error");
      }
    } finally {
      setPaying(false);
    }
  }

  if (loading || isLoading) return <LoadingSpinner />;

  return (
    <div className={pageContainerSm}>
      {step === "plans" ? (
        <>
          <Pricing2
            heading={t("featured.title")}
            description={t("featured.subtitle")}
            plans={plans}
            loading={loading}
            processingId={processingId}
            purchaseLabel={t("featured.buy")}
            popularLabel={t("featured.popular")}
            onPurchase={startPurchase}
          />
          <p className="pb-10 text-center text-xs text-[var(--muted-foreground)]">{t("featured.demoNote")}</p>
        </>
      ) : (
        selectedPlan && (
          <div className="py-8 md:py-12">
            <button
              type="button"
              onClick={() => {
                setStep("plans");
                setSelectedPlan(null);
                setPurchaseId(null);
              }}
              className="mb-6 inline-flex items-center gap-2 text-sm font-medium text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-white"
            >
              <ArrowLeft className="size-4" />
              {t("featured.backToPlans")}
            </button>

            <div className="mx-auto max-w-3xl text-center">
              <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white">{t("featured.payTitle")}</h1>
              <p className="mt-2 text-[var(--muted-foreground)]">{t("featured.paySubtitle")}</p>
            </div>

            <div className="mx-auto mt-8 max-w-xl rounded-2xl border border-slate-200 bg-[var(--card)] p-5 dark:border-slate-700">
              <div className="flex items-center justify-between gap-4">
                <div className="text-left">
                  <p className="text-sm text-[var(--muted-foreground)]">{selectedPlan.name}</p>
                  <p className="text-2xl font-bold">
                    {couponDiscount > 0
                      ? `${Math.max(0, selectedPlan.price - couponDiscount).toLocaleString("tr-TR")} TL`
                      : selectedPlan.priceLabel}
                  </p>
                  {couponDiscount > 0 && (
                    <p className="text-xs text-slate-500 line-through">{selectedPlan.priceLabel}</p>
                  )}
                </div>
                <div className="flex items-center gap-1.5 text-xs font-medium text-emerald-600 dark:text-emerald-400">
                  <ShieldCheck className="size-4" />
                  {t("featured.demoSecure")}
                </div>
              </div>
              <div className="mt-4 flex gap-2">
                <input
                  value={couponCode}
                  onChange={(e) => setCouponCode(e.target.value)}
                  placeholder={t("coupon.code")}
                  className="flex-1 rounded-lg border border-slate-200 px-3 py-2 text-sm dark:border-slate-600 dark:bg-slate-900"
                />
                <button
                  type="button"
                  onClick={applyCoupon}
                  className="rounded-lg bg-slate-100 px-4 py-2 text-sm font-medium dark:bg-slate-800"
                >
                  {t("coupon.apply")}
                </button>
              </div>
              {couponMessage && <p className="mt-2 text-xs text-slate-500">{couponMessage}</p>}
            </div>

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
                onSubmit={() => completePayment()}
              />
            </div>

            <p className="mt-6 text-center text-xs text-[var(--muted-foreground)]">
              {t("featured.demoNote")}{" "}
              <Link href={`/ilan/${adId}`} className="text-blue-600 hover:underline dark:text-blue-400">
                {t("featured.viewAd")} →
              </Link>
            </p>
          </div>
        )
      )}
    </div>
  );
}

export default function FeaturedPage() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <FeaturedContent />
    </Suspense>
  );
}
