"use client";

import { ArrowRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type AuthSubmitButtonProps = {
  active: boolean;
  loading?: boolean;
  children: React.ReactNode;
  className?: string;
} & Omit<React.ButtonHTMLAttributes<HTMLButtonElement>, "children">;

export function AuthSubmitButton({
  active,
  loading,
  children,
  className,
  disabled,
  ...props
}: AuthSubmitButtonProps) {
  const isEnabled = active && !loading && !disabled;

  return (
    <Button
      type="submit"
      variant={isEnabled ? "auth" : "authMuted"}
      size="auth"
      disabled={!isEnabled}
      className={cn("w-full", className)}
      {...props}
    >
      {children}
      {isEnabled && <ArrowRight />}
    </Button>
  );
}
