"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { btnBrandSm, surfaceElevated } from "@/lib/uiStyles";

export type SavedSearchFilter = {
  search?: string;
  categoryId?: string;
  minPrice?: string;
  maxPrice?: string;
  city?: string;
  featuredOnly?: boolean;
  brand?: string;
  model?: string;
  minYear?: string;
  maxYear?: string;
  minMileage?: string;
  maxMileage?: string;
};

type SavedSearchItem = {
  id: number;
  name: string;
  filterJson: string;
  notifyOnNew: boolean;
};

type SavedSearchPanelProps = {
  currentFilter: SavedSearchFilter;
  onApply: (filter: SavedSearchFilter) => void;
};

export function SavedSearchPanel({ currentFilter, onApply }: SavedSearchPanelProps) {
  const { t } = useLocale();
  const { isAuthenticated } = useAuth();
  const [items, setItems] = useState<SavedSearchItem[]>([]);
  const [name, setName] = useState("");
  const [notify, setNotify] = useState(false);
  const [loading, setLoading] = useState(false);

  function load() {
    if (!isAuthenticated) return;
    api.getSavedSearches().then((res) => {
      if (res.success && res.data) setItems(res.data);
    });
  }

  useEffect(() => {
    load();
  }, [isAuthenticated]);

  if (!isAuthenticated) return null;

  async function save() {
    if (!name.trim()) return;
    setLoading(true);
    try {
      const res = await api.createSavedSearch({
        name: name.trim(),
        filterJson: JSON.stringify(currentFilter),
        notifyOnNew: notify,
      });
      if (res.success) {
        setName("");
        load();
      }
    } finally {
      setLoading(false);
    }
  }

  async function remove(id: number) {
    await api.deleteSavedSearch(id);
    load();
  }

  return (
    <section className={`p-3 sm:p-4 ${surfaceElevated}`}>
      <h2 className="text-xs font-bold uppercase tracking-wide text-slate-500 sm:text-sm">{t("savedSearch.title")}</h2>
      <div className="mt-3 flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-center sm:gap-2">
        <input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder={t("savedSearch.namePlaceholder")}
          className="w-full min-w-0 flex-1 rounded-xl border border-slate-200 px-3 py-2.5 text-sm dark:border-slate-700 dark:bg-slate-900 sm:min-w-[10rem] sm:py-2"
        />
        <label className="flex items-center gap-2 text-xs text-slate-600 dark:text-slate-400">
          <input type="checkbox" checked={notify} onChange={(e) => setNotify(e.target.checked)} />
          {t("savedSearch.notify")}
        </label>
        <button
          type="button"
          disabled={loading || !name.trim()}
          onClick={save}
          className={`${btnBrandSm} w-full sm:w-auto`}
        >
          {t("savedSearch.save")}
        </button>
      </div>
      {items.length > 0 && (
        <ul className="mt-4 space-y-2">
          {items.map((item) => {
            let filter: SavedSearchFilter = {};
            try {
              filter = JSON.parse(item.filterJson) as SavedSearchFilter;
            } catch {
              /* ignore */
            }
            return (
              <li
                key={item.id}
                className="flex flex-col gap-2 rounded-lg border border-slate-100 px-3 py-2.5 text-sm sm:flex-row sm:items-center sm:justify-between dark:border-slate-800"
              >
                <button
                  type="button"
                  onClick={() => onApply(filter)}
                  className="font-medium text-blue-600 hover:underline dark:text-blue-400"
                >
                  {item.name}
                  {item.notifyOnNew && <span className="ml-2 text-xs text-amber-600">🔔</span>}
                </button>
                <button type="button" onClick={() => remove(item.id)} className="text-xs text-rose-600">
                  {t("savedSearch.delete")}
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </section>
  );
}
