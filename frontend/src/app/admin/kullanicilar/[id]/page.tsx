"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { AdminLoading } from "@/components/admin/AdminLoading";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { AdminPromptModal } from "@/components/admin/AdminPromptModal";
import { adminApi, isStaff } from "@/lib/admin";
import { ApiError } from "@/lib/api";
import {
  adminBadge,
  adminBtnDanger,
  adminBtnPrimary,
  adminBtnSuccess,
  adminBtnWarning,
  adminAlertInfo,
  adminCardPad,
  adminSelect,
} from "@/lib/adminStyles";
import { useAdminAuth } from "@/context/AdminAuthContext";
import type { AdminUserDetail } from "@/lib/types";

export default function AdminUserDetailPage() {
  const { id } = useParams();
  const userId = Number(id);
  const { user: me } = useAdminAuth();
  const [u, setU] = useState<AdminUserDetail | null>(null);
  const [banModalOpen, setBanModalOpen] = useState(false);
  const [banLoading, setBanLoading] = useState(false);
  const [actionMsg, setActionMsg] = useState("");
  const [actionError, setActionError] = useState("");
  const canModerate = isStaff(me?.roles);

  const load = () => {
    adminApi.getUser(userId).then((r) => {
      if (r.success && r.data) setU(r.data);
    });
  };

  useEffect(() => {
    if (!Number.isNaN(userId)) load();
  }, [userId]);

  const moderate = async (body: Record<string, unknown>) => {
    setActionError("");
    setActionMsg("");
    const res = await adminApi.moderateUser(userId, body);
    if (!res.success) {
      throw new ApiError(res.message || "İşlem başarısız", 400);
    }
    load();
  };

  const handleBanClick = async () => {
    if (u?.isBanned) {
      try {
        await moderate({ isBanned: false });
        setActionMsg("Ban kaldırıldı.");
      } catch (err) {
        setActionError(err instanceof ApiError ? err.message : "İşlem başarısız.");
      }
      return;
    }
    setBanModalOpen(true);
  };

  const confirmBan = async (banReason: string) => {
    setBanLoading(true);
    setActionError("");
    try {
      await moderate({ isBanned: true, banReason: banReason || undefined });
      setBanModalOpen(false);
      setActionMsg("Kullanıcı banlandı. Aktif oturumları kapatılacak.");
    } catch (err) {
      setActionError(err instanceof ApiError ? err.message : "Ban uygulanamadı.");
    } finally {
      setBanLoading(false);
    }
  };

  if (!u) return <AdminLoading />;

  return (
    <div>
      <Link href="/admin/kullanicilar" className="text-sm font-medium text-blue-400 hover:text-blue-300">
        ← Kullanıcı listesi
      </Link>

      <div className="mt-4">
        <AdminPageHeader
          title={`${u.firstName} ${u.lastName}`}
          subtitle={u.email}
          actions={
            <div className="flex flex-wrap gap-2">
              {u.roles.map((r) => (
                <span key={r} className={adminBadge("blue")}>
                  {r}
                </span>
              ))}
              <span className={adminBadge("slate")}>{u.adCount} ilan</span>
            </div>
          }
        />
      </div>

      {actionMsg && <p className={`mb-4 ${adminAlertInfo}`}>{actionMsg}</p>}
      {actionError && (
        <p className="mb-4 rounded-xl border border-rose-500/20 bg-rose-500/10 px-4 py-3 text-sm text-rose-200">
          {actionError}
        </p>
      )}

      <div className="mb-4 flex flex-wrap gap-2">
        {u.isBanned && <span className={adminBadge("rose")}>Banlı</span>}
        {u.isFrozen && <span className={adminBadge("amber")}>Dondurulmuş</span>}
        {u.isVerified && <span className={adminBadge("green")}>Doğrulanmış</span>}
        {u.phoneVerified && <span className={adminBadge("blue")}>Telefon ✓</span>}
        {u.warningCount > 0 && <span className={adminBadge("amber")}>Uyarı: {u.warningCount}</span>}
      </div>

      {canModerate && (
        <div className={`mb-8 flex flex-wrap gap-2 ${adminCardPad}`}>
          <button type="button" className={adminBtnDanger} onClick={handleBanClick}>
            {u.isBanned ? "Banı kaldır" : "Banla"}
          </button>
          <button
            type="button"
            className={adminBtnWarning}
            onClick={() => moderate({ isFrozen: !u.isFrozen, frozenDays: 7 })}
          >
            {u.isFrozen ? "Dondurmayı kaldır" : "7 gün dondur"}
          </button>
          <button type="button" className={adminBtnWarning} onClick={() => moderate({ addWarning: true })}>
            Uyarı ver
          </button>
          <button type="button" className={adminBtnSuccess} onClick={() => moderate({ isVerified: true })}>
            Kimlik doğrula
          </button>
          <button type="button" className={adminBtnPrimary} onClick={() => moderate({ phoneVerified: true })}>
            Telefon doğrula
          </button>
          <select
            className={adminSelect}
            defaultValue=""
            onChange={(e) => {
              const role = e.target.value;
              if (role) moderate({ role });
              e.target.value = "";
            }}
          >
            <option value="">Rol ata...</option>
            <option value="Admin">Admin</option>
            <option value="Moderator">Moderatör</option>
            <option value="User">Üye</option>
          </select>
        </div>
      )}

      <section className="mb-8">
        <h2 className="text-lg font-semibold text-white">Son ilanlar</h2>
        <ul className="mt-3 space-y-2">
          {u.recentAds.map((ad) => (
            <li key={ad.id} className={adminCardPad}>
              <Link href={`/ilan/${ad.id}`} className="text-sm font-medium text-blue-400 hover:text-blue-300" target="_blank">
                {ad.title}
              </Link>
            </li>
          ))}
          {!u.recentAds.length && <p className="text-sm text-slate-500">İlan yok.</p>}
        </ul>
      </section>

      <section>
        <h2 className="text-lg font-semibold text-white">Aktivite logu</h2>
        <ul className="mt-3 max-h-80 space-y-2 overflow-auto">
          {u.activity.map((a) => (
            <li key={a.id} className={`${adminCardPad} !py-3 text-sm`}>
              <span className="font-medium text-white">{a.type}</span>
              <span className="text-slate-400"> — {a.message}</span>
              <span className="mt-1 block text-xs text-slate-600">{new Date(a.createdTime).toLocaleString("tr")}</span>
            </li>
          ))}
        </ul>
      </section>

      <AdminPromptModal
        open={banModalOpen}
        title="Kullanıcıyı banla"
        label="Ban sebebi"
        placeholder="Örn: Sahte ilan, dolandırıcılık şüphesi..."
        confirmLabel="Banla"
        loading={banLoading}
        onCancel={() => !banLoading && setBanModalOpen(false)}
        onConfirm={(banReason) => void confirmBan(banReason)}
      />
    </div>
  );
}
