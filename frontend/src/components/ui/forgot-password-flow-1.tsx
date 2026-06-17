"use client";

import React from "react";
import { motion, AnimatePresence } from "framer-motion";
import Link from "next/link";
import { Mail } from "lucide-react";
import { AuthFlowShellForgot } from "@/components/ui/sign-in-flow-1";
import { AuthSubmitButton } from "@/components/ui/auth-submit-button";
import { useLocale } from "@/context/LocaleContext";

const inputClass =
  "w-full rounded-full border border-white/10 bg-white/5 py-3 px-4 text-center text-white outline-none backdrop-blur-[1px] transition-colors placeholder:text-white/40 focus:border-white/30";

type ForgotPasswordFlowProps = {
  email: string;
  error?: string;
  loading?: boolean;
  sent?: boolean;
  devPickupHint?: boolean;
  onEmailChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
};

export function ForgotPasswordFlow({
  email,
  error,
  loading,
  sent,
  devPickupHint,
  onEmailChange,
  onSubmit,
}: ForgotPasswordFlowProps) {
  const { t } = useLocale();
  const canSubmit = email.trim().length > 0 && email.includes("@");

  return (
    <AuthFlowShellForgot>
      <div className="flex flex-1 flex-col items-center justify-center px-4 pb-12">
        <div className="mt-[120px] w-full max-w-sm">
          <AnimatePresence mode="wait">
            {sent ? (
              <motion.div
                key="sent"
                initial={{ opacity: 0, y: 30 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4, ease: "easeOut" }}
                className="space-y-6 text-center"
              >
                <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-gradient-to-br from-blue-400 to-blue-600">
                  <Mail className="h-8 w-8 text-white" />
                </div>
                <div className="space-y-2">
                  <h1 className="text-2xl font-bold tracking-tight text-white">{t("auth.forgotTitle")}</h1>
                  <p className="text-sm leading-relaxed text-white/60">{t("auth.forgotSent")}</p>
                  {devPickupHint && (
                    <p className="text-xs leading-relaxed text-amber-300/90">{t("auth.forgotPickupHint")}</p>
                  )}
                </div>
                <Link
                  href="/giris"
                  className="inline-flex items-center gap-2 text-sm font-medium text-blue-400 transition-colors hover:text-blue-300"
                >
                  ← {t("auth.loginTitle")}
                </Link>
              </motion.div>
            ) : (
              <motion.div
                key="form"
                initial={{ opacity: 0, x: -40 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -40 }}
                transition={{ duration: 0.4, ease: "easeOut" }}
                className="space-y-6 text-center"
              >
                <div className="space-y-1">
                  <h1 className="text-[2.5rem] leading-[1.1] font-bold tracking-tight text-white">
                    {t("auth.forgotTitle")}
                  </h1>
                  <p className="text-[1.1rem] font-light text-white/70">{t("auth.forgotSubtitle")}</p>
                </div>

                {error && (
                  <motion.div
                    initial={{ opacity: 0, y: -8 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="rounded-full border border-rose-500/30 bg-rose-500/10 px-4 py-2 text-sm text-rose-200"
                  >
                    {error}
                  </motion.div>
                )}

                <form onSubmit={onSubmit} className="space-y-3">
                  <input
                    type="email"
                    required
                    placeholder={t("auth.email")}
                    value={email}
                    onChange={(e) => onEmailChange(e.target.value)}
                    className={inputClass}
                    autoComplete="email"
                  />
                  <AuthSubmitButton active={canSubmit} loading={loading}>
                    {loading ? "…" : t("auth.forgotSubmit")}
                  </AuthSubmitButton>
                </form>

                <p className="pt-4 text-sm text-white/40">
                  <Link href="/giris" className="font-medium text-white/60 underline hover:text-white/90">
                    ← {t("auth.loginTitle")}
                  </Link>
                </p>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </div>
    </AuthFlowShellForgot>
  );
}
