"use client";

import { useState } from "react";
import { useLocale } from "@/context/LocaleContext";

type CopyButtonProps = {
  text: string;
  className?: string;
  onCopied?: () => void;
  onError?: () => void;
};

export function CopyButton({ text, className = "", onCopied, onError }: CopyButtonProps) {
  const { t } = useLocale();
  const [copied, setCopied] = useState(false);

  async function handleClick() {
    try {
      await navigator.clipboard.writeText(text);
      setCopied(true);
      onCopied?.();
      window.setTimeout(() => setCopied(false), 2200);
    } catch {
      onError?.();
    }
  }

  return (
    <button
      type="button"
      onClick={handleClick}
      className={`inline-flex items-center justify-center gap-2 rounded-xl border border-slate-200 px-3 py-2 text-sm font-medium transition-colors duration-200 dark:border-slate-600 dark:text-slate-300 ${className}`}
    >
      <span
        className={`inline-flex h-5 w-5 items-center justify-center transition-transform duration-300 ${
          copied ? "scale-110 text-emerald-600 dark:text-emerald-400" : ""
        }`}
      >
        {copied ? (
          <svg className="h-5 w-5 animate-copy-check" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
          </svg>
        ) : (
          <svg className="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"
            />
          </svg>
        )}
      </span>
      <span className={copied ? "text-emerald-700 dark:text-emerald-300" : ""}>
        {copied ? t("share.copiedShort") : t("share.copy")}
      </span>
    </button>
  );
}
