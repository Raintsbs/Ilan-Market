"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { ThemeLanguageControls } from "@/components/ThemeLanguageControls";
import { NotificationBell } from "@/components/NotificationBell";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";
import {
  desktopPrimaryNavLinks,
  desktopSecondaryNavLinks,
  getVisibleNavLinks,
} from "@/lib/nav";
import { btnBrandSm, btnOutline, siteShell } from "@/lib/uiStyles";

function isActive(pathname: string, href: string): boolean {
  if (href === "/") return pathname === "/";
  return pathname === href || pathname.startsWith(`${href}/`);
}

function LogoMark() {
  return (
    <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-slate-900 text-sm font-bold text-white shadow-sm dark:bg-blue-600">
      İ
    </span>
  );
}

export function Header() {
  const pathname = usePathname();
  const { user, isAuthenticated, isLoading, logout } = useAuth();
  const { t } = useLocale();
  const [menuOpen, setMenuOpen] = useState(false);
  const primaryLinks = getVisibleNavLinks(isAuthenticated, desktopPrimaryNavLinks);
  const secondaryLinks = getVisibleNavLinks(isAuthenticated, desktopSecondaryNavLinks);
  const navLinks = getVisibleNavLinks(isAuthenticated);

  const navLinkClass = (href: string) => {
    const active = isActive(pathname, href);
    return `relative whitespace-nowrap rounded-lg px-3 py-2 text-sm font-medium transition ${
      active
        ? "bg-slate-100 text-slate-900 dark:bg-slate-800 dark:text-white"
        : "text-slate-700 hover:bg-slate-50 hover:text-slate-950 dark:text-slate-400 dark:hover:bg-slate-800/60 dark:hover:text-white"
    }`;
  };

  return (
    <header className="site-header sticky top-0 z-50 border-b border-slate-200/80 shadow-sm shadow-slate-900/5 dark:border-slate-800 dark:shadow-black/20">
      <div className={`site-shell flex h-14 items-center gap-2 sm:gap-3 ${siteShell}`}>
        <Link href="/" className="flex shrink-0 items-center gap-2.5 pr-1">
          <LogoMark />
          <span className="text-base font-bold tracking-tight text-slate-900 dark:text-white sm:text-lg">
            İlan<span className="text-blue-600 dark:text-blue-400">Market</span>
          </span>
        </Link>

        <nav className="hidden min-w-0 flex-1 items-center justify-center gap-0.5 xl:flex">
          {primaryLinks.map((link) => (
            <Link key={link.href} href={link.href} className={`${navLinkClass(link.href)} shrink-0`}>
              {t(link.labelKey)}
            </Link>
          ))}
          {secondaryLinks.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className={`${navLinkClass(link.href)} hidden shrink-0 2xl:inline-flex`}
            >
              {t(link.labelKey)}
            </Link>
          ))}
        </nav>

        <div className="ml-auto flex shrink-0 items-center gap-2">
          <ThemeLanguageControls compact />
          {isAuthenticated && <NotificationBell />}

          {isLoading ? (
            <div className="h-9 w-16 animate-pulse rounded-xl bg-slate-100 dark:bg-slate-800" />
          ) : isAuthenticated ? (
            <>
              <Link href="/ilan/yeni" title={t("nav.newAd")} className={btnBrandSm}>
                <span aria-hidden>+</span>
                <span className="hidden sm:inline">{t("nav.newAdShort")}</span>
              </Link>
              <Link
                href="/hesabim"
                className="hidden max-w-[6rem] truncate text-sm font-medium text-slate-600 hover:text-slate-900 dark:text-slate-300 dark:hover:text-white md:inline lg:max-w-[8rem]"
              >
                {user?.firstName}
              </Link>
              <button type="button" onClick={logout} className={`${btnOutline} hidden h-9 px-3 md:inline-flex`}>
                {t("nav.logout")}
              </button>
            </>
          ) : (
            <>
              <Link
                href="/giris"
                className="hidden h-9 items-center whitespace-nowrap px-2 text-sm font-medium text-slate-600 hover:text-slate-900 dark:text-slate-300 sm:inline-flex"
              >
                {t("nav.login")}
              </Link>
              <Link href="/kayit" className={btnBrandSm}>
                {t("nav.register")}
              </Link>
            </>
          )}

          <button
            type="button"
            aria-label={t("nav.menu")}
            aria-expanded={menuOpen}
            onClick={() => setMenuOpen((o) => !o)}
            className="inline-flex h-10 w-10 shrink-0 items-center justify-center rounded-xl text-slate-600 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800 xl:hidden"
          >
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              {menuOpen ? (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              ) : (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
              )}
            </svg>
          </button>
        </div>
      </div>

      {menuOpen && (
        <>
          <button
            type="button"
            aria-label={t("nav.menu")}
            className="fixed inset-0 z-40 bg-black/40 backdrop-blur-[2px] xl:hidden"
            onClick={() => setMenuOpen(false)}
          />
          <nav className="fixed inset-x-0 top-14 z-50 max-h-[calc(100dvh-3.5rem)] overflow-y-auto border-t border-slate-100 bg-white/98 px-4 py-3 backdrop-blur-md dark:border-slate-800 dark:bg-slate-900/98 xl:hidden">
          <div className="flex flex-col gap-0.5">
            {navLinks.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                onClick={() => setMenuOpen(false)}
                className={`rounded-xl px-3 py-2.5 text-sm ${
                  isActive(pathname, link.href)
                    ? "bg-slate-100 font-semibold text-slate-900 dark:bg-slate-800 dark:text-white"
                    : "text-slate-600 dark:text-slate-300"
                }`}
              >
                {t(link.labelKey)}
              </Link>
            ))}
            {isAuthenticated ? (
              <>
                <Link
                  href="/hesabim"
                  onClick={() => setMenuOpen(false)}
                  className="rounded-xl px-3 py-2.5 text-sm text-slate-600 dark:text-slate-300"
                >
                  {user?.firstName} — {t("nav.account")}
                </Link>
                <Link
                  href="/ilan/yeni"
                  onClick={() => setMenuOpen(false)}
                  className={`${btnBrandSm} mt-2 w-full`}
                >
                  + {t("nav.newAd")}
                </Link>
                <button
                  type="button"
                  onClick={() => {
                    setMenuOpen(false);
                    logout();
                  }}
                  className={`${btnOutline} mt-2 w-full`}
                >
                  {t("nav.logout")}
                </button>
              </>
            ) : (
              <>
                <Link
                  href="/giris"
                  onClick={() => setMenuOpen(false)}
                  className={`${btnOutline} mt-2 w-full`}
                >
                  {t("nav.login")}
                </Link>
                <Link
                  href="/kayit"
                  onClick={() => setMenuOpen(false)}
                  className={`${btnBrandSm} mt-2 w-full`}
                >
                  {t("nav.register")}
                </Link>
              </>
            )}
          </div>
        </nav>
        </>
      )}
    </header>
  );
}
