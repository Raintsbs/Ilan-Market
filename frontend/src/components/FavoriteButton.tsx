"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";

interface FavoriteButtonProps {
  advertisementId: number;
  variant?: "default" | "icon";
}

function FavoriteHeartIcon({
  isFavorite,
  sizeClass,
  bouncing,
  onAnimationEnd,
}: {
  isFavorite: boolean;
  sizeClass: string;
  bouncing: boolean;
  onAnimationEnd: () => void;
}) {
  return (
    <span className="inline-flex transform-gpu">
      <svg
        className={`${sizeClass} transition-colors duration-200 ${
          isFavorite ? "fill-current" : "fill-none"
        } ${bouncing ? "animate-favorite-bounce" : ""}`}
        viewBox="0 0 24 24"
        stroke="currentColor"
        strokeWidth={2}
        onAnimationEnd={onAnimationEnd}
        aria-hidden="true"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"
        />
      </svg>
    </span>
  );
}

export function FavoriteButton({ advertisementId, variant = "default" }: FavoriteButtonProps) {
  const { isAuthenticated, isLoading: authLoading } = useAuth();
  const { t } = useLocale();
  const [isFavorite, setIsFavorite] = useState(false);
  const [loading, setLoading] = useState(false);
  const [bouncing, setBouncing] = useState(false);

  useEffect(() => {
    if (!isAuthenticated || authLoading) return;
    api
      .getFavoriteStatus(advertisementId)
      .then((res) => {
        if (res.success && res.data !== undefined) setIsFavorite(res.data);
      })
      .catch(() => {});
  }, [advertisementId, isAuthenticated, authLoading]);

  function triggerBounce() {
    setBouncing(false);
    requestAnimationFrame(() => setBouncing(true));
  }

  async function handleToggle(e: React.MouseEvent) {
    e.preventDefault();
    e.stopPropagation();
    if (!isAuthenticated) {
      window.location.href = `/giris?redirect=/ilan/${advertisementId}`;
      return;
    }

    triggerBounce();
    setLoading(true);
    try {
      const res = await api.toggleFavorite(advertisementId);
      if (res.success && res.data !== undefined) setIsFavorite(res.data);
    } catch {
      /* ignore */
    } finally {
      setLoading(false);
    }
  }

  const label = isFavorite ? t("favorite.remove") : t("favorite.add");

  if (variant === "icon") {
    return (
      <button
        type="button"
        onClick={handleToggle}
        disabled={loading || authLoading}
        title={label}
        aria-label={label}
        className={`flex h-11 w-11 items-center justify-center rounded-full shadow-lg transition-colors duration-200 active:scale-95 disabled:opacity-60 ${
          isFavorite
            ? "bg-rose-500 text-white hover:bg-rose-600"
            : "bg-white/95 text-slate-600 hover:bg-white hover:text-rose-500 dark:bg-slate-800/95 dark:text-slate-200"
        }`}
      >
        <FavoriteHeartIcon
          isFavorite={isFavorite}
          sizeClass="h-6 w-6"
          bouncing={bouncing}
          onAnimationEnd={() => setBouncing(false)}
        />
      </button>
    );
  }

  return (
    <button
      type="button"
      onClick={handleToggle}
      disabled={loading || authLoading}
      className={`inline-flex items-center gap-2 rounded-xl border px-4 py-2.5 text-sm font-semibold transition-colors duration-200 active:scale-[0.98] disabled:opacity-60 ${
        isFavorite
          ? "border-rose-200 bg-rose-50 text-rose-700 dark:border-rose-800 dark:bg-rose-950 dark:text-rose-300"
          : "border-slate-200 bg-white text-slate-700 hover:border-rose-200 hover:text-rose-600 dark:border-slate-600 dark:bg-slate-900 dark:text-slate-200"
      }`}
    >
      <FavoriteHeartIcon
        isFavorite={isFavorite}
        sizeClass={`h-5 w-5 ${isFavorite ? "text-rose-500" : ""}`}
        bouncing={bouncing}
        onAnimationEnd={() => setBouncing(false)}
      />
      {loading ? "..." : label}
    </button>
  );
}
