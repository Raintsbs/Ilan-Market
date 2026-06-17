"use client";

import { usePathname } from "next/navigation";
import { AdminShell } from "@/components/admin/AdminShell";

const PUBLIC_ADMIN_PATHS = ["/admin/giris"];

export function AdminLayoutGate({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const isLoginPage = PUBLIC_ADMIN_PATHS.some((p) => pathname === p);

  if (isLoginPage) {
    return <>{children}</>;
  }

  return <AdminShell>{children}</AdminShell>;
}
