"use client";

import { useLocale } from "@/context/LocaleContext";
import { getStatusLabel } from "@/lib/status";
import type { AdvertisementStatus } from "@/lib/types";

export function StatusBadge({
  status,
  isActive,
}: {
  status: AdvertisementStatus;
  isActive: boolean;
}) {
  const { t } = useLocale();
  const { label, className } = getStatusLabel(status, isActive, t);
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold ring-1 ring-inset ${className}`}
    >
      {label}
    </span>
  );
}
