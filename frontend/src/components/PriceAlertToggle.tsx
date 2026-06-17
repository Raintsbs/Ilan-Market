"use client";

import { useState } from "react";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api, ApiError } from "@/lib/api";

type PriceAlertToggleProps = {
  advertisementId: number;
};

export function PriceAlertToggle({ advertisementId }: PriceAlertToggleProps) {
  const { t } = useLocale();
  const { showToast } = useToast();
  const [enabled, setEnabled] = useState(false);
  const [target, setTarget] = useState("");
  const [saving, setSaving] = useState(false);

  async function save() {
    setSaving(true);
    try {
      const alertPrice = target ? Number(target) : undefined;
      await api.setFavoritePriceAlert(advertisementId, enabled, alertPrice);
      showToast(t("priceAlert.saved"), "success");
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("priceAlert.failed"), "error");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="mt-3 rounded-xl border border-slate-200 p-3 text-sm dark:border-slate-700">
      <label className="flex items-center gap-2 font-medium text-slate-700 dark:text-slate-300">
        <input type="checkbox" checked={enabled} onChange={(e) => setEnabled(e.target.checked)} />
        {t("priceAlert.enable")}
      </label>
      {enabled && (
        <input
          type="number"
          min={0}
          value={target}
          onChange={(e) => setTarget(e.target.value)}
          placeholder={t("priceAlert.target")}
          className="mt-2 w-full rounded-lg border border-slate-200 px-3 py-1.5 dark:border-slate-600 dark:bg-slate-800"
        />
      )}
      <button
        type="button"
        onClick={save}
        disabled={saving}
        className="mt-2 text-xs font-semibold text-blue-600 hover:underline disabled:opacity-50"
      >
        {saving ? "…" : t("priceAlert.enable")}
      </button>
    </div>
  );
}
