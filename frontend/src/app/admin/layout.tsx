import { AdminLayoutGate } from "@/components/admin/AdminLayoutGate";
import { AdminAuthProvider } from "@/context/AdminAuthContext";

export const metadata = {
  title: "Yönetim Paneli",
  robots: { index: false, follow: false },
};

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <AdminAuthProvider>
      <AdminLayoutGate>{children}</AdminLayoutGate>
    </AdminAuthProvider>
  );
}
