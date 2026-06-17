"use client";

import { useEffect, useState } from "react";
import { adminBtnPrimary, adminBtnSecondary, adminInputFull } from "@/lib/adminStyles";

type AdminPromptModalProps = {
  open: boolean;
  title: string;
  label: string;
  placeholder?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  required?: boolean;
  multiline?: boolean;
  loading?: boolean;
  onConfirm: (value: string) => void;
  onCancel: () => void;
};

export function AdminPromptModal({
  open,
  title,
  label,
  placeholder,
  confirmLabel = "Onayla",
  cancelLabel = "İptal",
  required,
  multiline,
  loading,
  onConfirm,
  onCancel,
}: AdminPromptModalProps) {
  const [value, setValue] = useState("");

  useEffect(() => {
    if (open) setValue("");
  }, [open]);

  if (!open) return null;

  const canSubmit = !required || value.trim().length > 0;

  return (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-black/70 p-4 backdrop-blur-sm"
      role="dialog"
      aria-modal="true"
      aria-labelledby="admin-prompt-title"
      onClick={onCancel}
    >
      <div
        className="w-full max-w-md rounded-2xl border border-white/10 bg-[#121a2e] p-6 shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <h2 id="admin-prompt-title" className="text-lg font-bold text-white">
          {title}
        </h2>
        <label className="mt-4 block text-sm font-medium text-slate-300">{label}</label>
        {multiline ? (
          <textarea
            className={`${adminInputFull} mt-2 min-h-[6rem] resize-y`}
            placeholder={placeholder}
            value={value}
            onChange={(e) => setValue(e.target.value)}
            autoFocus
          />
        ) : (
          <input
            className={`${adminInputFull} mt-2`}
            placeholder={placeholder}
            value={value}
            onChange={(e) => setValue(e.target.value)}
            autoFocus
            onKeyDown={(e) => {
              if (e.key === "Enter" && canSubmit && !loading) onConfirm(value.trim());
            }}
          />
        )}
        <div className="mt-6 flex flex-wrap justify-end gap-2">
          <button type="button" disabled={loading} onClick={onCancel} className={adminBtnSecondary}>
            {cancelLabel}
          </button>
          <button
            type="button"
            disabled={loading || !canSubmit}
            onClick={() => onConfirm(value.trim())}
            className={adminBtnPrimary}
          >
            {loading ? "İşleniyor..." : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
