"use client";

import { useCallback, useEffect, useState } from "react";
import { api, ApiError } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { formatPrice } from "@/lib/listingDetails";
import type { AuctionInfo } from "@/lib/types";
import { formFieldClass as inputClass } from "@/lib/formStyles";
import { btnBrand, surfaceElevated } from "@/lib/uiStyles";

type AuctionPanelProps = {
  advertisementId: number;
  isOwner: boolean;
  initial?: AuctionInfo | null;
};

export function AuctionPanel({ advertisementId, isOwner, initial }: AuctionPanelProps) {
  const { t, locale } = useLocale();
  const { isAuthenticated } = useAuth();
  const [auction, setAuction] = useState<AuctionInfo | null>(initial ?? null);
  const [bidAmount, setBidAmount] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [now, setNow] = useState(Date.now());

  const load = useCallback(async () => {
    try {
      const res = await api.getAuctionByAd(advertisementId);
      if (res.success && res.data) setAuction(res.data);
    } catch {
      /* müzayede kaydı yok */
    }
  }, [advertisementId]);

  useEffect(() => {
    if (!initial) void load();
  }, [initial, load]);

  useEffect(() => {
    const id = window.setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(id);
  }, []);

  if (!auction) return null;

  const ends = new Date(auction.endsAt).getTime();
  const starts = new Date(auction.startsAt).getTime();
  const remaining = Math.max(0, ends - now);
  const notStarted = now < starts;
  const ended = auction.status === "ended" || remaining <= 0;
  const minBid = (auction.currentBid ?? auction.startingBid) + auction.minIncrement;

  const formatRemaining = (ms: number) => {
    const s = Math.floor(ms / 1000);
    const h = Math.floor(s / 3600);
    const m = Math.floor((s % 3600) / 60);
    const sec = s % 60;
    return `${h}sa ${m}dk ${sec}sn`;
  };

  async function placeBid() {
    setError("");
    setLoading(true);
    try {
      const amount = Number(bidAmount);
      const res = await api.placeAuctionBid(auction!.id, amount);
      if (!res.success || !res.data) throw new ApiError(res.message, 400);
      setAuction(res.data);
      setBidAmount("");
    } catch (e) {
      setError(e instanceof ApiError || e instanceof Error ? e.message : "Teklif verilemedi");
    } finally {
      setLoading(false);
    }
  }

  return (
    <section className={`p-5 ${surfaceElevated}`}>
      <h2 className="text-sm font-bold uppercase tracking-wide text-amber-600">{t("auction.title")}</h2>

      <dl className="mt-4 grid gap-2 text-sm sm:grid-cols-2">
        <div>
          <dt className="text-slate-500">{t("auction.current")}</dt>
          <dd className="text-xl font-bold text-slate-900 dark:text-white">
            {formatPrice(auction.currentBid ?? auction.startingBid, t("price.notSet"), locale)}
          </dd>
        </div>
        <div>
          <dt className="text-slate-500">{t("auction.minNext")}</dt>
          <dd className="font-semibold">{formatPrice(minBid, "", locale)}</dd>
        </div>
        <div>
          <dt className="text-slate-500">{t("auction.bids")}</dt>
          <dd>{auction.bidCount}</dd>
        </div>
        <div>
          <dt className="text-slate-500">{t("auction.timeLeft")}</dt>
          <dd>{ended ? t("auction.ended") : notStarted ? t("auction.notStarted") : formatRemaining(remaining)}</dd>
        </div>
      </dl>

      {!isOwner && isAuthenticated && !ended && !notStarted && (
        <div className="mt-4 flex flex-wrap gap-2">
          <input
            type="number"
            min={minBid}
            step={auction.minIncrement}
            value={bidAmount}
            onChange={(e) => setBidAmount(e.target.value)}
            placeholder={String(minBid)}
            className={`${inputClass} max-w-[12rem]`}
          />
          <button type="button" disabled={loading} onClick={placeBid} className={btnBrand}>
            {t("auction.placeBid")}
          </button>
        </div>
      )}

      {error && <p className="mt-2 text-sm text-rose-600">{error}</p>}

      {auction.recentBids.length > 0 && (
        <ul className="mt-4 space-y-1 border-t border-slate-100 pt-3 text-sm dark:border-slate-800">
          {auction.recentBids.map((b) => (
            <li key={b.id} className="flex justify-between text-slate-600 dark:text-slate-400">
              <span>{b.userDisplayName ?? `Kullanıcı #${b.userId}`}</span>
              <span className="font-medium">{formatPrice(b.amount, "", locale)}</span>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
