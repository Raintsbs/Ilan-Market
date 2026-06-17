"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { api } from "@/lib/api";
import { useLocale } from "@/context/LocaleContext";
import { inputField } from "@/lib/uiStyles";
import type { CategoryTreeNode } from "@/lib/types";

type CategoryTreePickerProps = {
  value: string;
  onChange: (categoryId: string) => void;
  required?: boolean;
  compact?: boolean;
  className?: string;
};

function findPath(nodes: CategoryTreeNode[], targetId: number, trail: CategoryTreeNode[] = []): CategoryTreeNode[] | null {
  for (const node of nodes) {
    const next = [...trail, node];
    if (node.id === targetId) return next;
    if (node.children?.length) {
      const found = findPath(node.children, targetId, next);
      if (found) return found;
    }
  }
  return null;
}

function labelForPath(path: CategoryTreeNode[]): string {
  return path.map((p) => p.name).join(" › ");
}

export function CategoryTreePicker({
  value,
  onChange,
  required,
  compact,
  className = "",
}: CategoryTreePickerProps) {
  const { t } = useLocale();
  const [tree, setTree] = useState<CategoryTreeNode[]>([]);
  const [loading, setLoading] = useState(true);
  const [open, setOpen] = useState(false);
  const [hoverL1, setHoverL1] = useState<number | null>(null);
  const [hoverL2, setHoverL2] = useState<number | null>(null);
  const [hoverL3, setHoverL3] = useState<number | null>(null);

  useEffect(() => {
    api.getCategoryTree().then((res) => {
      if (res.success && res.data) setTree(res.data);
      setLoading(false);
    });
  }, []);

  const selectedPath = useMemo(() => {
    const id = Number(value);
    if (!id || !tree.length) return null;
    return findPath(tree, id);
  }, [value, tree]);

  const level1 = tree;
  const level2 = useMemo(() => {
    const id = hoverL1 ?? selectedPath?.[0]?.id;
    return level1.find((c) => c.id === id)?.children ?? [];
  }, [level1, hoverL1, selectedPath]);

  const level3 = useMemo(() => {
    const id = hoverL2 ?? selectedPath?.[1]?.id;
    return level2.find((c) => c.id === id)?.children ?? [];
  }, [level2, hoverL2, selectedPath]);

  const level4 = useMemo(() => {
    const id = hoverL3 ?? selectedPath?.[2]?.id;
    return level3.find((c) => c.id === id)?.children ?? [];
  }, [level3, hoverL3, selectedPath]);

  const pick = useCallback(
    (node: CategoryTreeNode) => {
      onChange(String(node.id));
      setOpen(false);
    },
    [onChange],
  );

  const displayLabel = selectedPath?.length
    ? labelForPath(selectedPath)
    : t("home.allCategories");

  if (loading) {
    return (
      <div className={`text-sm text-slate-500 ${className}`}>{t("common.loading")}</div>
    );
  }

  const columns = (
    <div className="grid grid-cols-1 gap-0 overflow-hidden rounded-xl border border-slate-200 bg-white sm:grid-cols-2 lg:grid-cols-4 dark:border-slate-700 dark:bg-slate-900">
      <ul className="max-h-56 overflow-y-auto border-b border-slate-100 sm:border-b-0 sm:border-r dark:border-slate-800">
        {level1.map((c) => (
          <li key={c.id}>
            <button
              type="button"
              onMouseEnter={() => {
                setHoverL1(c.id);
                setHoverL2(null);
                setHoverL3(null);
              }}
              onClick={() => (c.children?.length ? setHoverL1(c.id) : pick(c))}
              className={`flex w-full items-center justify-between px-3 py-2 text-left text-sm hover:bg-slate-50 dark:hover:bg-slate-800 ${
                selectedPath?.[0]?.id === c.id ? "bg-blue-50 font-medium text-blue-700 dark:bg-blue-950 dark:text-blue-300" : ""
              }`}
            >
              {c.name}
              {c.children?.length ? <span className="text-slate-400">›</span> : null}
            </button>
          </li>
        ))}
      </ul>
      <ul className="max-h-56 overflow-y-auto border-b border-slate-100 sm:border-b-0 sm:border-r dark:border-slate-800">
        {level2.length === 0 && (
          <li className="px-3 py-4 text-xs text-slate-400">{t("form.categoryPick")}</li>
        )}
        {level2.map((c) => (
          <li key={c.id}>
            <button
              type="button"
              onMouseEnter={() => {
                setHoverL2(c.id);
                setHoverL3(null);
              }}
              onClick={() => (c.children?.length ? setHoverL2(c.id) : pick(c))}
              className={`flex w-full items-center justify-between px-3 py-2 text-left text-sm hover:bg-slate-50 dark:hover:bg-slate-800 ${
                selectedPath?.[1]?.id === c.id ? "bg-blue-50 font-medium text-blue-700 dark:bg-blue-950 dark:text-blue-300" : ""
              }`}
            >
              {c.name}
              {c.children?.length ? <span className="text-slate-400">›</span> : null}
            </button>
          </li>
        ))}
      </ul>
      <ul className="max-h-56 overflow-y-auto border-b border-slate-100 lg:border-b-0 lg:border-r dark:border-slate-800">
        {level3.length === 0 && level2.length > 0 && (
          <li className="px-3 py-2">
            <button
              type="button"
              onClick={() => {
                const parent = level2.find((c) => c.id === (hoverL2 ?? selectedPath?.[1]?.id));
                if (parent) pick(parent);
              }}
              className="text-sm font-medium text-blue-600 hover:underline"
            >
              {t("category.selectParent")}
            </button>
          </li>
        )}
        {level3.map((c) => (
          <li key={c.id}>
            <button
              type="button"
              onMouseEnter={() => setHoverL3(c.id)}
              onClick={() => (c.children?.length ? setHoverL3(c.id) : pick(c))}
              className={`flex w-full items-center justify-between px-3 py-2 text-left text-sm hover:bg-slate-50 dark:hover:bg-slate-800 ${
                selectedPath?.[2]?.id === c.id ? "bg-blue-50 font-medium text-blue-700 dark:bg-blue-950 dark:text-blue-300" : ""
              }`}
            >
              {c.name}
              {c.children?.length ? <span className="text-slate-400">›</span> : null}
            </button>
          </li>
        ))}
      </ul>
      <ul className="max-h-56 overflow-y-auto">
        {level4.length === 0 && level3.length > 0 && (
          <li className="px-3 py-2">
            <button
              type="button"
              onClick={() => {
                const parent = level3.find((c) => c.id === (hoverL3 ?? selectedPath?.[2]?.id));
                if (parent) pick(parent);
              }}
              className="text-sm font-medium text-blue-600 hover:underline"
            >
              {t("category.selectParent")}
            </button>
          </li>
        )}
        {level4.map((c) => (
          <li key={c.id}>
            <button
              type="button"
              onClick={() => pick(c)}
              className={`w-full px-3 py-2 text-left text-sm hover:bg-slate-50 dark:hover:bg-slate-800 ${
                selectedPath?.[3]?.id === c.id ? "bg-blue-50 font-medium text-blue-700 dark:bg-blue-950 dark:text-blue-300" : ""
              }`}
            >
              {c.name}
            </button>
          </li>
        ))}
      </ul>
    </div>
  );

  if (compact) {
    return (
      <div className={`relative ${className}`}>
        <button
          type="button"
          onClick={() => setOpen((o) => !o)}
          className={`${inputField} flex w-full items-center justify-between text-left lg:w-56`}
        >
          <span className="truncate">{displayLabel}</span>
          <span className="text-slate-400">▾</span>
        </button>
        {open && (
          <>
            <button type="button" className="fixed inset-0 z-40" aria-label="close" onClick={() => setOpen(false)} />
            <div className="absolute left-0 z-50 mt-1 w-[min(100vw-2rem,42rem)] shadow-xl">{columns}</div>
          </>
        )}
        {value && (
          <button
            type="button"
            onClick={() => onChange("")}
            className="mt-1 text-xs text-slate-500 hover:text-slate-800"
          >
            {t("home.allCategories")}
          </button>
        )}
      </div>
    );
  }

  return (
    <div className={className}>
      {required && !value && (
        <input tabIndex={-1} required value="" onChange={() => {}} className="sr-only" aria-hidden />
      )}
      {selectedPath && (
        <p className="mb-2 text-sm text-slate-600 dark:text-slate-400">
          {t("form.category")}: <strong>{displayLabel}</strong>
        </p>
      )}
      {columns}
    </div>
  );
}
