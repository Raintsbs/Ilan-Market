"use client";

import { useState } from "react";
import { useLocale } from "@/context/LocaleContext";

interface SafeImageProps {
  src: string;
  alt: string;
  className?: string;
  fill?: boolean;
  onClick?: () => void;
  priority?: boolean;
}

/** next/image localhost sorunlarında düz img kullanır */
export function SafeImage({ src, alt, className = "", fill, onClick }: SafeImageProps) {
  const { t } = useLocale();
  const [failed, setFailed] = useState(false);

  if (failed || !src) {
    return (
      <div
        className={`flex items-center justify-center bg-slate-100 text-slate-400 ${fill ? "absolute inset-0" : ""} ${className}`}
      >
        <span className="text-sm">{t("ad.imageFailed")}</span>
      </div>
    );
  }

  if (fill) {
    return (
      // eslint-disable-next-line @next/next/no-img-element
      <img
        src={src}
        alt={alt}
        className={`absolute inset-0 h-full w-full ${className}`}
        onClick={onClick}
        onError={() => setFailed(true)}
      />
    );
  }

  return (
    // eslint-disable-next-line @next/next/no-img-element
    <img
      src={src}
      alt={alt}
      className={className}
      onClick={onClick}
      onError={() => setFailed(true)}
    />
  );
}
