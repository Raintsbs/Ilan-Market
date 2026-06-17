"use client";

import { Search } from "lucide-react";
import { useId } from "react";
import { cn } from "@/lib/utils";
import { inputField } from "@/lib/uiStyles";

export interface AnimatedGlowingSearchBarProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  name?: string;
  className?: string;
  id?: string;
  /** subtle: kategoriler sayfası; varsayılan: ana sayfa ilan filtreleri */
  variant?: "default" | "subtle";
}

export function AnimatedGlowingSearchBar({
  value,
  onChange,
  placeholder = "Ara…",
  name = "search",
  className,
  id,
  variant = "default",
}: AnimatedGlowingSearchBarProps) {
  const uid = useId().replace(/:/g, "");
  const inputId = id ?? `search-${uid}`;

  if (variant === "subtle") {
    return (
      <div className={cn("relative w-full min-w-0", className)}>
        <Search
          className="pointer-events-none absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400 dark:text-slate-500"
          strokeWidth={2}
          aria-hidden
        />
        <input
          id={inputId}
          type="search"
          name={name}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder={placeholder}
          autoComplete="off"
          className={cn(inputField, "h-10 pl-10")}
        />
      </div>
    );
  }

  return (
    <div className={cn("group relative w-full min-w-0", className)}>
      <div
        aria-hidden
        className="pointer-events-none absolute -inset-[2px] rounded-2xl opacity-0 blur-md transition-opacity duration-500 group-focus-within:opacity-100 dark:group-focus-within:opacity-90"
        style={{
          background:
            "conic-gradient(from 0deg, transparent, #3b82f6, #1d4ed8, transparent, transparent, #60a5fa, transparent)",
        }}
      />
      <div
        aria-hidden
        className="search-glow-ring pointer-events-none absolute -inset-px rounded-[15px] opacity-0 transition-opacity duration-300 group-focus-within:opacity-100"
      />
      <div className="relative flex items-center">
        <Search
          className="pointer-events-none absolute left-4 z-10 h-[18px] w-[18px] text-slate-500 transition-colors group-focus-within:text-blue-600 dark:text-slate-400 dark:group-focus-within:text-blue-400"
          strokeWidth={2.25}
          aria-hidden
        />
        <input
          id={inputId}
          type="search"
          name={name}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder={placeholder}
          autoComplete="off"
          className={cn(
            "relative z-[1] h-12 w-full rounded-2xl border border-slate-200 bg-white pl-11 pr-4",
            "text-base font-medium text-slate-900 placeholder:font-normal placeholder:text-slate-500",
            "shadow-sm outline-none transition-all duration-300",
            "focus:border-blue-500 focus:ring-2 focus:ring-blue-500/25",
            "dark:border-slate-600 dark:bg-slate-900 dark:text-slate-50 dark:placeholder:text-slate-500",
            "dark:focus:border-blue-500 dark:focus:ring-blue-500/30",
          )}
        />
      </div>
    </div>
  );
}

export default AnimatedGlowingSearchBar;
