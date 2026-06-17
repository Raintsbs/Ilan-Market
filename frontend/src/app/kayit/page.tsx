"use client";

import { Suspense } from "react";
import { LoadingSpinner } from "@/components/LoadingSpinner";
import { useLocale } from "@/context/LocaleContext";
import RegisterForm from "./RegisterForm";

export default function RegisterPage() {
  const { t } = useLocale();
  return (
    <Suspense fallback={<LoadingSpinner label={t("common.loading")} />}>
      <RegisterForm />
    </Suspense>
  );
}
