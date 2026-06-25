"use client";

import { useEffect, useState } from "react";
import { useLocale } from "@/context/LocaleContext";
import { useToast } from "@/context/ToastContext";
import { api, ApiError } from "@/lib/api";
import { formFieldClass } from "@/lib/formStyles";
import { surfaceCardPad } from "@/lib/uiStyles";
import type { VerificationRequest } from "@/lib/types";

export function VerificationPanel() {
  const { t } = useLocale();
  const { showToast } = useToast();
  const [status, setStatus] = useState<VerificationRequest | null | undefined>(undefined);
  const [docType, setDocType] = useState("identity");
  const [file, setFile] = useState<File | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    api.getMyVerification().then((r) => {
      if (r.success) setStatus(r.data ?? null);
    });
  }, []);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!file) return;
    setSubmitting(true);
    try {
      const res = await api.submitVerification(docType, file);
      if (res.success && res.data) {
        setStatus(res.data);
        showToast(t("verify.submitted"), "success");
        setFile(null);
      } else showToast(res.message || t("verify.failed"), "error");
    } catch (e) {
      showToast(e instanceof ApiError ? e.message : t("verify.failed"), "error");
    } finally {
      setSubmitting(false);
    }
  }

  if (status === undefined) return null;

  return (
    <section className={`mt-8 ${surfaceCardPad}`}>
      <h2 className="text-lg font-semibold text-slate-900 dark:text-white">{t("verify.title")}</h2>
      <p className="mt-1 text-sm text-slate-500">{t("verify.subtitle")}</p>

      {status && (
        <div className="mt-4 rounded-xl border border-slate-200 p-4 text-sm dark:border-slate-700">
          <p>
            <span className="font-medium">{t("verify.status")}:</span>{" "}
            {status.status === "pending" && t("verify.pending")}
            {status.status === "approved" && t("verify.approved")}
            {status.status === "rejected" && t("verify.rejected")}
          </p>
          {status.adminNote && <p className="mt-2 text-slate-600 dark:text-slate-400">{status.adminNote}</p>}
        </div>
      )}

      {(!status || status.status === "rejected") && (
        <form onSubmit={handleSubmit} className="mt-4 space-y-4">
          <div>
            <label className="text-sm font-medium text-slate-700 dark:text-slate-300">{t("verify.docType")}</label>
            <select value={docType} onChange={(e) => setDocType(e.target.value)} className={`mt-1 ${formFieldClass}`}>
              <option value="identity">{t("verify.identity")}</option>
              <option value="tax">{t("verify.tax")}</option>
              <option value="trade">{t("verify.trade")}</option>
            </select>
          </div>
          <div>
            <label className="text-sm font-medium text-slate-700 dark:text-slate-300">{t("verify.document")}</label>
            <input
              type="file"
              accept="image/*,.pdf"
              onChange={(e) => setFile(e.target.files?.[0] ?? null)}
              className="mt-1 block w-full text-sm"
            />
          </div>
          <button
            type="submit"
            disabled={submitting || !file}
            className="rounded-xl bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white disabled:opacity-60"
          >
            {submitting ? "…" : t("verify.submit")}
          </button>
        </form>
      )}
    </section>
  );
}
