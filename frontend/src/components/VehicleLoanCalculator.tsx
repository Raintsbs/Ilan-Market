"use client";

import { useMemo, useState } from "react";
import { useLocale } from "@/context/LocaleContext";
import { formatPrice } from "@/lib/listingDetails";
import { formFieldClass as inputClass } from "@/lib/formStyles";
import { surfaceElevated } from "@/lib/uiStyles";

type VehicleLoanCalculatorProps = {
  price: number | null | undefined;
};

export function VehicleLoanCalculator({ price }: VehicleLoanCalculatorProps) {
  const { t, locale } = useLocale();
  const basePrice = price != null && price > 0 ? price : 0;

  const [downPercent, setDownPercent] = useState(30);
  const [months, setMonths] = useState(36);
  const [annualRate, setAnnualRate] = useState(3.2);

  const result = useMemo(() => {
    if (basePrice <= 0) return null;
    const down = (basePrice * downPercent) / 100;
    const principal = basePrice - down;
    if (principal <= 0) return { down, monthly: 0, total: down };

    const monthlyRate = annualRate / 100 / 12;
    const monthly =
      monthlyRate === 0
        ? principal / months
        : (principal * monthlyRate * (1 + monthlyRate) ** months) /
          ((1 + monthlyRate) ** months - 1);

    return { down, monthly, total: down + monthly * months };
  }, [basePrice, downPercent, months, annualRate]);

  if (basePrice <= 0) return null;

  return (
    <section className={`p-5 ${surfaceElevated}`}>
      <h2 className="text-sm font-bold uppercase tracking-wide text-slate-500 dark:text-slate-400">
        {t("loan.title")}
      </h2>
      <p className="mt-1 text-xs text-slate-500">{t("loan.hint")}</p>

      <div className="mt-4 grid gap-4 sm:grid-cols-3">
        <label className="block text-sm">
          <span className="mb-1 block font-medium text-slate-700 dark:text-slate-300">{t("loan.downPayment")}</span>
          <input
            type="range"
            min={0}
            max={80}
            step={5}
            value={downPercent}
            onChange={(e) => setDownPercent(Number(e.target.value))}
            className="w-full"
          />
          <span className="text-slate-600 dark:text-slate-400">%{downPercent}</span>
        </label>
        <label className="block text-sm">
          <span className="mb-1 block font-medium text-slate-700 dark:text-slate-300">{t("loan.term")}</span>
          <select
            value={months}
            onChange={(e) => setMonths(Number(e.target.value))}
            className={inputClass}
          >
            {[12, 24, 36, 48, 60].map((m) => (
              <option key={m} value={m}>
                {m} {t("loan.months")}
              </option>
            ))}
          </select>
        </label>
        <label className="block text-sm">
          <span className="mb-1 block font-medium text-slate-700 dark:text-slate-300">{t("loan.annualRate")}</span>
          <input
            type="number"
            min={0}
            max={100}
            step={0.1}
            value={annualRate}
            onChange={(e) => setAnnualRate(Number(e.target.value))}
            className={inputClass}
          />
        </label>
      </div>

      {result && (
        <dl className="mt-4 grid gap-2 rounded-xl bg-slate-50 p-4 text-sm dark:bg-slate-800/80 sm:grid-cols-3">
          <div>
            <dt className="text-slate-500">{t("loan.downAmount")}</dt>
            <dd className="font-semibold text-slate-900 dark:text-white">
              {formatPrice(result.down, t("price.notSet"), locale)}
            </dd>
          </div>
          <div>
            <dt className="text-slate-500">{t("loan.monthlyPayment")}</dt>
            <dd className="text-lg font-bold text-blue-600 dark:text-blue-400">
              {formatPrice(result.monthly, t("price.notSet"), locale)}
            </dd>
          </div>
          <div>
            <dt className="text-slate-500">{t("loan.total")}</dt>
            <dd className="font-semibold text-slate-900 dark:text-white">
              {formatPrice(result.total, t("price.notSet"), locale)}
            </dd>
          </div>
        </dl>
      )}
    </section>
  );
}
