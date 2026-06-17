"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { Lock, Shield } from "lucide-react";
import { ApiError } from "@/lib/api";
import { useAdminAuth } from "@/context/AdminAuthContext";
import { AdminLoading } from "@/components/admin/AdminLoading";
import { adminBtnPrimary, adminInput } from "@/lib/adminStyles";

export default function AdminLoginPage() {
  const router = useRouter();
  const { login, isAuthenticated, isLoading } = useAdminAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      router.replace("/admin");
    }
  }, [isLoading, isAuthenticated, router]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await login({ email, password });
      router.replace("/admin");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Giriş başarısız.");
    } finally {
      setLoading(false);
    }
  }

  if (isLoading) {
    return (
      <div className="admin-shell flex min-h-screen items-center justify-center">
        <AdminLoading />
      </div>
    );
  }

  return (
    <div className="admin-shell flex min-h-screen items-center justify-center px-4 py-12">
      <div className="w-full max-w-md">
        <div className="mb-8 text-center">
          <span className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-500 to-blue-700 text-xl font-bold text-white shadow-xl shadow-blue-600/30">
            İ
          </span>
          <h1 className="mt-5 text-2xl font-bold text-white">Yönetim paneli</h1>
          <p className="mt-2 text-sm text-slate-400">Yetkili personel girişi</p>
        </div>

        <div className="rounded-2xl border border-white/[0.08] bg-white/[0.03] p-8 shadow-2xl shadow-black/30 backdrop-blur-sm">
          <div className="mb-6 flex items-start gap-3 rounded-xl border border-blue-500/20 bg-blue-500/10 p-4">
            <Shield className="mt-0.5 size-5 shrink-0 text-blue-400" />
            <p className="text-sm leading-relaxed text-slate-300">
              Bu alan site üyelerinden ayrıdır. Yalnızca Admin ve Moderatör hesapları erişebilir.
            </p>
          </div>

          {error && (
            <div className="mb-4 rounded-xl border border-rose-500/30 bg-rose-500/10 px-4 py-3 text-sm text-rose-200">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="admin-email" className="mb-1.5 block text-sm font-medium text-slate-300">
                E-posta
              </label>
              <input
                id="admin-email"
                type="email"
                autoComplete="username"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className={`${adminInput} w-full`}
              />
            </div>
            <div>
              <label htmlFor="admin-password" className="mb-1.5 block text-sm font-medium text-slate-300">
                Şifre
              </label>
              <input
                id="admin-password"
                type="password"
                autoComplete="current-password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className={`${adminInput} w-full`}
              />
            </div>
            <button type="submit" disabled={loading} className={`${adminBtnPrimary} w-full`}>
              <Lock className="size-4" />
              {loading ? "Giriş yapılıyor..." : "Panele gir"}
            </button>
          </form>
        </div>

        <p className="mt-6 text-center text-xs text-slate-600">
          Panel adresini paylaşmayın · Üye girişi için ana siteyi kullanın
        </p>
      </div>
    </div>
  );
}
