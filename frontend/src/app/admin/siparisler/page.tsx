"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { adminApi } from "@/lib/admin";
import {
  adminBadge,
  adminBtnDanger,
  adminBtnPrimary,
  adminBtnSecondary,
  adminCardPad,
  adminInputFull,
} from "@/lib/adminStyles";
import type { MarketplaceOrder } from "@/lib/types";

const STATUS_OPTIONS = [
  { value: "", label: "Tüm durumlar" },
  { value: "0", label: "Ödeme bekleniyor" },
  { value: "1", label: "Ödeme güvende" },
  { value: "2", label: "Hazırlanıyor" },
  { value: "3", label: "Kargoda" },
  { value: "4", label: "Teslim edildi" },
  { value: "5", label: "Tamamlandı" },
  { value: "6", label: "İptal" },
  { value: "7", label: "İtiraz" },
];

export default function AdminOrdersPage() {
  const [items, setItems] = useState<MarketplaceOrder[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState("");
  const [search, setSearch] = useState("");
  const [disputedOnly, setDisputedOnly] = useState(false);
  const pageSize = 20;

  const load = useCallback(() => {
    if (disputedOnly) {
      adminApi.getDisputedOrders().then((r) => {
        if (r.success && r.data) {
          setItems(r.data);
          setTotal(r.data.length);
        }
      });
      return;
    }

    adminApi
      .getMarketplaceOrders({
        status: status ? Number(status) : undefined,
        search: search || undefined,
        page,
        pageSize,
      })
      .then((r) => {
        if (r.success && r.data) {
          setItems(r.data.items);
          setTotal(r.data.totalCount);
        }
      });
  }, [disputedOnly, status, search, page]);

  useEffect(() => {
    load();
  }, [load]);

  const totalPages = useMemo(() => Math.max(1, Math.ceil(total / pageSize)), [total]);

  const resolve = async (id: number, resolution: "complete" | "cancel") => {
    const note = prompt("Admin notu (opsiyonel):") ?? undefined;
    const res = await adminApi.resolveDispute(id, resolution, note);
    if (res.success) load();
    else alert(res.message || "İşlem başarısız");
  };

  const cancelOrder = async (id: number) => {
    if (!confirm("Sipariş iptal edilsin mi?")) return;
    const res = await adminApi.cancelOrder(id);
    if (res.success) load();
    else alert(res.message || "İptal başarısız");
  };

  const markRefund = async (id: number) => {
    const note = prompt("İade notu (opsiyonel):") ?? undefined;
    const res = await adminApi.markRefund(id, note);
    if (res.success) load();
    else alert(res.message || "İade kaydedilemedi");
  };

  const markPayout = async (id: number) => {
    const note = prompt("Satıcı ödeme notu (opsiyonel):") ?? undefined;
    const res = await adminApi.markSellerPayout(id, note);
    if (res.success) load();
    else alert(res.message || "Ödeme kaydedilemedi");
  };

  return (
    <div>
      <AdminPageHeader
        title="Escrow siparişleri"
        subtitle="Tüm siparişleri filtreleyin, iade ve satıcı ödemelerini yönetin"
      />

      <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end">
        <label className="block w-full min-w-0 text-sm text-slate-400 sm:w-auto sm:flex-1">
          Durum
          <select
            value={status}
            onChange={(e) => {
              setPage(1);
              setStatus(e.target.value);
              setDisputedOnly(false);
            }}
            className={`${adminInputFull} mt-1`}
          >
            {STATUS_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </label>
        <label className="block w-full min-w-0 text-sm text-slate-400 sm:w-auto sm:flex-1">
          Ara (sipariş # veya ilan)
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && (setPage(1), load())}
            className={`${adminInputFull} mt-1`}
            placeholder="#123 veya başlık"
          />
        </label>
        <button type="button" onClick={() => (setPage(1), load())} className={adminBtnSecondary}>
          Filtrele
        </button>
        <button
          type="button"
          onClick={() => {
            setDisputedOnly((v) => !v);
            setPage(1);
          }}
          className={disputedOnly ? adminBtnPrimary : adminBtnSecondary}
        >
          Yalnızca itirazlı
        </button>
      </div>

      <div className="space-y-4">
        {items.length === 0 && <p className="text-sm text-slate-400">Sipariş bulunamadı.</p>}
        {items.map((o) => (
          <div key={o.id} className={adminCardPad}>
            <div className="flex flex-wrap items-start justify-between gap-2">
              <p className="font-medium text-white">
                #{o.id} ·{" "}
                <Link
                  href={`/ilan/${o.advertisementId}`}
                  className="text-blue-400 hover:text-blue-300"
                  target="_blank"
                >
                  {o.advertisementTitle}
                </Link>
              </p>
              <span className={adminBadge("slate")}>{o.statusLabel}</span>
            </div>
            <p className="mt-1 text-sm text-slate-400">
              {o.amount.toLocaleString("tr-TR")} TL · Alıcı #{o.buyerUserId} · Satıcı #{o.sellerUserId}
            </p>
            {o.disputeReason && (
              <p className="mt-2 rounded-lg bg-amber-500/10 px-3 py-2 text-sm text-amber-200">
                İtiraz: {o.disputeReason}
              </p>
            )}
            {o.refundedAt && (
              <p className="mt-2 text-xs text-emerald-300">
                İade: {new Date(o.refundedAt).toLocaleString("tr")}
                {o.refundNote ? ` — ${o.refundNote}` : ""}
              </p>
            )}
            {o.sellerPaidOutAt && (
              <p className="mt-2 text-xs text-sky-300">
                Satıcı ödemesi: {new Date(o.sellerPaidOutAt).toLocaleString("tr")}
                {o.sellerPayoutNote ? ` — ${o.sellerPayoutNote}` : ""}
              </p>
            )}
            <div className="mt-4 flex flex-wrap gap-2">
              {o.status === 7 && (
                <>
                  <button type="button" onClick={() => resolve(o.id, "complete")} className={adminBtnSecondary}>
                    Satışı tamamla
                  </button>
                  <button type="button" onClick={() => resolve(o.id, "cancel")} className={adminBtnDanger}>
                    İptal / iade
                  </button>
                </>
              )}
              {o.status !== 6 && o.status !== 5 && (
                <button type="button" onClick={() => cancelOrder(o.id)} className={adminBtnDanger}>
                  Admin iptal
                </button>
              )}
              {o.status === 6 && !o.refundedAt && (
                <button type="button" onClick={() => markRefund(o.id)} className={adminBtnSecondary}>
                  İade işaretle
                </button>
              )}
              {o.status === 5 && !o.sellerPaidOutAt && (
                <button type="button" onClick={() => markPayout(o.id)} className={adminBtnPrimary}>
                  Satıcıya ödendi
                </button>
              )}
            </div>
          </div>
        ))}
      </div>

      {!disputedOnly && totalPages > 1 && (
        <div className="mt-6 flex items-center justify-center gap-3 text-sm text-slate-400">
          <button
            type="button"
            disabled={page <= 1}
            onClick={() => setPage((p) => p - 1)}
            className={adminBtnSecondary}
          >
            Önceki
          </button>
          <span>
            {page} / {totalPages}
          </span>
          <button
            type="button"
            disabled={page >= totalPages}
            onClick={() => setPage((p) => p + 1)}
            className={adminBtnSecondary}
          >
            Sonraki
          </button>
        </div>
      )}
    </div>
  );
}
