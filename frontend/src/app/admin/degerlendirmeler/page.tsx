"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { StarRating } from "@/components/StarRating";
import { adminApi } from "@/lib/admin";
import {
  adminBtnDanger,
  adminBtnWarning,
  adminCardPad,
  adminSelect,
} from "@/lib/adminStyles";
import { formatDate } from "@/lib/status";
import type { AdminReviewItem } from "@/lib/types";

const typeLabels: Record<string, string> = {
  seller: "Satıcı",
  advertisement: "İlan",
  buyer: "Alıcı",
};

export default function AdminReviewsPage() {
  const [items, setItems] = useState<AdminReviewItem[]>([]);
  const [type, setType] = useState("all");

  const load = () => {
    adminApi.getReviews(type === "all" ? undefined : type).then((r) => {
      if (r.success && r.data) setItems(r.data);
    });
  };

  useEffect(() => {
    load();
  }, [type]);

  const hide = async (item: AdminReviewItem) => {
    await adminApi.hideReview(item.reviewType, item.id, !item.isHidden);
    load();
  };

  const remove = async (item: AdminReviewItem) => {
    if (!confirm("Bu yorumu kalıcı olarak silmek istiyor musunuz?")) return;
    await adminApi.deleteReview(item.reviewType, item.id);
    load();
  };

  return (
    <div>
      <AdminPageHeader
        title="Değerlendirmeler"
        subtitle="Satıcı, ilan ve alıcı yorumlarını yönetin"
        actions={
          <select value={type} onChange={(e) => setType(e.target.value)} className={adminSelect}>
            <option value="all">Tümü</option>
            <option value="seller">Satıcı</option>
            <option value="advertisement">İlan</option>
            <option value="buyer">Alıcı</option>
          </select>
        }
      />

      <div className="space-y-4">
        {items.map((item) => (
          <div key={`${item.reviewType}-${item.id}`} className={adminCardPad}>
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                  {typeLabels[item.reviewType] ?? item.reviewType}
                  {item.isHidden && (
                    <span className="ml-2 rounded bg-rose-900/50 px-2 py-0.5 text-rose-300">Gizli</span>
                  )}
                </p>
                <p className="mt-1 text-sm text-slate-300">
                  <span className="text-white">{item.authorName}</span>
                  {" → "}
                  <span className="text-white">{item.targetName}</span>
                </p>
                {item.advertisementId && (
                  <Link
                    href={`/ilan/${item.advertisementId}`}
                    target="_blank"
                    className="mt-1 inline-block text-sm text-blue-400 hover:text-blue-300"
                  >
                    {item.advertisementTitle ?? `İlan #${item.advertisementId}`}
                  </Link>
                )}
              </div>
              <div className="text-right text-xs text-slate-500">
                {formatDate(item.createdTime, "tr")}
              </div>
            </div>
            <div className="mt-3">
              <StarRating rating={item.rating} size="sm" />
            </div>
            {item.comment && (
              <p className="mt-2 text-sm text-slate-300">{item.comment}</p>
            )}
            <div className="mt-4 flex flex-wrap gap-2">
              <button type="button" onClick={() => hide(item)} className={adminBtnWarning}>
                {item.isHidden ? "Göster" : "Gizle"}
              </button>
              <button type="button" onClick={() => remove(item)} className={adminBtnDanger}>
                Sil
              </button>
            </div>
          </div>
        ))}
        {items.length === 0 && (
          <p className="text-center text-slate-500">Yorum bulunamadı.</p>
        )}
      </div>
    </div>
  );
}
