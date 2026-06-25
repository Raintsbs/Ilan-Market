"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Home, LayoutGrid, LogIn, MessageSquare, PlusCircle, User, UserPlus } from "lucide-react";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";

import type { MessageKey } from "@/lib/i18n/messages";

type NavItem = {
  href: string;
  labelKey: MessageKey;
  icon: typeof Home;
  auth?: boolean;
  guestOnly?: boolean;
};

const items: NavItem[] = [
  { href: "/", labelKey: "nav.ads", icon: Home },
  { href: "/kategoriler", labelKey: "nav.categories", icon: LayoutGrid },
  { href: "/ilan/yeni", labelKey: "nav.newAdShort", icon: PlusCircle, auth: true },
  { href: "/mesajlar", labelKey: "nav.messages", icon: MessageSquare, auth: true },
  { href: "/hesabim", labelKey: "nav.account", icon: User, auth: true },
  { href: "/giris", labelKey: "nav.login", icon: LogIn, guestOnly: true },
  { href: "/kayit", labelKey: "nav.register", icon: UserPlus, guestOnly: true },
];

function isActive(pathname: string, href: string) {
  if (href === "/") return pathname === "/";
  return pathname === href || pathname.startsWith(`${href}/`);
}

export function MobileBottomNav() {
  const pathname = usePathname();
  const { isAuthenticated } = useAuth();
  const { t } = useLocale();

  const visible = items.filter((item) => {
    if (item.auth && !isAuthenticated) return false;
    if (item.guestOnly && isAuthenticated) return false;
    return true;
  });

  return (
    <nav
      className="mobile-bottom-nav fixed inset-x-0 bottom-0 z-50 border-t border-slate-200/90 bg-white/95 backdrop-blur-md dark:border-slate-800 dark:bg-slate-950/95 lg:hidden"
      aria-label={t("nav.menu")}
    >
      <ul className="mx-auto flex max-w-lg items-stretch justify-around gap-0.5 px-1 pb-[max(0.5rem,env(safe-area-inset-bottom))] pt-1 sm:gap-1 sm:px-2">
        {visible.map((item) => {
          const active = isActive(pathname, item.href);
          const Icon = item.icon;
          return (
            <li key={item.href} className="min-w-0 flex-1">
              <Link
                href={item.href}
                className={`flex flex-col items-center gap-0.5 rounded-xl px-0.5 py-2 text-[0.6rem] font-medium transition sm:px-1 sm:text-[0.65rem] ${
                  active
                    ? "text-blue-600 dark:text-blue-400"
                    : "text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-slate-200"
                }`}
              >
                <Icon className={`size-5 shrink-0 ${active ? "stroke-[2.5px]" : ""}`} aria-hidden />
                <span className="max-w-full truncate">{t(item.labelKey)}</span>
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}
