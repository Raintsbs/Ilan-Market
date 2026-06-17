"use client";

import dynamic from "next/dynamic";
import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useCallback, useEffect, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { TurnstileWidget } from "@/components/TurnstileWidget";
import { ApiError, api } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";

const SignInFlow = dynamic(
  () => import("@/components/ui/sign-in-flow-1").then((m) => m.SignInFlow),
  { ssr: false, loading: () => <LoadingSpinner /> },
);

function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { t } = useLocale();
  const { login, isAuthenticated, isLoading } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [successReveal, setSuccessReveal] = useState(false);
  const [captchaSiteKey, setCaptchaSiteKey] = useState("");
  const [captchaEnabled, setCaptchaEnabled] = useState(false);
  const [captchaToken, setCaptchaToken] = useState<string | null>(null);

  const rawRedirect = searchParams.get("redirect");
  const redirectTo =
    rawRedirect?.startsWith("/") && !rawRedirect.startsWith("/admin")
      ? rawRedirect
      : "/";

  useEffect(() => {
    api.getAuthPublicConfig().then((res) => {
      if (res.success && res.data) {
        setCaptchaSiteKey(res.data.captchaSiteKey ?? "");
        setCaptchaEnabled(!!res.data.captchaEnabled);
      }
    });
  }, []);

  useEffect(() => {
    const blocked = sessionStorage.getItem("auth_block_message");
    if (blocked) {
      setError(blocked);
      sessionStorage.removeItem("auth_block_message");
    }
  }, []);

  useEffect(() => {
    if (!isLoading && isAuthenticated && !successReveal) {
      router.replace(redirectTo);
    }
  }, [isLoading, isAuthenticated, router, redirectTo, successReveal]);

  const finishLogin = useCallback(
    async (action: () => Promise<void>) => {
      setError("");
      if (captchaEnabled && !captchaToken) {
        setError("Lütfen güvenlik doğrulamasını tamamlayın.");
        return;
      }
      setLoading(true);
      try {
        await action();
        setSuccessReveal(true);
        window.setTimeout(() => router.replace(redirectTo), 1600);
      } catch (err) {
        setError(err instanceof ApiError ? err.message : t("auth.loginFailed"));
        setLoading(false);
        setCaptchaToken(null);
      }
    },
    [router, redirectTo, t, captchaEnabled, captchaToken],
  );

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    await finishLogin(() => login({ email, password, captchaToken: captchaToken ?? undefined }));
  }

  if (isLoading) return <LoadingSpinner label={t("auth.checkingSession")} />;

  return (
    <SignInFlow
      email={email}
      password={password}
      error={error}
      loading={loading}
      successReveal={successReveal}
      onEmailChange={setEmail}
      onPasswordChange={setPassword}
      onSubmit={handleSubmit}
      captchaSlot={
        captchaEnabled && captchaSiteKey ? (
          <TurnstileWidget siteKey={captchaSiteKey} onToken={setCaptchaToken} />
        ) : null
      }
    />
  );
}

export default function LoginPage() {
  const { t } = useLocale();
  return (
    <Suspense fallback={<LoadingSpinner label={t("common.loading")} />}>
      <LoginForm />
    </Suspense>
  );
}
