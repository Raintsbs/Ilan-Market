"use client";

import { useMemo, useState } from "react";
import { useLocale } from "@/context/LocaleContext";
import type { Category } from "@/lib/types";
import { inputField, surfaceCard } from "@/lib/uiStyles";

export interface ParametricFilterValues {
  brand?: string;
  model?: string;
  minYear?: string;
  maxYear?: string;
  minMileage?: string;
  maxMileage?: string;
}

interface SchemaField {
  key: string;
  type?: string;
  label?: string;
}

function parseSchema(json?: string): SchemaField[] {
  if (!json?.trim()) return [];
  try {
    const parsed = JSON.parse(json) as { fields?: SchemaField[] } | SchemaField[];
    if (Array.isArray(parsed)) return parsed;
    return parsed.fields ?? [];
  } catch {
    return [];
  }
}

interface ParametricFiltersProps {
  categories: Category[];
  categoryId: string;
  values: ParametricFilterValues;
  onChange: (values: ParametricFilterValues) => void;
}

export function ParametricFilters({
  categories,
  categoryId,
  values,
  onChange,
}: ParametricFiltersProps) {
  const { t } = useLocale();
  const [open, setOpen] = useState(false);
  const category = categories.find((c) => String(c.id) === categoryId);
  const fields = useMemo(() => parseSchema(category?.fieldSchemaJson), [category]);

  if (!categoryId || fields.length === 0) return null;

  const set = (patch: Partial<ParametricFilterValues>) => onChange({ ...values, ...patch });

  return (
    <div className={`${surfaceCard} mt-3 overflow-hidden`}>
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        className="flex w-full items-center justify-between px-4 py-3 text-left text-sm font-semibold text-slate-800 dark:text-slate-100"
      >
        <span>
          {t("filters.parametric")} — {category?.name}
        </span>
        <svg
          className={`h-4 w-4 text-slate-400 transition-transform ${open ? "rotate-180" : ""}`}
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={2}
        >
          <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
        </svg>
      </button>
      {open && (
        <div className="grid gap-3 border-t border-slate-100 p-4 sm:grid-cols-2 lg:grid-cols-3 dark:border-slate-800">
          {fields.map((f) => {
            const key = f.key as keyof ParametricFilterValues;
            if (!["brand", "model", "minYear", "maxYear", "minMileage", "maxMileage"].includes(f.key)) {
              return null;
            }
            const label = f.label || f.key;
            return (
              <label key={f.key} className="block text-sm">
                <span className="mb-1.5 block text-xs font-medium text-slate-500">{label}</span>
                <input
                  type={f.type === "number" ? "number" : "text"}
                  value={values[key] ?? ""}
                  onChange={(e) => set({ [key]: e.target.value })}
                  className={inputField}
                />
              </label>
            );
          })}
        </div>
      )}
    </div>
  );
}
