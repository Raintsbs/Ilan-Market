"use client";

import Link from "next/link";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import { ExternalLink, Package } from "lucide-react";
import { InteractiveStarRating } from "@/components/StarRating";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { PageHeader } from "@/components/PageHeader";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api, ApiError } from "@/lib/api";
import { formatPrice } from "@/lib/listingDetails";
import { formFieldClass } from "@/lib/formStyles";
import { formatDate } from "@/lib/status";
import { linkBack, pageContainerMd, surfaceCard } from "@/lib/uiStyles";
import type { CargoCarrier, MarketplaceOrder } from "@/lib/types";

export default function OrderDetailPage() {
  const params = useParams();
  const searchParams = useSearchParams();
  const orderId = Number(params.id);
  const router = useRouter();
  const { t, locale } = useLocale();
  const { showToast } = useToast();
  const { user, isAuthenticated, isLoading } = useAuth();
  const [order, setOrder] = useState<MarketplaceOrder | null>(null);
  const [carriers, setCarriers] = useState<CargoCarrier[]>([]);
  const [loading, setLoading] = useState(true);
  const [carrierCode, setCarrierCode] = useState("");
  const [tracking, setTracking] = useState("");
  const [shipping, setShipping] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");
  const [reviewing, setReviewing] = useState(false);
  const [adRating, setAdRating] = useState(5);
  const [adComment, setAdComment] = useState("");
  const [reviewingAd, setReviewingAd] = useState(false);
  const [buyerRating, setBuyerRating] = useState(5);
  const [buyerComment, setBuyerComment] = useState("");
  const [reviewingBuyer, setReviewingBuyer] = useState(false);
  const [disputeReason, setDisputeReason] = useState("");
  const [disputing, setDisputing] = useState(false);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) router.replace(`/giris?redirect=/siparisler/${orderId}`);
  }, [isLoading, isAuthenticated, orderId, router]);

  async function load() {
    const res = await api.getMarketplaceOrder(orderId);
    if (res.success && res.data) setOrder(res.data);
    setLoading(false);
  }

  useEffect(() => {
    if (!isAuthenticated || !orderId) return;
    const sessionId = searchParams.get("session_id");
    if (searchParams.get("paid") === "1" && sessionId) {
      void api.completeStripeSession(sessionId).then(() => load());
      return;
    }
    load();
    api.getCargoCarriers().then((res) => {
      if (res.success && res.data) {
        setCarriers(res.data);
        if (res.data[0]) setCarrierCode(res.data[0].code);
      }
    });
  }, [isAuthenticated, orderId]);

  const isBuyer = user && order && user.userId === order.buyerUserId;
  const isSeller = user && order && user.userId === order.sellerUserId;
  const canShip = isSeller && order && (order.status === 1 || order.status === 2);
  const canConfirm = isBuyer && order && (order.status === 3 || order.status === 4);

  async function handleShip(e: React.FormEvent) {
    e.preventDefault();
    if (!tracking.trim()) return;
    setShipping(true);
    try {
      const res = await api.shipMarketplaceOrder(orderId, carrierCode, tracking.trim());
      if (res.success && res.data) {
        setOrder(res.data);
        showToast(t("orders.shipOk"), "success");
      } else showToast(res.message || t("buy.failed"), "error");
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("buy.failed"), "error");
    } finally {
      setShipping(false);
    }
  }

  async function handleConfirm() {
    setConfirming(true);
    try {
      const res = await api.confirmMarketplaceDelivery(orderId);
      if (res.success && res.data) {
        setOrder(res.data);
        showToast(t("orders.confirmOk"), "success");
      } else showToast(res.message || t("buy.failed"), "error");
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("buy.failed"), "error");
    } finally {
      setConfirming(false);
    }
  }

  async function handleReview(e: React.FormEvent) {
    e.preventDefault();
    if (!order) return;
    setReviewing(true);
    try {
      const res = await api.createSellerReview(order.sellerUserId, order.id, rating, comment || undefined);
      if (res.success) {
        showToast(t("reviews.sent"), "success");
        await load();
      } else showToast(res.message || t("reviews.failed"), "error");
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("reviews.failed"), "error");
    } finally {
      setReviewing(false);
    }
  }

  async function handleAdReview(e: React.FormEvent) {
    e.preventDefault();
    if (!order) return;
    setReviewingAd(true);
    try {
      const res = await api.createAdvertisementReview(
        order.advertisementId,
        order.id,
        adRating,
        adComment || undefined,
      );
      if (res.success) {
        showToast(t("reviews.adSent"), "success");
        await load();
      } else showToast(res.message || t("reviews.failed"), "error");
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("reviews.failed"), "error");
    } finally {
      setReviewingAd(false);
    }
  }

  async function handleBuyerReview(e: React.FormEvent) {
    e.preventDefault();
    if (!order) return;
    setReviewingBuyer(true);
    try {
      const res = await api.createBuyerReview(
        order.buyerUserId,
        order.id,
        buyerRating,
        buyerComment || undefined,
      );
      if (res.success) {
        showToast(t("reviews.buyerSent"), "success");
        await load();
      } else showToast(res.message || t("reviews.failed"), "error");
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("reviews.failed"), "error");
    } finally {
      setReviewingBuyer(false);
    }
  }

  async function handleDispute(e: React.FormEvent) {
    e.preventDefault();
    if (!disputeReason.trim()) return;
    setDisputing(true);
    try {
      const res = await api.openMarketplaceDispute(orderId, disputeReason.trim());
      if (res.success && res.data) {
        setOrder(res.data);
        showToast(t("orders.disputeOk"), "success");
        setDisputeReason("");
      } else showToast(res.message || t("orders.disputeFailed"), "error");
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("orders.disputeFailed"), "error");
    } finally {
      setDisputing(false);
    }
  }

  if (isLoading || loading) return <LoadingSpinner />;
  if (!order) {
    return (
      <div className={`${pageContainerMd} py-16 text-center text-slate-500`}>
        {t("ad.notFound")}
      </div>
    );
  }

  return (
    <div className={pageContainerMd}>
      <PageHeader title={t("orders.detail")} />
      <div className={`mt-6 p-6 ${surfaceCard}`}>
        <h2 className="text-lg font-semibold text-slate-900 dark:text-white">
          {order.advertisementTitle}
        </h2>
        <dl className="mt-4 space-y-2 text-sm">
          <div className="flex justify-between">
            <dt className="text-slate-500">{t("orders.status")}</dt>
            <dd className="font-medium">{order.statusLabel}</dd>
          </div>
          <div className="flex justify-between">
            <dt className="text-slate-500">{t("buy.amount")}</dt>
            <dd className="font-bold text-emerald-700 dark:text-emerald-400">{formatPrice(order.amount)}</dd>
          </div>
          <div className="flex justify-between">
            <dt className="text-slate-500">{t("buy.method")}</dt>
            <dd className="font-medium">
              {order.paymentMethod === "param_guvende" ? t("buy.paramGuvende") : t("buy.card")}
            </dd>
          </div>
          <div className="flex justify-between">
            <dt className="text-slate-500">Tarih</dt>
            <dd>{formatDate(order.createdTime, locale)}</dd>
          </div>
        </dl>
        {order.disputeReason && (
          <div className="mt-4 rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900 dark:border-amber-800 dark:bg-amber-950/40 dark:text-amber-200">
            <p className="font-medium">{t("orders.dispute")}</p>
            <p className="mt-1">{order.disputeReason}</p>
            {order.disputeResolutionNote && (
              <p className="mt-2 text-xs opacity-80">Admin: {order.disputeResolutionNote}</p>
            )}
          </div>
        )}
        <Link
          href={`/ilan/${order.advertisementId}`}
          className="mt-4 inline-block text-sm text-blue-600 hover:underline dark:text-blue-400"
        >
          {t("featured.viewAd")} →
        </Link>
      </div>

      {order.shipment && (
        <div className={`mt-6 p-6 ${surfaceCard}`}>
          <div className="flex items-center gap-2">
            <Package className="size-5 text-blue-600" />
            <h3 className="font-semibold text-slate-900 dark:text-white">{t("orders.cargoSafe")}</h3>
          </div>
          <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">
            {order.shipment.carrierName} · {order.shipment.trackingNumber}
          </p>
          {order.shipment.trackingUrl && (
            <a
              href={order.shipment.trackingUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="mt-4 inline-flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-blue-700"
            >
              {t("orders.trackCargo")}
              <ExternalLink className="size-4" />
            </a>
          )}
        </div>
      )}

      {canShip && (
        <form onSubmit={handleShip} className={`mt-6 p-6 ${surfaceCard}`}>
          <h3 className="font-semibold text-slate-900 dark:text-white">{t("orders.ship")}</h3>
          <div className="mt-4 grid gap-4 sm:grid-cols-2">
            <div>
              <label className="mb-1 block text-sm font-medium">{t("orders.carrier")}</label>
              <select
                value={carrierCode}
                onChange={(e) => setCarrierCode(e.target.value)}
                className={formFieldClass}
              >
                {carriers.map((c) => (
                  <option key={c.code} value={c.code}>{c.name}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium">{t("orders.tracking")}</label>
              <input
                required
                value={tracking}
                onChange={(e) => setTracking(e.target.value)}
                className={formFieldClass}
              />
            </div>
          </div>
          <button
            type="submit"
            disabled={shipping}
            className="mt-4 rounded-xl bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white disabled:opacity-60"
          >
            {shipping ? "…" : t("orders.ship")}
          </button>
        </form>
      )}

      {canConfirm && (
        <div className={`mt-6 p-6 ${surfaceCard}`}>
          <p className="text-sm text-slate-600 dark:text-slate-400">{t("buy.paramGuvendeHint")}</p>
          <button
            type="button"
            disabled={confirming}
            onClick={handleConfirm}
            className="mt-4 rounded-xl bg-emerald-600 px-6 py-2.5 text-sm font-semibold text-white disabled:opacity-60"
          >
            {confirming ? "…" : t("orders.confirmDelivery")}
          </button>
        </div>
      )}

      {order.canOpenDispute && (
        <form onSubmit={handleDispute} className={`mt-6 p-6 ${surfaceCard}`}>
          <h3 className="font-semibold text-slate-900 dark:text-white">{t("orders.dispute")}</h3>
          <label className="mt-3 block text-sm font-medium text-slate-600 dark:text-slate-400">
            {t("orders.disputeReason")}
          </label>
          <textarea
            required
            value={disputeReason}
            onChange={(e) => setDisputeReason(e.target.value)}
            placeholder={t("orders.disputePlaceholder")}
            rows={3}
            className={`mt-1 ${formFieldClass}`}
          />
          <button
            type="submit"
            disabled={disputing}
            className="mt-4 rounded-xl border border-amber-500 bg-amber-50 px-6 py-2.5 text-sm font-semibold text-amber-900 hover:bg-amber-100 disabled:opacity-60 dark:bg-amber-950/30 dark:text-amber-200"
          >
            {disputing ? "…" : t("orders.dispute")}
          </button>
        </form>
      )}

      {order.canReview && isBuyer && (
        <form onSubmit={handleReview} className={`mt-6 p-6 ${surfaceCard}`}>
          <h3 className="font-semibold text-slate-900 dark:text-white">{t("reviews.writeSeller")}</h3>
          <div className="mt-3">
            <InteractiveStarRating value={rating} onChange={setRating} />
          </div>
          <textarea
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            placeholder={t("reviews.comment")}
            rows={3}
            className={`mt-3 ${formFieldClass}`}
          />
          <button
            type="submit"
            disabled={reviewing}
            className="mt-4 rounded-xl bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white disabled:opacity-60"
          >
            {reviewing ? "…" : t("reviews.submit")}
          </button>
        </form>
      )}

      {order.canReviewAd && isBuyer && (
        <form onSubmit={handleAdReview} className={`mt-6 p-6 ${surfaceCard}`}>
          <h3 className="font-semibold text-slate-900 dark:text-white">{t("reviews.writeAd")}</h3>
          <div className="mt-3">
            <InteractiveStarRating value={adRating} onChange={setAdRating} />
          </div>
          <textarea
            value={adComment}
            onChange={(e) => setAdComment(e.target.value)}
            placeholder={t("reviews.comment")}
            rows={3}
            className={`mt-3 ${formFieldClass}`}
          />
          <button
            type="submit"
            disabled={reviewingAd}
            className="mt-4 rounded-xl bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white disabled:opacity-60"
          >
            {reviewingAd ? "…" : t("reviews.submit")}
          </button>
        </form>
      )}

      {order.canReviewBuyer && isSeller && (
        <form onSubmit={handleBuyerReview} className={`mt-6 p-6 ${surfaceCard}`}>
          <h3 className="font-semibold text-slate-900 dark:text-white">{t("reviews.writeBuyer")}</h3>
          <div className="mt-3">
            <InteractiveStarRating value={buyerRating} onChange={setBuyerRating} />
          </div>
          <textarea
            value={buyerComment}
            onChange={(e) => setBuyerComment(e.target.value)}
            placeholder={t("reviews.comment")}
            rows={3}
            className={`mt-3 ${formFieldClass}`}
          />
          <button
            type="submit"
            disabled={reviewingBuyer}
            className="mt-4 rounded-xl bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white disabled:opacity-60"
          >
            {reviewingBuyer ? "…" : t("reviews.submit")}
          </button>
        </form>
      )}

      <Link href="/siparisler" className={`mt-8 ${linkBack}`}>
        ← {t("orders.title")}
      </Link>
    </div>
  );
}
