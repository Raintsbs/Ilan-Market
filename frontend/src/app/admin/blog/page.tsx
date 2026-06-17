"use client";

import { useEffect, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { adminApi } from "@/lib/admin";
import {
  adminBadge,
  adminBtnPrimary,
  adminBtnSecondary,
  adminInputFull,
  adminListItem,
  adminModalOverlay,
  adminModalPanel,
  adminTextarea,
} from "@/lib/adminStyles";
import type { BlogPost } from "@/lib/types";

const empty: BlogPost = {
  id: 0,
  title: "",
  slug: "",
  summary: "",
  content: "",
  isPublished: false,
  createdTime: new Date().toISOString(),
};

export default function AdminBlogPage() {
  const [posts, setPosts] = useState<BlogPost[]>([]);
  const [form, setForm] = useState<BlogPost | null>(null);

  const load = () => adminApi.getBlog().then((r) => r.data && setPosts(r.data));

  useEffect(() => {
    load();
  }, []);

  const save = async () => {
    if (!form) return;
    await adminApi.saveBlog(form, form.id > 0 ? form.id : undefined);
    setForm(null);
    load();
  };

  return (
    <div>
      <AdminPageHeader
        title="Blog / Duyuru"
        subtitle="Site içi haberler ve duyurular"
        actions={
          <button type="button" className={adminBtnPrimary} onClick={() => setForm({ ...empty })}>
            + Yeni yazı
          </button>
        }
      />

      <ul className="space-y-2">
        {posts.map((p) => (
          <li key={p.id} className={adminListItem}>
            <div>
              <p className="font-medium text-white">{p.title}</p>
              <p className="mt-0.5">
                <span className={adminBadge(p.isPublished ? "green" : "amber")}>
                  {p.isPublished ? "Yayında" : "Taslak"}
                </span>
              </p>
            </div>
            <div className="flex gap-3 text-sm">
              <button type="button" className="font-medium text-blue-400 hover:text-blue-300" onClick={() => setForm(p)}>
                Düzenle
              </button>
              <button
                type="button"
                className="font-medium text-rose-400 hover:text-rose-300"
                onClick={async () => {
                  if (confirm("Silinsin mi?")) {
                    await adminApi.deleteBlog(p.id);
                    load();
                  }
                }}
              >
                Sil
              </button>
            </div>
          </li>
        ))}
      </ul>

      {form && (
        <div className={adminModalOverlay}>
          <div className={`${adminModalPanel} max-w-2xl`}>
            <h2 className="text-lg font-bold text-white">{form.id ? "Yazıyı düzenle" : "Yeni yazı"}</h2>
            <div className="mt-4 space-y-3">
              <input
                className={adminInputFull}
                placeholder="Başlık"
                value={form.title}
                onChange={(e) => setForm({ ...form, title: e.target.value })}
              />
              <input
                className={adminInputFull}
                placeholder="slug"
                value={form.slug}
                onChange={(e) => setForm({ ...form, slug: e.target.value })}
              />
              <textarea
                className={`${adminTextarea} h-40`}
                placeholder="İçerik"
                value={form.content}
                onChange={(e) => setForm({ ...form, content: e.target.value })}
              />
              <label className="flex items-center gap-2 text-sm text-slate-300">
                <input
                  type="checkbox"
                  checked={form.isPublished}
                  onChange={(e) => setForm({ ...form, isPublished: e.target.checked })}
                />
                Yayınla
              </label>
            </div>
            <div className="mt-6 flex gap-2">
              <button type="button" onClick={save} className={adminBtnPrimary}>
                Kaydet
              </button>
              <button type="button" onClick={() => setForm(null)} className={adminBtnSecondary}>
                İptal
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
