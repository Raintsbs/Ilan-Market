"use client";

import { Moon, Sun } from "lucide-react";
import { useLocale } from "@/context/LocaleContext";
import { useTheme } from "@/context/ThemeContext";
import { cn } from "@/lib/utils";

interface ThemeToggleProps {
  className?: string;
}

export function ThemeToggle({ className }: ThemeToggleProps) {
  const { resolved, setMode } = useTheme();
  const { t } = useLocale();
  const isDark = resolved === "dark";

  function toggle() {
    setMode(isDark ? "light" : "dark");
  }

  function onKeyDown(e: React.KeyboardEvent) {
    if (e.key === "Enter" || e.key === " ") {
      e.preventDefault();
      toggle();
    }
  }

  return (
    <div
      className={cn(
        "flex h-8 w-16 shrink-0 cursor-pointer rounded-full p-1 transition-all duration-300",
        isDark
          ? "border border-slate-800 bg-slate-950"
          : "border border-slate-200 bg-white shadow-sm",
        className,
      )}
      onClick={toggle}
      onKeyDown={onKeyDown}
      role="button"
      tabIndex={0}
      aria-label={t("theme.toggle")}
      title={isDark ? t("theme.light") : t("theme.dark")}
    >
      <div className="flex w-full items-center justify-between">
        <div
          className={cn(
            "flex h-6 w-6 items-center justify-center rounded-full transition-transform duration-300",
            isDark ? "translate-x-0 bg-slate-800" : "translate-x-8 bg-slate-100",
          )}
        >
          {isDark ? (
            <Moon className="h-4 w-4 text-white" strokeWidth={1.5} />
          ) : (
            <Sun className="h-4 w-4 text-slate-700" strokeWidth={1.5} />
          )}
        </div>
        <div
          className={cn(
            "flex h-6 w-6 items-center justify-center rounded-full transition-transform duration-300",
            isDark ? "bg-transparent" : "-translate-x-8",
          )}
        >
          {isDark ? (
            <Sun className="h-4 w-4 text-slate-500" strokeWidth={1.5} />
          ) : (
            <Moon className="h-4 w-4 text-slate-800" strokeWidth={1.5} />
          )}
        </div>
      </div>
    </div>
  );
}

export default ThemeToggle;
