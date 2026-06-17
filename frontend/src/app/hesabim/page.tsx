"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { AccountSettings } from "@/components/AccountSettings";
import { VerificationPanel } from "@/components/VerificationPanel";
import { ReferralPanel } from "@/components/ReferralPanel";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { SafeImage } from "@/components/SafeImage";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { useCountUp } from "@/hooks/useCountUp";
import { useMountReveal } from "@/hooks/useMountReveal";
import { api } from "@/lib/api";
import { PageHeader } from "@/components/PageHeader";
import { getImageUrl } from "@/lib/image";
import { btnBrand, btnOutline, pageContainerSm, surfaceCard } from "@/lib/uiStyles";

function StatCard({ label, value, delay }: { label: string; value: number; delay: string }) {
  const reveal = useMountReveal();
  const display = useCountUp(value, reveal);

  return (
    <div
      className={`${surfaceCard} p-4 text-center ${
        reveal ? "home-hero-line" : "home-hero-pending"
      }`}
      style={reveal ? ({ "--hero-delay": delay } as React.CSSProperties) : undefined}
    >
      <p className="text-2xl font-bold tabular-nums text-blue-600 dark:text-blue-400">
        {display.toLocaleString("tr-TR")}
      </p>
      <p className="mt-1 text-xs font-medium text-slate-500 dark:text-slate-400">{label}</p>
    </div>
  );
}

export default function AccountPage() {
  const router = useRouter();
  const { t } = useLocale();
  const { user, isAuthenticated, isLoading, logout } = useAuth();
  const heroReveal = useMountReveal();
  const [progressActive, setProgressActive] = useState(false);
  const [adCount, setAdCount] = useState(0);
  const [totalViews, setTotalViews] = useState(0);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace("/giris?redirect=/hesabim");
    }
  }, [isLoading, isAuthenticated, router]);

  useEffect(() => {
    if (!isAuthenticated) return;
    api.getMyAdvertisements({ page: 1, pageSize: 100 }).then((res) => {
      if (res.success && res.data) {
        setAdCount(res.data.totalCount);
        setTotalViews(
          res.data.items.reduce((sum, ad) => sum + (ad.viewCount ?? 0), 0),
        );
      }
    });
  }, [isAuthenticated]);

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      const id = requestAnimationFrame(() => setProgressActive(true));
      return () => cancelAnimationFrame(id);
    }
    setProgressActive(false);
  }, [isLoading, isAuthenticated]);

  const profilePercent = useMemo(() => {
    if (!user) return 0;
    let score = 0;
    if (user.firstName?.trim()) score += 25;
    if (user.lastName?.trim()) score += 25;
    if (user.email?.trim()) score += 25;
    if (user.profileImagePath) score += 25;
    return score;
  }, [user]);

  if (isLoading) return <LoadingSpinner />;
  if (!isAuthenticated || !user) return null;

  const displayUser = user;
  const shortcuts = [
    { href: "/ilanlarim", label: t("nav.myAds"), desc: t("account.myAdsDesc") },
    { href: "/favorilerim", label: t("nav.favorites"), desc: t("account.favoritesDesc") },
    { href: "/takip-ettiklerim", label: t("nav.following"), desc: t("follow.desc") },
    { href: "/hesabim/magaza", label: t("store.settings"), desc: t("store.settingsDesc") },
    { href: "/hesabim/kazanc", label: t("earnings.title"), desc: t("earnings.desc") },
    { href: "/hesabim/toplu-yukle", label: t("bulk.title"), desc: t("bulk.desc") },
    { href: "/kategoriler", label: t("nav.categories"), desc: t("account.categoriesDesc") },
  ];

  const avatarUrl = displayUser.profileImagePath
    ? getImageUrl(displayUser.profileImagePath)
    : null;

  return (
    <div className={pageContainerSm}>
      <PageHeader title={t("account.title")} subtitle={t("account.subtitle")} />

      <div
        className={`mt-8 overflow-hidden ${surfaceCard} ${
          heroReveal ? "animate-slide-down-in" : "opacity-0"
        }`}
      >
        <div className="bg-gradient-to-br from-blue-600 to-blue-600 px-6 py-8 text-white">
          <Link
            href="/hesabim#profile-photo"
            className="group relative inline-block h-16 w-16 overflow-hidden rounded-2xl bg-white/20 backdrop-blur"
          >
            {avatarUrl ? (
              <SafeImage src={avatarUrl} alt="" fill className="object-cover" />
            ) : (
              <div className="flex h-full w-full items-center justify-center text-2xl font-bold">
                {(displayUser.firstName?.charAt(0) || displayUser.email.charAt(0) || "?").toUpperCase()}
                {displayUser.lastName?.charAt(0)?.toUpperCase() ?? ""}
              </div>
            )}
            <span className="avatar-edit-overlay absolute inset-0 flex items-center justify-center bg-black/45 text-xs font-semibold">
              {t("account.editAvatar")}
            </span>
          </Link>
          <h2 className="mt-4 text-xl font-semibold">
            {displayUser.firstName || displayUser.lastName
              ? `${displayUser.firstName} ${displayUser.lastName}`.trim()
              : displayUser.email || t("account.title")}
          </h2>
          <p className="mt-1 text-blue-100">{displayUser.email}</p>
          {displayUser.roles.length > 0 && (
            <div className="mt-3 flex flex-wrap gap-2">
              {displayUser.roles.map((role) => (
                <span
                  key={role}
                  className="rounded-full bg-white/20 px-2.5 py-0.5 text-xs font-medium"
                >
                  {role}
                </span>
              ))}
            </div>
          )}

          <div className="mt-6">
            <div className="flex items-center justify-between text-xs text-blue-100">
              <span>{t("account.profileComplete")}</span>
              <span className="font-semibold tabular-nums">{profilePercent}%</span>
            </div>
            <div className="mt-2 h-2 overflow-hidden rounded-full bg-white/20">
              <div
                className="profile-progress-fill h-full rounded-full bg-white"
                style={{ width: progressActive ? `${profilePercent}%` : "0%" }}
              />
            </div>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3 border-b border-slate-100 p-4 dark:border-slate-800">
          <StatCard label={t("account.statAds")} value={adCount} delay="0ms" />
          <StatCard label={t("account.statViews")} value={totalViews} delay="100ms" />
        </div>

        <div className="p-6">
          <Link
            href="/ilan/yeni"
            className={`${btnBrand} btn-ripple w-full py-4 text-base`}
          >
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            {t("nav.newAd")}
          </Link>
        </div>
      </div>

      <div className="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {shortcuts.map((item, index) => (
          <Link
            key={item.href}
            href={item.href}
            className={`${surfaceCard} p-4 transition hover:border-blue-200 hover:shadow-md ${
              heroReveal ? "home-hero-line" : "home-hero-pending"
            }`}
            style={
              heroReveal
                ? ({ "--hero-delay": `${200 + index * 70}ms` } as React.CSSProperties)
                : undefined
            }
          >
            <p className="font-semibold text-slate-900 dark:text-white">{item.label}</p>
            <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">{item.desc}</p>
          </Link>
        ))}
      </div>

      <AccountSettings />
      <VerificationPanel />
      <ReferralPanel />

      <button
        type="button"
        onClick={logout}
        className={`${btnOutline} mt-8 w-full hover:border-rose-200 hover:bg-rose-50 hover:text-rose-700 dark:hover:border-rose-800 dark:hover:bg-rose-950 dark:hover:text-rose-400`}
      >
        {t("nav.logout")}
      </button>
    </div>
  );
}
