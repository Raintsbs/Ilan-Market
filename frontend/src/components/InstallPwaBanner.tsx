"use client";

import { useEffect, useState } from "react";
import { useLocale } from "@/context/LocaleContext";

type BeforeInstallPromptEvent = Event & {
  prompt: () => Promise<void>;
  userChoice: Promise<{ outcome: "accepted" | "dismissed" }>;
};

export function InstallPwaBanner() {
  const { t } = useLocale();
  const [deferred, setDeferred] = useState<BeforeInstallPromptEvent | null>(null);
  const [dismissed, setDismissed] = useState(false);
  const [isStandalone, setIsStandalone] = useState(false);

  useEffect(() => {
    if (typeof window === "undefined") return;
    const standalone =
      window.matchMedia("(display-mode: standalone)").matches ||
      (window.navigator as Navigator & { standalone?: boolean }).standalone === true;
    setIsStandalone(standalone);
    if (localStorage.getItem("pwa_install_dismissed") === "1") setDismissed(true);

    function onPrompt(e: Event) {
      e.preventDefault();
      setDeferred(e as BeforeInstallPromptEvent);
    }
    window.addEventListener("beforeinstallprompt", onPrompt);
    return () => window.removeEventListener("beforeinstallprompt", onPrompt);
  }, []);

  if (isStandalone || dismissed) return null;

  async function install() {
    if (deferred) {
      await deferred.prompt();
      const choice = await deferred.userChoice;
      if (choice.outcome === "accepted") setDeferred(null);
      return;
    }
    alert(t("pwa.manualHint"));
  }

  function dismiss() {
    localStorage.setItem("pwa_install_dismissed", "1");
    setDismissed(true);
  }

  return (
    <div className="border-b border-indigo-200 bg-indigo-50 px-4 py-2.5 text-sm dark:border-indigo-900 dark:bg-indigo-950/50">
      <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-2">
        <p className="text-indigo-900 dark:text-indigo-100">{t("pwa.prompt")}</p>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={install}
            className="rounded-lg bg-indigo-600 px-3 py-1.5 text-xs font-semibold text-white"
          >
            {t("pwa.install")}
          </button>
          <button type="button" onClick={dismiss} className="text-xs text-indigo-700 dark:text-indigo-300">
            {t("pwa.dismiss")}
          </button>
        </div>
      </div>
    </div>
  );
}
