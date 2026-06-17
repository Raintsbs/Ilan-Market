"use client";

import dynamic from "next/dynamic";
import { useRouter, useSearchParams } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { TurnstileWidget } from "@/components/TurnstileWidget";
import { ApiError, api } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { useLocale } from "@/context/LocaleContext";

const SignUpFlow = dynamic(
  () => import("@/components/ui/sign-up-flow-1").then((m) => m.SignUpFlow),
  { ssr: false, loading: () => <LoadingSpinner /> },
);

export default function RegisterForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const referralCode = searchParams.get("ref") ?? undefined;
  const { t } = useLocale();
  const { register } = useAuth();
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [successReveal, setSuccessReveal] = useState(false);
  const [captchaSiteKey, setCaptchaSiteKey] = useState("");
  const [captchaEnabled, setCaptchaEnabled] = useState(false);
  const [captchaToken, setCaptchaToken] = useState<string | null>(null);

  useEffect(() => {
    api.getAuthPublicConfig().then((res) => {
      if (res.success && res.data) {
        setCaptchaSiteKey(res.data.captchaSiteKey ?? "");
        setCaptchaEnabled(!!res.data.captchaEnabled);
      }
    });
  }, []);

  const finishAuth = useCallback(
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
        window.setTimeout(() => router.replace("/"), 1600);
      } catch (err) {
        setError(err instanceof ApiError ? err.message : t("auth.registerFailed"));
        setLoading(false);
        setCaptchaToken(null);
      }
    },
    [router, t, captchaEnabled, captchaToken],
  );

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    await finishAuth(() =>
      register({
        firstName,
        lastName,
        email,
        password,
        captchaToken: captchaToken ?? undefined,
        referralCode,
      }),
    );
  }

  return (
    <SignUpFlow
      firstName={firstName}
      lastName={lastName}
      email={email}
      password={password}
      error={error}
      loading={loading}
      successReveal={successReveal}
      onFirstNameChange={setFirstName}
      onLastNameChange={setLastName}
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
