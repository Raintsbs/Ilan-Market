"use client";

import React from "react";
import { motion, AnimatePresence } from "framer-motion";
import Link from "next/link";
import { AuthFlowShellRegister } from "@/components/ui/sign-in-flow-1";
import { AuthSubmitButton } from "@/components/ui/auth-submit-button";
import { useLocale } from "@/context/LocaleContext";

const inputClass =
  "w-full rounded-full border border-white/10 bg-white/5 py-3 px-4 text-center text-white outline-none backdrop-blur-[1px] transition-colors placeholder:text-white/40 focus:border-white/30";

type SignUpFlowProps = {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  error?: string;
  loading?: boolean;
  successReveal?: boolean;
  captchaSlot?: React.ReactNode;
  onFirstNameChange: (value: string) => void;
  onLastNameChange: (value: string) => void;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
};

export function SignUpFlow({
  firstName,
  lastName,
  email,
  password,
  error,
  loading,
  successReveal,
  captchaSlot,
  onFirstNameChange,
  onLastNameChange,
  onEmailChange,
  onPasswordChange,
  onSubmit,
}: SignUpFlowProps) {
  const { t } = useLocale();
  const canSubmit =
    firstName.trim().length > 0 &&
    lastName.trim().length > 0 &&
    email.trim().includes("@") &&
    password.trim().length >= 6;

  return (
    <AuthFlowShellRegister showSuccessReveal={successReveal}>
      <div className="flex flex-1 flex-col items-center justify-center px-4 pb-12">
        <div className="mt-[120px] w-full max-w-sm">
          <AnimatePresence mode="wait">
            {successReveal ? (
              <motion.div
                key="success"
                initial={{ opacity: 0, y: 40 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.4, ease: "easeOut", delay: 0.3 }}
                className="space-y-6 text-center"
              >
                <div className="space-y-1">
                  <h1 className="text-[2.5rem] leading-[1.1] font-bold tracking-tight text-white">
                    {t("auth.registerTitle")}
                  </h1>
                  <p className="text-[1.25rem] font-light text-white/50">{t("auth.registerLoading")}</p>
                </div>
                <motion.div
                  initial={{ scale: 0.8, opacity: 0 }}
                  animate={{ scale: 1, opacity: 1 }}
                  transition={{ duration: 0.5, delay: 0.5 }}
                  className="py-10"
                >
                  <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-gradient-to-br from-blue-400 to-blue-600">
                    <svg className="h-8 w-8 text-white" viewBox="0 0 20 20" fill="currentColor">
                      <path
                        fillRule="evenodd"
                        d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
                        clipRule="evenodd"
                      />
                    </svg>
                  </div>
                </motion.div>
              </motion.div>
            ) : (
              <motion.div
                key="form"
                initial={{ opacity: 0, x: 60 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: 60 }}
                transition={{ duration: 0.4, ease: "easeOut" }}
                className="space-y-6 text-center"
              >
                <div className="space-y-1">
                  <h1 className="text-[2.5rem] leading-[1.1] font-bold tracking-tight text-white">
                    {t("auth.registerTitle")}
                  </h1>
                  <p className="text-[1.25rem] font-light text-white/70">{t("auth.registerSubtitleFree")}</p>
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
                  <div className="grid grid-cols-2 gap-3">
                    <input
                      required
                      placeholder={t("auth.firstName")}
                      value={firstName}
                      onChange={(e) => onFirstNameChange(e.target.value)}
                      className={inputClass}
                      autoComplete="given-name"
                    />
                    <input
                      required
                      placeholder={t("auth.lastName")}
                      value={lastName}
                      onChange={(e) => onLastNameChange(e.target.value)}
                      className={inputClass}
                      autoComplete="family-name"
                    />
                  </div>
                  <input
                    type="email"
                    required
                    placeholder={t("auth.email")}
                    value={email}
                    onChange={(e) => onEmailChange(e.target.value)}
                    className={inputClass}
                    autoComplete="email"
                  />
                  <input
                    type="password"
                    required
                    minLength={6}
                    placeholder={t("auth.password")}
                    value={password}
                    onChange={(e) => onPasswordChange(e.target.value)}
                    className={inputClass}
                    autoComplete="new-password"
                  />
                  {captchaSlot}
                  <AuthSubmitButton active={canSubmit} loading={loading}>
                    {loading ? t("auth.registerSaving") : t("auth.registerBtn")}
                  </AuthSubmitButton>
                </form>

                <p className="pt-4 text-sm text-white/40">
                  {t("auth.hasAccount")}{" "}
                  <Link href="/giris" className="font-medium text-white/60 underline hover:text-white/90">
                    {t("auth.loginLink")}
                  </Link>
                </p>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </div>
    </AuthFlowShellRegister>
  );
}
