"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import {
  ChevronRight,
  ExternalLink,
  FileStack,
  FileText,
  Flag,
  FolderTree,
  LayoutDashboard,
  LogOut,
  MapPin,
  Shield,
  Ticket,
  Upload,
  MessageSquare,
  Menu,
  Newspaper,
  Package,
  Sparkles,
  Users,
  X,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { useAdminAuth } from "@/context/AdminAuthContext";
import { isStaff } from "@/lib/admin";
import { AdminLoading } from "@/components/admin/AdminLoading";

type NavItem = { href: string; label: string; icon: LucideIcon };

const links: NavItem[] = [
  { href: "/admin", label: "Dashboard", icon: LayoutDashboard },
  { href: "/admin/ilanlar", label: "İlanlar", icon: FileText },
  { href: "/admin/sikayetler", label: "Şikayetler", icon: Flag },
  { href: "/admin/degerlendirmeler", label: "Değerlendirmeler", icon: MessageSquare },
  { href: "/admin/dogrulama", label: "Doğrulama", icon: Shield },
  { href: "/admin/kuponlar", label: "Kuponlar", icon: Ticket },
  { href: "/admin/ilan-import", label: "CSV Import", icon: Upload },
  { href: "/admin/siparisler", label: "Siparişler", icon: Package },
  { href: "/admin/paketler", label: "Paketler", icon: Sparkles },
  { href: "/admin/kullanicilar", label: "Kullanıcılar", icon: Users },
  { href: "/admin/kategoriler", label: "Kategoriler", icon: FolderTree },
  { href: "/admin/sehirler", label: "Şehirler", icon: MapPin },
  { href: "/admin/sayfalar", label: "Sayfalar", icon: FileStack },
  { href: "/admin/blog", label: "Blog / Duyuru", icon: Newspaper },
];

const pageTitles: Record<string, string> = {
  "/admin": "Dashboard",
  "/admin/ilanlar": "İlanlar",
  "/admin/sikayetler": "Şikayetler",
  "/admin/degerlendirmeler": "Değerlendirmeler",
  "/admin/dogrulama": "Doğrulama",
  "/admin/kuponlar": "Kuponlar",
  "/admin/ilan-import": "CSV Import",
  "/admin/siparisler": "Siparişler",
  "/admin/paketler": "Paketler",
  "/admin/kullanicilar": "Kullanıcılar",
  "/admin/kategoriler": "Kategoriler",
  "/admin/sehirler": "Şehirler",
  "/admin/sayfalar": "Sayfalar",
  "/admin/blog": "Blog",
};

function isActive(pathname: string, href: string) {
  if (href === "/admin") return pathname === "/admin";
  return pathname === href || pathname.startsWith(`${href}/`);
}

function resolveTitle(pathname: string) {
  if (pageTitles[pathname]) return pageTitles[pathname];
  const match = Object.entries(pageTitles)
    .filter(([k]) => k !== "/admin")
    .find(([k]) => pathname.startsWith(k));
  return match?.[1] ?? "Yönetim";
}

