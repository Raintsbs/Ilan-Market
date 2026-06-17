"use client";

import React, { useEffect, useRef, useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import Link from "next/link";
import { Menu, X } from "lucide-react";
import { cn } from "@/lib/utils";
import { AuthSubmitButton } from "@/components/ui/auth-submit-button";
import { AuthFlowBackground } from "@/components/ui/auth-flow-canvas";
import { useLocale } from "@/context/LocaleContext";

type AuthFlowShellProps = {
  children: React.ReactNode;
  showSuccessReveal?: boolean;
  className?: string;
};

function AuthFlowNav({ active }: { active: "login" | "register" | "forgot" }) {
  const { t } = useLocale();
  const [isOpen, setIsOpen] = useState(false);
  const [headerShapeClass, setHeaderShapeClass] = useState("rounded-full");
  const shapeTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (shapeTimeoutRef.current) clearTimeout(shapeTimeoutRef.current);
    if (isOpen) {
      setHeaderShapeClass("rounded-xl");
    } else {
      shapeTimeoutRef.current = setTimeout(() => setHeaderShapeClass("rounded-full"), 300);
    }
    return () => {
      if (shapeTimeoutRef.current) clearTimeout(shapeTimeoutRef.current);
    };
  }, [isOpen]);

  const navLinks = [
    { label: t("nav.ads"), href: "/" },
    { label: t("nav.categories"), href: "/kategoriler" },
    { label: t("header.help"), href: "/yardim" },
  ];

  return (
    <header
      className={cn(
        "fixed top-6 left-1/2 z-20 flex w-[calc(100%-2rem)] -translate-x-1/2 flex-col items-center border border-[#333] bg-[#1f1f1f57] py-3 pr-6 pl-6 backdrop-blur-sm transition-[border-radius] sm:w-auto",
        headerShapeClass,
      )}
    >
      <div className="flex w-full items-center justify-between gap-x-6 sm:gap-x-8">
        <Link href="/" className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-600 text-xs font-bold text-white">
            İM
          </div>
          <span className="hidden text-sm font-semibold text-white sm:inline">İlanMarket</span>
        </Link>

        <nav className="hidden items-center space-x-4 text-sm sm:flex sm:space-x-6">
          {navLinks.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className="text-gray-300 transition-colors hover:text-white"
            >
              {link.label}
            </Link>
          ))}
        </nav>

        <div className="hidden items-center gap-2 sm:flex sm:gap-3">
          {active === "register" ? (
            <Link
              href="/giris"
              className="rounded-full border border-[#333] bg-[rgba(31,31,31,0.62)] px-4 py-2 text-xs text-gray-300 transition-colors hover:border-white/50 hover:text-white sm:px-3 sm:text-sm"
            >
              {t("auth.loginShort")}
            </Link>
          ) : (
            <Link
              href="/kayit"
              className="relative rounded-full bg-gradient-to-br from-blue-500 to-blue-700 px-4 py-2 text-xs font-semibold text-white transition-all hover:from-blue-400 hover:to-blue-600 sm:px-3 sm:text-sm"
            >
              {t("auth.registerBtn")}
            </Link>
          )}
        </div>

        <button
          type="button"
          className="flex h-8 w-8 items-center justify-center text-gray-300 focus:outline-none sm:hidden"
          onClick={() => setIsOpen(!isOpen)}
          aria-label={isOpen ? "Menüyü kapat" : "Menüyü aç"}
        >
          {isOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
        </button>
      </div>

      <div
        className={cn(
          "flex w-full flex-col items-center overflow-hidden transition-all duration-300 ease-in-out sm:hidden",
          isOpen ? "max-h-[1000px] pt-4 opacity-100" : "pointer-events-none max-h-0 pt-0 opacity-0",
        )}
      >
        <nav className="flex w-full flex-col items-center space-y-4 text-base">
          {navLinks.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className="w-full text-center text-gray-300 transition-colors hover:text-white"
            >
              {link.label}
            </Link>
          ))}
        </nav>
        <div className="mt-4 flex w-full flex-col items-center space-y-4">
          <Link
            href={active === "login" ? "/kayit" : "/giris"}
            className="w-full rounded-full border border-[#333] bg-[rgba(31,31,31,0.62)] px-4 py-2 text-center text-sm text-gray-300"
          >
            {active === "login" ? t("auth.registerBtn") : t("auth.loginShort")}
          </Link>
        </div>
      </div>
    </header>
  );
}

