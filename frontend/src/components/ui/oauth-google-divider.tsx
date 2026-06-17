"use client";

import { GoogleSignInButton } from "@/components/GoogleSignInButton";
import { useLocale } from "@/context/LocaleContext";

type OAuthGoogleDividerProps = {
  disabled?: boolean;
  intent?: "signin" | "signup";
  onToken: (token: string) => void;
};

export function OAuthGoogleDivider({ disabled, intent = "signin", onToken }: OAuthGoogleDividerProps) {
  const { t } = useLocale();

  return (
    <div className="space-y-4">
      <GoogleSignInButton
        variant="auth-flow"
        intent={intent}
        disabled={disabled}
        onToken={onToken}
      />
      <div className="flex items-center gap-4">
        <div className="h-px flex-1 bg-white/10" />
        <span className="text-sm text-white/40">{t("auth.or")}</span>
        <div className="h-px flex-1 bg-white/10" />
      </div>
    </div>
  );
}
