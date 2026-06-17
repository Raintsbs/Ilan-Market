"use client";

import { usePathname } from "next/navigation";
import { CompareBar } from "@/components/CompareBar";
import { CookieConsent } from "@/components/CookieConsent";
import { Footer } from "@/components/Footer";
import { Header } from "@/components/Header";
import { InstallPwaBanner } from "@/components/InstallPwaBanner";

export function SiteChrome({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const isAdminArea = pathname?.startsWith("/admin");
  const isAuthFlow =
    pathname === "/giris" ||
    pathname === "/kayit" ||
    pathname === "/sifremi-unuttum" ||
    pathname === "/sifre-sifirla";

  if (isAdminArea || isAuthFlow) {
    return <>{children}</>;
  }

  return (
    <>
      <InstallPwaBanner />
      <Header />
      <main className="flex-1">{children}</main>
      <CompareBar />
      <Footer />
      <CookieConsent />
    </>
  );
}
