"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import {
  AlertTriangle,
  Clock,
  FileText,
  Sparkles,
  Users,
} from "lucide-react";
import { AdminCategoryChart } from "@/components/admin/AdminCategoryChart";
import { AdminLoading } from "@/components/admin/AdminLoading";
import { AdminPageHeader } from "@/components/admin/AdminPageHeader";
import { AdminStatCard } from "@/components/admin/AdminStatCard";
import { adminApi } from "@/lib/admin";
import { useAdsChangeListener } from "@/lib/adsSync";
import { adminBtnSecondary, adminCardPad } from "@/lib/adminStyles";
import type { AdminDashboard } from "@/lib/types";

export default function AdminDashboardPage() {
  const [data, setData] = useState<AdminDashboard | null>(null);

  const load = useCallback(() => {
    adminApi.dashboard().then((r) => {
      if (r.success && r.data) setData(r.data);
    });
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  useAdsChangeListener(load);

  if (!data) return <AdminLoading />;

  const chartData =
    data.categoryDistribution?.map((c) => ({
      categoryId: c.categoryId,
      categoryName: c.categoryName,
      count: c.count,
    })) ?? [];

  return (
    <div>
      <AdminPageHeader
        title="Dashboard"
        subtitle="Platform özeti ve hızlı erişim"
        actions={
          <Link href="/admin/ilanlar?status=0" className={adminBtnSecondary}>
            <Clock className="size-4" />
            Bekleyen ilanlar ({data.pendingAds})
          </Link>
        }
      />

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        <AdminStatCard
          label="Toplam ilan"
          value={data.totalAds}
          href="/admin/ilanlar"
          icon={FileText}
          accent="blue"
        />
        <AdminStatCard
          label="Onay bekleyen"
          value={data.pendingAds}
          href="/admin/ilanlar?status=0"
          icon={Clock}
          accent="amber"
        />
        <AdminStatCard
          label="Kullanıcı"
          value={data.totalUsers}
          href="/admin/kullanicilar"
          icon={Users}
          accent="violet"
        />
        <AdminStatCard
          label="Açık şikayet"
          value={data.openReports}
          href="/admin/sikayetler"
          icon={AlertTriangle}
          accent="rose"
        />
        <AdminStatCard
          label="Öne çıkan"
          value={data.featuredAds}
          href="/admin/ilanlar"
          icon={Sparkles}
          accent="emerald"
        />
        <AdminStatCard
          label="Süresi dolan"
          value={data.expiredAds}
          href="/admin/ilanlar?expired=1"
          icon={AlertTriangle}
          accent="slate"
        />
      </div>

      <section className={`mt-8 ${adminCardPad}`}>
        <h2 className="text-lg font-semibold text-white">Kategori dağılımı</h2>
        <p className="mt-1 text-sm text-slate-400">En çok ilan bulunan kategoriler</p>
        <div className="mt-6">
          <AdminCategoryChart data={chartData} />
        </div>
      </section>
    </div>
  );
}
