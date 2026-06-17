"use client";

import { useEffect, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { adminApi, isAdmin } from "@/lib/admin";
import {
  adminBtnPrimary,
  adminBtnSecondary,
  adminCard,
  adminInputFull,
  adminModalOverlay,
  adminModalPanel,
  adminTextarea,
} from "@/lib/adminStyles";
import { useAdminAuth } from "@/context/AdminAuthContext";
import type { StaticPage } from "@/lib/types";

export default function AdminPagesPage() {
  const { user } = useAdminAuth();
  const canEdit = isAdmin(user?.roles);
  const [pages, setPages] = useState<StaticPage[]>([]);
  const [edit, setEdit] = useState<StaticPage | null>(null);

  const load = () => adminApi.getPages().then((r) => r.data && setPages(r.data));

  useEffect(() => {
    load();
  }, []);

  return (
    <div>
      <AdminPageHeader title="Statik sayfalar" subtitle="Hakkımızda, gizlilik ve diğer sabit içerikler" />

      <div className="space-y-2">
        {pages.map((p) => (
          <button
            key={p.id}
            type="button"
            onClick={() => canEdit && setEdit(p)}
            className={`block w-full ${adminCard} px-4 py-3 text-left transition hover:border-blue-500/30 hover:bg-white/[0.05] ${!canEdit ? "cursor-default" : ""}`}
          >
            <p className="font-medium text-white">{p.title}</p>
            <p className="mt-0.5 text-xs text-slate-500">/{p.slug}</p>
          </button>
        ))}
        {!pages.length && <p className="py-12 text-center text-sm text-slate-500">Henüz sayfa yok.</p>}
      </div>

      {edit && canEdit && (
        <div className={adminModalOverlay}>
          <div className={`${adminModalPanel} max-w-2xl`}>
            <h2 className="text-lg font-bold text-white">{edit.title}</h2>
            <input
              className={`${adminInputFull} mt-4`}
              value={edit.title}
              onChange={(e) => setEdit({ ...edit, title: e.target.value })}
            />
            <textarea
              className={`${adminTextarea} mt-3 h-64`}
              value={edit.content}
              onChange={(e) => setEdit({ ...edit, content: e.target.value })}
            />
            <div className="mt-6 flex gap-2">
              <button
                type="button"
                className={adminBtnPrimary}
                onClick={async () => {
                  await adminApi.savePage(edit);
                  setEdit(null);
                  load();
                }}
              >
                Kaydet
              </button>
              <button type="button" className={adminBtnSecondary} onClick={() => setEdit(null)}>
                Kapat
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
