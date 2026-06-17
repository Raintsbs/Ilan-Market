"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { adminApi } from "@/lib/admin";
import {
  adminBadge,
  adminBtnPrimary,
  adminInput,
  adminSelect,
  adminTable,
  adminTableWrap,
  adminTd,
  adminTh,
  adminTr,
} from "@/lib/adminStyles";
import type { AdminUser } from "@/lib/types";

export default function AdminUsersPage() {
  const [search, setSearch] = useState("");
  const [role, setRole] = useState("");
  const [users, setUsers] = useState<AdminUser[]>([]);

  const load = () => {
    adminApi.getUsers(search || undefined, role || undefined).then((r) => {
      if (r.success && r.data) setUsers(r.data);
    });
  };

  useEffect(() => {
    load();
  }, []);

  return (
    <div>
      <AdminPageHeader
        title="Kullanıcı yönetimi"
        subtitle="Üye hesapları, roller ve moderasyon durumu"
      />

      <div className="mb-6 flex flex-wrap gap-2">
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="E-posta veya isim..."
          className={adminInput}
        />
        <select value={role} onChange={(e) => setRole(e.target.value)} className={adminSelect}>
          <option value="">Tüm roller</option>
          <option value="Admin">Admin</option>
          <option value="Moderator">Moderatör</option>
          <option value="User">Üye</option>
        </select>
        <button type="button" onClick={load} className={adminBtnPrimary}>
          Ara
        </button>
      </div>

      <div className={adminTableWrap}>
        <table className={adminTable}>
          <thead>
            <tr>
              <th className={adminTh}>Kullanıcı</th>
              <th className={adminTh}>Roller</th>
              <th className={adminTh}>İlan</th>
              <th className={adminTh}>Durum</th>
              <th className={adminTh} />
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id} className={adminTr}>
                <td className={adminTd}>
                  <p className="font-medium text-white">
                    {u.firstName} {u.lastName}
                  </p>
                  <p className="text-xs text-slate-500">{u.email}</p>
                </td>
                <td className={adminTd}>
                  <span className={adminBadge("blue")}>{u.roles.join(", ")}</span>
                </td>
                <td className={`${adminTd} tabular-nums`}>{u.adCount}</td>
                <td className={adminTd}>
                  <div className="flex flex-wrap gap-1">
                    {u.isBanned && <span className={adminBadge("rose")}>Ban</span>}
                    {u.isFrozen && <span className={adminBadge("amber")}>Dondurulmuş</span>}
                    {u.isVerified && <span className={adminBadge("green")}>Doğrulanmış</span>}
                    {u.phoneVerified && <span className={adminBadge("blue")}>Tel ✓</span>}
                    {u.warningCount > 0 && (
                      <span className={adminBadge("amber")}>Uyarı {u.warningCount}</span>
                    )}
                  </div>
                </td>
                <td className={adminTd}>
                  <Link
                    href={`/admin/kullanicilar/${u.id}`}
                    className="text-sm font-medium text-blue-400 hover:text-blue-300"
                  >
                    Detay →
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
