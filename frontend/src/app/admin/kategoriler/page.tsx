"use client";

import { useEffect, useMemo, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { adminApi, isAdmin } from "@/lib/admin";
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
import { useAdminAuth } from "@/context/AdminAuthContext";
import type { AdminCategory } from "@/lib/types";

type Tab = "root" | "sub";

const empty: AdminCategory = {
  id: 0,
  name: "",
  description: "",
  parentId: undefined,
  sortOrder: 0,
  fieldSchemaJson: "",
  isActive: true,
  adCount: 0,
};

function buildPath(items: AdminCategory[], cat: AdminCategory): string {
  const parts: string[] = [cat.name];
  let pid = cat.parentId;
  const byId = new Map(items.map((c) => [c.id, c]));
  while (pid != null) {
    const p = byId.get(pid);
    if (!p) break;
    parts.unshift(p.name);
    pid = p.parentId;
  }
  return parts.join(" › ");
}

export default function AdminCategoriesPage() {
  const { user } = useAdminAuth();
  const canEdit = isAdmin(user?.roles);
  const [items, setItems] = useState<AdminCategory[]>([]);
  const [form, setForm] = useState<AdminCategory | null>(null);
  const [tab, setTab] = useState<Tab>("root");
  const [parentFilter, setParentFilter] = useState<number | "all">("all");
  const [search, setSearch] = useState("");

  const load = () => adminApi.getCategories().then((r) => r.data && setItems(r.data));

  useEffect(() => {
    load();
  }, []);

  const roots = useMemo(
    () => items.filter((c) => c.parentId == null && c.isActive),
    [items],
  );
  const subs = useMemo(() => items.filter((c) => c.parentId != null), [items]);

  const parentOptions = useMemo(
    () => items.filter((c) => c.id !== form?.id),
    [items, form?.id],
  );

  const displayed = useMemo(() => {
    let list: AdminCategory[];
    if (tab === "root") list = roots;
    else if (parentFilter === "all") list = subs;
    else {
      const ids = new Set<number>();
      const collect = (id: number) => {
        ids.add(id);
        items.filter((c) => c.parentId === id).forEach((c) => collect(c.id));
      };
      collect(parentFilter);
      list = subs.filter((c) => ids.has(c.id) && c.id !== parentFilter);
    }

    const q = search.trim().toLowerCase();
    if (!q) return list;
    return list.filter(
      (c) =>
        c.name.toLowerCase().includes(q) ||
        buildPath(items, c).toLowerCase().includes(q),
    );
  }, [tab, roots, subs, parentFilter, items, search]);

  const openNew = (parentId?: number) => {
    setForm({ ...empty, parentId, sortOrder: 0 });
    if (parentId != null) setTab("sub");
  };

  const save = async () => {
    if (!form) return;
    const id = form.id > 0 ? form.id : undefined;
    await adminApi.saveCategory(form, id);
    setForm(null);
    load();
  };

  const tabBtn = (key: Tab, label: string, count: number) => (
    <button
      type="button"
      onClick={() => setTab(key)}
      className={`rounded-lg px-4 py-2 text-sm font-medium transition ${
        tab === key
          ? "bg-blue-600 text-white"
          : "bg-slate-800 text-slate-400 hover:bg-slate-700 hover:text-white"
      }`}
    >
      {label}
      <span className="ml-2 rounded-full bg-black/20 px-2 py-0.5 text-xs tabular-nums">{count}</span>
    </button>
  );

  return (
    <div>
      <AdminPageHeader
        title="Kategoriler"
        subtitle="Ana ve alt kategori ağacı, sıralama ve parametrik alan şemaları"
        actions={
          canEdit ? (
            <button
              type="button"
              className={adminBtnPrimary}
              onClick={() => openNew(tab === "sub" && parentFilter !== "all" ? parentFilter : undefined)}
            >
              + {tab === "root" ? "Ana kategori" : "Alt kategori"}
            </button>
          ) : undefined
        }
      />

      <div className="mb-6 flex flex-wrap gap-2">
        {tabBtn("root", "Ana Kategoriler", roots.length)}
        {tabBtn("sub", "Alt Kategoriler", subs.length)}
      </div>

      <div className="mb-4">
        <input
          className={`${adminInputFull} max-w-md`}
          placeholder={
            tab === "root"
              ? "Ana kategori ara…"
              : "Alt kategori ara (örn. telefon, apple, iphone)…"
          }
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {tab === "sub" && (
        <div className="mb-4 flex flex-wrap items-center gap-3">
          <label className="text-sm text-slate-400">Üst kategoriye göre filtre:</label>
          <select
            className={`${adminInputFull} max-w-xs`}
            value={parentFilter === "all" ? "all" : String(parentFilter)}
            onChange={(e) =>
              setParentFilter(e.target.value === "all" ? "all" : Number(e.target.value))
            }
          >
            <option value="all">Tüm alt kategoriler</option>
            {roots.map((r) => (
              <option key={r.id} value={r.id}>
                {r.name} altı
              </option>
            ))}
          </select>
          <p className="text-xs text-slate-500">
            Telefon, Televizyon vb. Elektronik altındadır — &quot;Elektronik altı&quot; filtresinde veya aramada
            &quot;telefon&quot; / &quot;apple&quot; yazın.
          </p>
        </div>
      )}

      <div className="space-y-2">
        {displayed.length === 0 && (
          <p className="rounded-xl border border-dashed border-slate-700 p-8 text-center text-sm text-slate-500">
            {tab === "root" ? "Ana kategori yok." : "Bu filtrede alt kategori yok."}
          </p>
        )}
        {displayed.map((c) => (
          <div key={c.id} className={adminListItem}>
            <div className="min-w-0 flex-1">
              <p className="font-medium text-white">{c.name}</p>
              {tab === "sub" && (
                <p className="mt-0.5 truncate text-xs text-slate-500">{buildPath(items, c)}</p>
              )}
              <p className="mt-0.5 flex flex-wrap items-center gap-2 text-xs text-slate-500">
                <span>ID: {c.id}</span>
                {c.slug && (
                  <>
                    <span>·</span>
                    <span>/{c.slug}</span>
                  </>
                )}
                <span>·</span>
                <span>Sıra: {c.sortOrder}</span>
                <span>·</span>
                <span>{c.adCount} ilan</span>
                {(c.childCount ?? 0) > 0 && (
                  <>
                    <span>·</span>
                    <span>{c.childCount} alt kategori</span>
                  </>
                )}
                <span className={adminBadge(c.isActive ? "green" : "slate")}>
                  {c.isActive ? "Aktif" : "Pasif"}
                </span>
              </p>
            </div>
            {canEdit && (
              <div className="flex shrink-0 flex-wrap gap-3">
                {tab === "root" && (
                  <button
                    type="button"
                    className="text-sm font-medium text-emerald-400 hover:text-emerald-300"
                    onClick={() => openNew(c.id)}
                  >
                    + Alt ekle
                  </button>
                )}
                {tab === "sub" && (c.childCount ?? 0) > 0 && (
                  <button
                    type="button"
                    className="text-sm font-medium text-emerald-400 hover:text-emerald-300"
                    onClick={() => openNew(c.id)}
                  >
                    + Alt ekle
                  </button>
                )}
                <button
                  type="button"
                  className="text-sm font-medium text-blue-400 hover:text-blue-300"
                  onClick={() => setForm(c)}
                >
                  Düzenle
                </button>
                <button
                  type="button"
                  className="text-sm font-medium text-rose-400 hover:text-rose-300"
                  onClick={async () => {
                    if (confirm(`"${c.name}" silinsin mi? Alt kategoriler de silinir.`)) {
                      await adminApi.deleteCategory(c.id);
                      load();
                    }
                  }}
                >
                  Sil
                </button>
              </div>
            )}
          </div>
        ))}
      </div>

      {form && (
        <div className={adminModalOverlay}>
          <div className={`${adminModalPanel} max-w-lg`}>
            <h2 className="text-lg font-bold text-white">
              {form.id ? "Kategori düzenle" : form.parentId ? "Yeni alt kategori" : "Yeni ana kategori"}
            </h2>
            <div className="mt-4 space-y-3">
              <input
                className={adminInputFull}
                placeholder="Ad"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
              />
              <input
                className={adminInputFull}
                placeholder="Açıklama"
                value={form.description ?? ""}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
              />
              <div>
                <label className="mb-1 block text-xs text-slate-400">Üst kategori</label>
                <select
                  className={adminInputFull}
                  value={form.parentId ?? ""}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      parentId: e.target.value ? Number(e.target.value) : undefined,
                    })
                  }
                >
                  <option value="">— Ana kategori (kök) —</option>
                  {parentOptions.map((p) => (
                    <option key={p.id} value={p.id}>
                      {buildPath(items, p)}
                    </option>
                  ))}
                </select>
              </div>
              <input
                className={adminInputFull}
                placeholder="Slug (boş bırakılırsa otomatik)"
                value={form.slug ?? ""}
                onChange={(e) => setForm({ ...form, slug: e.target.value })}
              />
              <input
                type="number"
                className={adminInputFull}
                placeholder="Sıra"
                value={form.sortOrder}
                onChange={(e) => setForm({ ...form, sortOrder: Number(e.target.value) })}
              />
              <textarea
                className={`${adminTextarea} h-24`}
                placeholder='Form alanları JSON örn: {"fields":[{"key":"mileage","label":"KM"}]}'
                value={form.fieldSchemaJson ?? ""}
                onChange={(e) => setForm({ ...form, fieldSchemaJson: e.target.value })}
              />
              <label className="flex items-center gap-2 text-sm text-slate-300">
                <input
                  type="checkbox"
                  checked={form.isActive}
                  onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
                />
                Aktif
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