export function AdminShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const { user, isLoading, isAuthenticated, logout } = useAdminAuth();
  const [mobileOpen, setMobileOpen] = useState(false);

  useEffect(() => {
    if (isLoading) return;
    if (!isAuthenticated || !isStaff(user?.roles)) {
      router.replace("/admin/giris");
    }
  }, [isLoading, isAuthenticated, user, router]);

  useEffect(() => {
    setMobileOpen(false);
  }, [pathname]);

  if (isLoading || !isAuthenticated || !isStaff(user?.roles)) {
    return (
      <div className="admin-shell flex min-h-screen items-center justify-center">
        <AdminLoading />
      </div>
    );
  }

  const initials = `${user?.firstName?.[0] ?? ""}${user?.lastName?.[0] ?? ""}`.toUpperCase() || "A";

  const sidebar = (
    <div className="flex h-full flex-col">
      <div className="border-b border-white/[0.06] px-5 py-5">
        <Link href="/admin" className="flex items-center gap-3">
          <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-br from-blue-500 to-blue-700 text-sm font-bold text-white shadow-lg shadow-blue-600/30">
            İ
          </span>
          <div>
            <p className="text-sm font-bold text-white">İlanMarket</p>
            <p className="text-xs text-slate-500">Yönetim paneli</p>
          </div>
        </Link>
      </div>

      <nav className="flex-1 space-y-1 overflow-y-auto px-3 py-4">
        {links.map((l) => {
          const active = isActive(pathname, l.href);
          const Icon = l.icon;
          return (
            <Link
              key={l.href}
              href={l.href}
              className={`flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition ${
                active
                  ? "bg-blue-600/15 text-blue-300 ring-1 ring-blue-500/25"
                  : "text-slate-400 hover:bg-white/[0.04] hover:text-slate-200"
              }`}
            >
              <Icon className={`size-[18px] shrink-0 ${active ? "text-blue-400" : ""}`} strokeWidth={2} />
              {l.label}
            </Link>
          );
        })}
      </nav>

      <div className="border-t border-white/[0.06] p-4">
        <div className="mb-3 flex items-center gap-3 rounded-xl bg-white/[0.03] p-3">
          <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-white/10 text-xs font-bold text-white">
            {initials}
          </span>
          <div className="min-w-0 flex-1">
            <p className="truncate text-sm font-medium text-white">
              {user?.firstName} {user?.lastName}
            </p>
            <p className="truncate text-xs text-slate-500">{user?.email}</p>
          </div>
        </div>
        <button
          type="button"
          onClick={() => {
            logout();
            router.replace("/admin/giris");
          }}
          className="flex w-full items-center gap-2 rounded-xl px-3 py-2 text-sm text-slate-400 transition hover:bg-white/[0.04] hover:text-white"
        >
          <LogOut className="size-4" />
          Panelden çık
        </button>
        <Link
          href="/"
          className="mt-1 flex items-center gap-2 rounded-xl px-3 py-2 text-sm text-blue-400 transition hover:bg-blue-500/10"
        >
          <ExternalLink className="size-4" />
          Ana siteye git
        </Link>
      </div>
    </div>
  );

  return (
    <div className="admin-shell flex min-h-screen text-slate-100">
      {/* Desktop sidebar */}
      <aside className="hidden w-[260px] shrink-0 border-r border-white/[0.06] bg-[#0c1222]/80 backdrop-blur-xl lg:flex lg:flex-col">
        {sidebar}
      </aside>

      {/* Mobile sidebar */}
      {mobileOpen && (
        <div className="fixed inset-0 z-50 lg:hidden">
          <button
            type="button"
            aria-label="Menüyü kapat"
            className="absolute inset-0 bg-black/60 backdrop-blur-sm"
            onClick={() => setMobileOpen(false)}
          />
          <aside className="absolute inset-y-0 left-0 w-[min(100%,280px)] border-r border-white/[0.08] bg-[#0c1222] shadow-2xl">
            <button
              type="button"
              onClick={() => setMobileOpen(false)}
              className="absolute right-3 top-4 rounded-lg p-2 text-slate-400 hover:bg-white/10"
            >
              <X className="size-5" />
            </button>
            {sidebar}
          </aside>
        </div>
      )}

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-40 flex h-14 items-center gap-3 border-b border-white/[0.06] bg-[#0c1222]/80 px-4 backdrop-blur-xl sm:px-6">
          <button
            type="button"
            onClick={() => setMobileOpen(true)}
            className="rounded-lg p-2 text-slate-400 hover:bg-white/[0.06] lg:hidden"
            aria-label="Menü"
          >
            <Menu className="size-5" />
          </button>
          <div className="flex min-w-0 items-center gap-2 text-sm text-slate-500">
            <span className="hidden sm:inline">Panel</span>
            <ChevronRight className="hidden size-4 sm:inline" />
            <span className="truncate font-medium text-slate-200">{resolveTitle(pathname)}</span>
          </div>
        </header>

        <main className="flex-1 overflow-auto p-4 sm:p-6 lg:p-8">{children}</main>
      </div>
    </div>
  );
}
