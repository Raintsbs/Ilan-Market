"use client";

import { useEffect, useRef, useState } from "react";
import { useLocale } from "@/context/LocaleContext";
import { api } from "@/lib/api";
import { cn } from "@/lib/utils";

type GoogleSignInButtonProps = {
  onToken: (idToken: string) => void;
  disabled?: boolean;
  variant?: "default" | "auth-flow";
  intent?: "signin" | "signup";
};

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: {
            client_id: string;
            callback: (response: { credential: string }) => void;
          }) => void;
          renderButton: (
            el: HTMLElement,
            config: {
              theme?: string;
              size?: string;
              shape?: string;
              text?: string;
              width?: number;
              locale?: string;
            },
          ) => void;
        };
      };
    };
  }
}

export function GoogleSignInButton({
  onToken,
  disabled,
  variant = "default",
  intent = "signin",
}: GoogleSignInButtonProps) {
  const { t } = useLocale();
  const ref = useRef<HTMLDivElement>(null);
  const onTokenRef = useRef(onToken);
  const envClientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID?.trim();
  const [remoteClientId, setRemoteClientId] = useState("");
  const [configLoaded, setConfigLoaded] = useState(!!envClientId);
  const [scriptReady, setScriptReady] = useState(false);

  onTokenRef.current = onToken;
  const clientId = envClientId || remoteClientId;

  useEffect(() => {
    if (envClientId) {
      setConfigLoaded(true);
      return;
    }
    api
      .getAuthPublicConfig()
      .then((res) => {
        if (res.success && res.data?.googleClientId) {
          setRemoteClientId(res.data.googleClientId.trim());
        }
      })
      .finally(() => setConfigLoaded(true));
  }, [envClientId]);

  useEffect(() => {
    if (!clientId) return;
    const existing = document.querySelector('script[src="https://accounts.google.com/gsi/client"]');
    if (existing) {
      setScriptReady(true);
      return;
    }
    const script = document.createElement("script");
    script.src = "https://accounts.google.com/gsi/client";
    script.async = true;
    script.onload = () => setScriptReady(true);
    document.body.appendChild(script);
  }, [clientId]);

  useEffect(() => {
    if (!scriptReady || !clientId || !ref.current || disabled) return;

    const el = ref.current;

    function render() {
      if (!el || !clientId) return;
      const width = el.offsetWidth > 0 ? el.offsetWidth : 320;

      window.google?.accounts.id.initialize({
        client_id: clientId,
        callback: (res) => onTokenRef.current(res.credential),
      });

      el.innerHTML = "";
      window.google?.accounts.id.renderButton(el, {
        theme: variant === "auth-flow" ? "filled_black" : "outline",
        size: "large",
        shape: variant === "auth-flow" ? "pill" : "rectangular",
        text: intent === "signup" ? "signup_with" : "signin_with",
        width: variant === "auth-flow" ? width : undefined,
      });
    }

    render();

    if (variant !== "auth-flow") return;

    const observer = new ResizeObserver(() => render());
    observer.observe(el);
    return () => observer.disconnect();
  }, [scriptReady, clientId, disabled, variant, intent]);

  if (!configLoaded) {
    return <div className="h-11 w-full animate-pulse rounded-full bg-white/5" />;
  }

  if (!clientId) {
    return (
      <div className="rounded-2xl border border-amber-500/25 bg-amber-500/10 p-4 text-left text-xs leading-relaxed text-amber-100/90">
        <p className="font-semibold text-amber-200">{t("oauth.googleMissing")}</p>
        <p className="mt-2 text-amber-100/75">{t("oauth.googleSetupIntro")}</p>
        <ol className="mt-2 list-decimal space-y-1 pl-4 text-amber-100/70">
          <li>{t("oauth.googleSetup1")}</li>
          <li>{t("oauth.googleSetup2")}</li>
          <li>{t("oauth.googleSetup3")}</li>
        </ol>
        <p className="mt-3 rounded-lg bg-black/30 px-3 py-2 font-mono text-[10px] text-amber-100/80">
          NEXT_PUBLIC_GOOGLE_CLIENT_ID=xxx.apps.googleusercontent.com
        </p>
        <p className="mt-1 text-amber-100/60">{t("oauth.googleSetupAlt")}</p>
      </div>
    );
  }

  return (
    <div
      ref={ref}
      className={cn(
        variant === "auth-flow" && "flex w-full min-h-[44px] justify-center [&>div]:!w-full",
        disabled && "pointer-events-none opacity-50",
      )}
    />
  );
}
