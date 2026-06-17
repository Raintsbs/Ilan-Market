"use client";

import dynamic from "next/dynamic";
import { useEffect, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useLocale } from "@/context/LocaleContext";
import { api, ApiError } from "@/lib/api";

const ForgotPasswordFlow = dynamic(
  () => import("@/components/ui/forgot-password-flow-1").then((m) => m.ForgotPasswordFlow),
  { ssr: false, loading: () => <LoadingSpinner /> },
);

export default function ForgotPasswordPage() {
  const { t } = useLocale();
  const [email, setEmail] = useState("");
  const [sent, setSent] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [emailUsesPickup, setEmailUsesPickup] = useState(false);

  useEffect(() => {
    api.getAuthPublicConfig().then((res) => {
      if (res.success && res.data?.emailUsesPickup) setEmailUsesPickup(true);
    });
  }, []);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await api.forgotPassword(email.trim());
      setSent(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("auth.resetFailed"));
    } finally {
      setLoading(false);
    }
  }

  return (
    <ForgotPasswordFlow
      email={email}
      error={error}
      loading={loading}
      sent={sent}
      devPickupHint={emailUsesPickup && sent}
      onEmailChange={setEmail}
      onSubmit={handleSubmit}
    />
  );
}
