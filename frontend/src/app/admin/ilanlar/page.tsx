"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { adminApi } from "@/lib/admin";
import {
  AdvertisementStatus,
  type Advertisement,
  type AuditLog,
  type PagedResult,
} from "@/lib/types";
import { AdminConfirmModal } from "@/components/admin/AdminConfirmModal";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import {
  adminAlertInfo,
  adminBtnDanger,
  adminBtnPrimary,
  adminBtnSuccess,
  adminBtnWarning,
  adminInput,
  adminSelect,
  adminTable,
  adminTableWrap,
  adminTd,
  adminTh,
  adminTr,
  adminBadge,
  adminBtnChip,
  adminBtnChipDanger,
  adminBtnSecondary,
  adminCardPad,
  adminModalOverlay,
  adminModalPanel,
  adminPaginationBtn,
} from "@/lib/adminStyles";
import { useAdminAuth } from "@/context/AdminAuthContext";
import { isAdmin } from "@/lib/admin";
import { useAdsChangeListener } from "@/lib/adsSync";

const statusLabel: Record<number, string> = {
  [AdvertisementStatus.Pending]: "Bekliyor",
  [AdvertisementStatus.Approved]: "Onaylı",
  [AdvertisementStatus.Rejected]: "Reddedildi",
};

export default function AdminAdsPage() {
  const { user } = useAdminAuth();
  const canDelete = isAdmin(user?.roles);

  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [expiredOnly, setExpiredOnly] = useState(false);

  useEffect(() => {
    const sp = new URLSearchParams(window.location.search);
    if (sp.get("status")) setStatus(sp.get("status")!);
    if (sp.get("expired") === "1") setExpiredOnly(true);
  }, []);
  const [page, setPage] = useState(1);
  const [data, setData] = useState<PagedResult<Advertisement> | null>(null);
  const [selected, setSelected] = useState<Set<number>>(new Set());
  const [history, setHistory] = useState<AuditLog[] | null>(null);
  const [historyId, setHistoryId] = useState<number | null>(null);
  const [msg, setMsg] = useState("");
  const [deleteTarget, setDeleteTarget] = useState<{
    ids: number[];
    label: string;
  } | null>(null);
  const [deleteLoading, setDeleteLoading] = useState(false);

  const load = useCallback(async () => {
    const res = await adminApi.getAds({
      search: search || undefined,
      status: status !== "" ? Number(status) : undefined,
      page,
      pageSize: 20,
      expiredOnly,
    });
    if (res.success && res.data) setData(res.data);
  }, [search, status, page, expiredOnly]);

  useEffect(() => {
    load();
  }, [load]);

  useAdsChangeListener(load);

  const toggle = (id: number) => {
    setSelected((prev) => {
      const n = new Set(prev);
      if (n.has(id)) n.delete(id);
      else n.add(id);
      return n;
    });
  };

  const runDelete = async (ids: number[]) => {
    setDeleteLoading(true);
    try {
      if (ids.length === 1) {
        const res = await adminApi.deleteAd(ids[0]);
        setMsg(res.message);
      } else {
        const res = await adminApi.bulkAds(ids, "delete");
        setMsg(res.message);
      }
      setSelected(new Set());
      setDeleteTarget(null);
      load();
    } catch (err) {
      setMsg(err instanceof Error ? err.message : "Silme başarısız.");
    } finally {
      setDeleteLoading(false);
    }
  };

  const bulk = async (action: string) => {
    const ids = [...selected];
    if (!ids.length) return;
    if (action === "delete") {
      setDeleteTarget({
        ids,
        label: `${ids.length} ilan`,
      });
      return;
    }
    const reason = action === "reject" ? prompt("Red sebebi:") ?? undefined : undefined;
    try {
      const res = await adminApi.bulkAds(ids, action, reason);
      setMsg(res.message);
      setSelected(new Set());
      load();
    } catch (err) {
      setMsg(err instanceof Error ? err.message : "Toplu işlem başarısız.");
    }
  };

  const showHistory = async (id: number) => {
    setHistoryId(id);
    const res = await adminApi.adHistory(id);
    setHistory(res.data ?? []);
  };

  return (
    <div>
      <AdminPageHeader
        title="İlan yönetimi"
        subtitle="Onay, red, öne çıkarma ve toplu işlemler"
      />

      <div className="mb-6 flex flex-wrap gap-2">
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Ara..."
          className={adminInput}
        />
        <select
          value={status}
          onChange={(e) => {
            setStatus(e.target.value);
            setPage(1);
          }}
          className={adminSelect}
        >
          <option value="">Tüm durumlar</option>
          <option value="0">Bekliyor</option>
          <option value="1">Onaylı</option>
          <option value="2">Reddedildi</option>
        </select>
        <label className="flex items-center gap-2 text-sm text-slate-300">
          <input
            type="checkbox"
            checked={expiredOnly}
            onChange={(e) => {
              setExpiredOnly(e.target.checked);
              setPage(1);
            }}
          />
          Süresi dolan
        </label>
        <button type="button" onClick={() => load()} className={adminBtnPrimary}>
          Filtrele
        </button>
      </div>

      {selected.size > 0 && (
        <div className={`mb-4 flex flex-wrap items-center gap-2 ${adminCardPad}`}>
          <span className="text-sm text-slate-400">{selected.size} seçili</span>
          <button type="button" onClick={() => bulk("approve")} className={adminBtnSuccess}>
            Toplu onayla
          </button>
          <button type="button" onClick={() => bulk("reject")} className={adminBtnWarning}>
            Toplu reddet
          </button>
          {canDelete && (
            <button type="button" onClick={() => bulk("delete")} className={adminBtnDanger}>
              Toplu sil
            </button>
          )}
        </div>
      )}

      {msg && <p className={`mb-4 ${adminAlertInfo}`}>{msg}</p>}

      <div className={adminTableWrap}>
        <table className={adminTable}>
          <thead>
            <tr>
              <th className={`${adminTh} w-10`} />
              <th className={adminTh}>Başlık</th>
              <th className={adminTh}>Kategori</th>
              <th className={adminTh}>Durum</th>
              <th className={adminTh}>Öne çıkan</th>
              <th className={adminTh}>İşlem</th>
            </tr>
          </thead>
          <tbody>
            {data?.items.map((ad) => (
              <tr key={ad.id} className={adminTr}>
                <td className={adminTd} data-label="">
                  <input type="checkbox" checked={selected.has(ad.id)} onChange={() => toggle(ad.id)} />
                </td>
                <td className={adminTd} data-label="Başlık">
                  <Link href={`/ilan/${ad.id}`} className="text-blue-400 hover:text-blue-300" target="_blank">
                    {ad.title}
                  </Link>
                </td>
                <td className={`${adminTd} text-slate-400`} data-label="Kategori">
                  {ad.categoryName}
                </td>
                <td className={adminTd} data-label="Durum">
                  <span className={adminBadge(ad.status === AdvertisementStatus.Pending ? "amber" : ad.status === AdvertisementStatus.Approved ? "green" : "rose")}>
                    {statusLabel[ad.status] ?? ad.status}
                  </span>
                </td>
                <td className={adminTd} data-label="Öne çıkan">
                  {ad.isFeatured ? "Evet" : "—"}
                </td>
                <td className={adminTd} data-label="İşlem">
                  <div className="flex flex-wrap gap-1">
                    {ad.status === AdvertisementStatus.Pending && (
                      <>
                        <ActionBtn label="Onayla" onClick={() => adminApi.approveAd(ad.id).then(load)} />
                        <ActionBtn
                          label="Reddet"
                          onClick={async () => {
                            const reason = prompt("Sebep:");
                            await adminApi.rejectAd(ad.id, reason ?? undefined);
                            load();
                          }}
                        />
                      </>
                    )}
                    <ActionBtn
                      label="Öne çıkar"
                      onClick={async () => {
                        await adminApi.featureAd(ad.id, 7);
                        load();
                      }}
                    />
                    <ActionBtn label="Uzat" onClick={() => adminApi.extendAd(ad.id, 30).then(load)} />
                    <ActionBtn label="Arşiv" onClick={() => adminApi.archiveAd(ad.id).then(load)} />
                    {canDelete && (
                      <ActionBtn
                        label="Sil"
                        danger
                        onClick={() =>
                          setDeleteTarget({
                            ids: [ad.id],
                            label: ad.title,
                          })
                        }
                      />
                    )}
                    <ActionBtn label="Geçmiş" onClick={() => showHistory(ad.id)} />
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {data && (
        <div className="mt-6 flex flex-wrap items-center gap-3 text-sm text-slate-400">
          <button type="button" disabled={!data.hasPrevious} onClick={() => setPage((p) => p - 1)} className={adminPaginationBtn}>
            Önceki
          </button>
          <span>
            Sayfa {data.page} / {data.totalPages} ({data.totalCount} kayıt)
          </span>
          <button type="button" disabled={!data.hasNext} onClick={() => setPage((p) => p + 1)} className={adminPaginationBtn}>
            Sonraki
          </button>
        </div>
      )}

      <AdminConfirmModal
        open={deleteTarget != null}
        title="İlanı sil"
        message={
          deleteTarget
            ? deleteTarget.ids.length === 1
              ? `"${deleteTarget.label}" kalıcı olarak silinecek. Bu işlem geri alınamaz.`
              : `${deleteTarget.label} kalıcı olarak silinecek. Bu işlem geri alınamaz.`
            : ""
        }
        confirmLabel="Evet, sil"
        cancelLabel="Vazgeç"
        danger
        loading={deleteLoading}
        onCancel={() => !deleteLoading && setDeleteTarget(null)}
        onConfirm={() => deleteTarget && void runDelete(deleteTarget.ids)}
      />

      {historyId != null && (
        <div className={adminModalOverlay}>
          <div className={`${adminModalPanel} max-w-lg`}>
            <h2 className="text-lg font-bold text-white">İlan #{historyId} geçmişi</h2>
            <ul className="mt-4 space-y-3 text-sm">
              {history?.map((h) => (
                <li key={h.id} className="border-b border-white/[0.06] pb-3 text-slate-300">
                  <span className="font-medium text-white">{h.actorEmail}</span> — {h.action}
                  {h.details && <span className="mt-0.5 block text-slate-500">{h.details}</span>}
                  <span className="mt-1 block text-xs text-slate-600">{new Date(h.createdTime).toLocaleString("tr")}</span>
                </li>
              ))}
            </ul>
            <button
              type="button"
              className={`mt-6 ${adminBtnSecondary}`}
              onClick={() => {
                setHistoryId(null);
                setHistory(null);
              }}
            >
              Kapat
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

function ActionBtn({
  label,
  onClick,
  danger,
}: {
  label: string;
  onClick: () => void;
  danger?: boolean;
}) {
  return (
    <button type="button" onClick={onClick} className={danger ? adminBtnChipDanger : adminBtnChip}>
      {label}
    </button>
  );
}