export function AuthFlowShell({ children, showSuccessReveal = false, className }: AuthFlowShellProps) {
  return (
    <div className={cn("auth-flow relative flex min-h-screen w-full flex-col bg-black", className)}>
      <AuthFlowBackground showSuccessReveal={showSuccessReveal} />
      <div className="relative z-10 flex flex-1 flex-col">
        <AuthFlowNav active="login" />
        {children}
      </div>
    </div>
  );
}

export function AuthFlowShellRegister({
  children,
  showSuccessReveal = false,
  className,
}: AuthFlowShellProps) {
  return (
    <div className={cn("auth-flow relative flex min-h-screen w-full flex-col bg-black", className)}>
      <AuthFlowBackground showSuccessReveal={showSuccessReveal} />
      <div className="relative z-10 flex flex-1 flex-col">
        <AuthFlowNav active="register" />
        {children}
      </div>
    </div>
  );
}

export function AuthFlowShellForgot({ children, className }: AuthFlowShellProps) {
  return (
    <div className={cn("auth-flow relative flex min-h-screen w-full flex-col bg-black", className)}>
      <AuthFlowBackground />
      <div className="relative z-10 flex flex-1 flex-col">
        <AuthFlowNav active="forgot" />
        {children}
      </div>
    </div>
  );
}

const inputClass =
  "w-full rounded-full border border-white/10 bg-white/5 py-3 px-4 text-center text-white outline-none backdrop-blur-[1px] transition-colors placeholder:text-white/40 focus:border-white/30";

type SignInFlowProps = {
  email: string;
  password: string;
  error?: string;
  loading?: boolean;
  successReveal?: boolean;
  captchaSlot?: React.ReactNode;
  onEmailChange: (value: string) => void;
  onPasswordChange: (value: string) => void;
  onSubmit: (e: React.FormEvent) => void;
};

export function SignInFlow({
  email,
  password,
  error,
  loading,
  successReveal,
  captchaSlot,
  onEmailChange,
  onPasswordChange,
  onSubmit,
}: SignInFlowProps) {
  const { t } = useLocale();
  const canSubmit = email.trim().length > 0 && password.trim().length > 0;

  return (
    <AuthFlowShell showSuccessReveal={successReveal}>
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
                    {t("auth.loginTitle")}
                  </h1>
                  <p className="text-[1.25rem] font-light text-white/50">{t("auth.loginLoading")}</p>
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
                initial={{ opacity: 0, x: -60 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -60 }}
                transition={{ duration: 0.4, ease: "easeOut" }}
                className="space-y-6 text-center"
              >
                <div className="space-y-1">
                  <h1 className="text-[2.5rem] leading-[1.1] font-bold tracking-tight text-white">
                    {t("auth.loginTitle")}
                  </h1>
                  <p className="text-[1.25rem] font-light text-white/70">{t("auth.loginSubtitle")}</p>
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

                <div className="space-y-4">
                  <form onSubmit={onSubmit} className="space-y-3">
                    <input
                      type="email"
                      placeholder={t("auth.email")}
                      value={email}
                      onChange={(e) => onEmailChange(e.target.value)}
                      className={inputClass}
                      required
                      autoComplete="email"
                    />
                    <input
                      type="password"
                      placeholder={t("auth.password")}
                      value={password}
                      onChange={(e) => onPasswordChange(e.target.value)}
                      className={inputClass}
                      required
                      autoComplete="current-password"
                    />
                    {captchaSlot}
                    <div className="relative pt-1">
                      <AuthSubmitButton active={canSubmit} loading={loading}>
                        {loading ? t("auth.loginLoading") : t("auth.loginBtn")}
                      </AuthSubmitButton>
                    </div>
                  </form>

                  <p className="text-sm">
                    <Link
                      href="/sifremi-unuttum"
                      className="text-white/50 transition-colors hover:text-white/80"
                    >
                      {t("auth.forgotPassword")}
                    </Link>
                  </p>
                </div>

                <p className="pt-6 text-sm text-white/40">
                  {t("auth.noAccount")}{" "}
                  <Link href="/kayit" className="font-medium text-white/60 underline hover:text-white/90">
                    {t("auth.registerLink")}
                  </Link>
                </p>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </div>
    </AuthFlowShell>
  );
}

/** @deprecated Use SignInFlow — demo export adı korundu */
export const SignInPage = SignInFlow;
