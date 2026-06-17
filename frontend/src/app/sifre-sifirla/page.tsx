"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useLocale } from "@/context/LocaleContext";
import { api, ApiError } from "@/lib/api";
import { alertErrorClass, formFieldClass } from "@/lib/formStyles";
import { btnBrand, pageContainerAuth, surfaceCard } from "@/lib/uiStyles";

function ResetForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { t } = useLocale();
  const [email, setEmail] = useState(searchParams.get("email") ?? "");
  const [token, setToken] = useState(searchParams.get("token") ?? "");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await api.resetPassword({ email: email.trim(), token, newPassword, confirmPassword });
      router.replace("/giris");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("auth.resetFailed"));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className={pageContainerAuth}>
      <div className={`${surfaceCard} p-8`}>
        <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">{t("auth.resetTitle")}</h1>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">{t("auth.resetSubtitle")}</p>
        <form onSubmit={handleSubmit} className="mt-6 space-y-4">
          {error && <div className={alertErrorClass}>{error}</div>}
          <input
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder={t("auth.email")}
            className={formFieldClass}
          />
          <input
            type="text"
            required
            value={token}
            onChange={(e) => setToken(e.target.value)}
            placeholder={t("auth.resetToken")}
            className={formFieldClass}
          />
          <input
            type="password"
            required
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            placeholder={t("account.newPassword")}
            className={formFieldClass}
          />
          <input
            type="password"
            required
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder={t("account.confirmPassword")}
            className={formFieldClass}
          />
          <button type="submit" disabled={loading} className={`${btnBrand} w-full py-3`}>
            {loading ? "…" : t("account.updatePassword")}
          </button>
        </form>
      </div>
    </div>
  );
}

export default function ResetPasswordPage() {
  const { t } = useLocale();
  return (
    <Suspense fallback={<LoadingSpinner label={t("common.loading")} />}>
      <ResetForm />
    </Suspense>
  );
}
