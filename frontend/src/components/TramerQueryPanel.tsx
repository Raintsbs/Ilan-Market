"use client";

import { useState } from "react";
import { api } from "@/lib/api";
import { useLocale } from "@/context/LocaleContext";
import type { TramerResult } from "@/lib/types";
import { formFieldClass as inputClass } from "@/lib/formStyles";
import { btnBrand, surfaceElevated } from "@/lib/uiStyles";

type TramerQueryPanelProps = {
  advertisementId?: number;
  initialResult?: TramerResult | null;
  onSaved?: (result: TramerResult) => void;
  readOnly?: boolean;
};

export function TramerQueryPanel({
  advertisementId,
  initialResult,
  onSaved,
  readOnly,
}: TramerQueryPanelProps) {
  const { t } = useLocale();
  const [plate, setPlate] = useState(initialResult?.plate ?? "");
  const [result, setResult] = useState<TramerResult | null>(initialResult ?? null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function runQuery(save: boolean) {
    setError("");
    setLoading(true);
    try {
      const res = save && advertisementId
        ? await api.saveTramerToAd(advertisementId, plate)
        : await api.queryTramer(plate);
      if (!res.success || !res.data) throw new Error(res.message);
      setResult(res.data);
      onSaved?.(res.data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Sorgu başarısız");
    } finally {
      setLoading(false);
    }
  }

  if (readOnly && result) {
    return (
      <div className={`p-4 ${surfaceElevated}`}>
        <h3 className="text-sm font-bold uppercase tracking-wide text-slate-500">{t("tramer.title")}</h3>
        <p className="mt-2 text-sm text-slate-700 dark:text-slate-300">
          <strong>{result.plate}</strong> — {result.status}
        </p>
        {result.summary && <p className="mt-1 text-sm text-slate-500">{result.summary}</p>}
        {result.isSimulated && (
          <p className="mt-2 text-xs text-amber-600">{t("tramer.simulated")}</p>
        )}
      </div>
    );
  }

  if (readOnly) return null;

  return (
    <div className={`p-4 ${surfaceElevated}`}>
      <h3 className="text-sm font-bold uppercase tracking-wide text-slate-500">{t("tramer.title")}</h3>
      <p className="mt-1 text-xs text-slate-500">{t("tramer.hint")}</p>
      <div className="mt-3 flex flex-wrap gap-2">
        <input
          value={plate}
          onChange={(e) => setPlate(e.target.value.toUpperCase())}
          placeholder="34 ABC 123"
          className={`${inputClass} min-w-[10rem] flex-1`}
        />
        <button type="button" disabled={loading || !plate.trim()} onClick={() => runQuery(false)} className={btnBrand}>
          {loading ? "..." : t("tramer.query")}
        </button>
        {advertisementId && (
          <button
            type="button"
            disabled={loading || !plate.trim()}
            onClick={() => runQuery(true)}
            className="rounded-xl border border-slate-200 px-4 py-2 text-sm font-semibold dark:border-slate-700"
          >
            {t("tramer.saveToAd")}
          </button>
        )}
      </div>
      {error && <p className="mt-2 text-sm text-rose-600">{error}</p>}
      {result && (
        <div className="mt-3 rounded-lg bg-slate-50 p-3 text-sm dark:bg-slate-800">
          <p className="font-medium">{result.status}</p>
          <p className="text-slate-600 dark:text-slate-400">{result.summary}</p>
          {result.isSimulated && <p className="mt-1 text-xs text-amber-600">{t("tramer.simulated")}</p>}
        </div>
      )}
    </div>
  );
}
