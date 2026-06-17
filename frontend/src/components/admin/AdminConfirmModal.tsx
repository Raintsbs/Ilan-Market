"use client";

import { adminBtnDanger, adminBtnPrimary, adminBtnSecondary } from "@/lib/adminStyles";

type AdminConfirmModalProps = {
  open: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  danger?: boolean;
  loading?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
};

export function AdminConfirmModal({
  open,
  title,
  message,
  confirmLabel = "Onayla",
  cancelLabel = "İptal",
  danger,
  loading,
  onConfirm,
  onCancel,
}: AdminConfirmModalProps) {
  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-black/70 p-4 backdrop-blur-sm"
      role="dialog"
      aria-modal="true"
      aria-labelledby="admin-confirm-title"
      onClick={onCancel}
    >
      <div
        className="w-full max-w-md rounded-2xl border border-white/10 bg-[#121a2e] p-6 shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <h2 id="admin-confirm-title" className="text-lg font-bold text-white">
          {title}
        </h2>
        <p className="mt-3 text-sm leading-relaxed text-slate-300">{message}</p>
        <div className="mt-6 flex flex-wrap justify-end gap-2">
          <button type="button" disabled={loading} onClick={onCancel} className={adminBtnSecondary}>
            {cancelLabel}
          </button>
          <button
            type="button"
            disabled={loading}
            onClick={onConfirm}
            className={danger ? adminBtnDanger : adminBtnPrimary}
          >
            {loading ? "İşleniyor..." : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
