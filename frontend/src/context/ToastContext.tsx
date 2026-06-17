"use client";



import {

  createContext,

  useCallback,

  useContext,

  useMemo,

  useState,

} from "react";

import { useLocale } from "@/context/LocaleContext";



export type ToastType = "success" | "error" | "info";



interface ToastItem {

  id: number;

  message: string;

  type: ToastType;

  exiting?: boolean;

}



interface ToastContextValue {

  showToast: (message: string, type?: ToastType) => void;

}



const ToastContext = createContext<ToastContextValue | null>(null);



let toastId = 0;

const TOAST_LIFE_MS = 4500;

const TOAST_EXIT_MS = 280;



export function ToastProvider({ children }: { children: React.ReactNode }) {

  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const { t } = useLocale();



  const removeToast = useCallback((id: number) => {

    setToasts((prev) => prev.filter((x) => x.id !== id));

  }, []);



  const startExit = useCallback(

    (id: number) => {

      setToasts((prev) =>

        prev.map((x) => (x.id === id ? { ...x, exiting: true } : x)),

      );

      window.setTimeout(() => removeToast(id), TOAST_EXIT_MS);

    },

    [removeToast],

  );



  const showToast = useCallback(

    (message: string, type: ToastType = "info") => {

      const id = ++toastId;

      setToasts((prev) => [...prev, { id, message, type }]);

      window.setTimeout(() => startExit(id), TOAST_LIFE_MS);

    },

    [startExit],

  );



  const dismiss = useCallback(

    (id: number) => {

      startExit(id);

    },

    [startExit],

  );



  const value = useMemo(() => ({ showToast }), [showToast]);



  return (

    <ToastContext.Provider value={value}>

      {children}

      <div

        className="pointer-events-none fixed inset-0 z-[200] flex flex-col items-end justify-start gap-0 p-4 pt-20 sm:pr-6"

        aria-live="polite"

      >

        {toasts.map((toast) => (

          <div

            key={toast.id}

            role="alert"

            className={`pointer-events-auto mb-3 flex max-w-sm items-start gap-3 rounded-2xl border px-4 py-3 shadow-xl backdrop-blur ${

              toast.exiting ? "animate-toast-out" : "animate-toast-in"

            } ${

              toast.type === "success"

                ? "border-emerald-200/80 bg-emerald-50/95 text-emerald-900 dark:border-emerald-800 dark:bg-emerald-950/95 dark:text-emerald-100"

                : toast.type === "error"

                  ? "border-rose-200/80 bg-rose-50/95 text-rose-900 dark:border-rose-800 dark:bg-rose-950/95 dark:text-rose-100"

                  : "border-slate-200/80 bg-white/95 text-slate-900 dark:border-slate-600 dark:bg-slate-900/95 dark:text-slate-100"

            }`}

          >

            <span className="mt-0.5 shrink-0 text-lg" aria-hidden>

              {toast.type === "success" ? "✓" : toast.type === "error" ? "!" : "ℹ"}

            </span>

            <p className="flex-1 text-sm font-medium">{toast.message}</p>

            <button

              type="button"

              onClick={() => dismiss(toast.id)}

              className="shrink-0 rounded-lg px-2 py-1 text-xs opacity-70 hover:opacity-100"

              aria-label={t("toast.close")}

            >

              ✕

            </button>

          </div>

        ))}

      </div>

    </ToastContext.Provider>

  );

}



export function useToast() {

  const ctx = useContext(ToastContext);

  if (!ctx) throw new Error("useToast must be used within ToastProvider");

  return ctx;

}

