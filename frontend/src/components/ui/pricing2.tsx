"use client";

import { ArrowRight, CircleCheck, Sparkles } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { cn } from "@/lib/utils";

export interface PricingFeature {
  text: string;
}

export interface FeaturedPricingPlan {
  id: string;
  packageId: number;
  name: string;
  description: string;
  priceLabel: string;
  price: number;
  periodLabel: string;
  featuredDays: number;
  features: PricingFeature[];
  popular?: boolean;
}

interface Pricing2Props {
  heading?: string;
  description?: string;
  plans?: FeaturedPricingPlan[];
  loading?: boolean;
  processingId?: number | null;
  onPurchase?: (plan: FeaturedPricingPlan) => void;
  purchaseLabel?: string;
  popularLabel?: string;
}

const Pricing2 = ({
  heading = "Pricing",
  description = "Check out our affordable pricing plans",
  plans = [],
  loading = false,
  processingId = null,
  onPurchase,
  purchaseLabel = "Purchase",
  popularLabel = "Popular",
}: Pricing2Props) => {
  if (loading) {
    return (
      <section className="py-8 md:py-12">
        <div className="mx-auto flex max-w-5xl flex-col items-center gap-6">
          <div className="h-10 w-64 animate-pulse rounded-lg bg-slate-200 dark:bg-slate-800" />
          <div className="grid w-full gap-6 md:grid-cols-2">
            {[1, 2].map((i) => (
              <div key={i} className="h-96 animate-pulse rounded-xl bg-slate-200 dark:bg-slate-800" />
            ))}
          </div>
        </div>
      </section>
    );
  }

  return (
    <section className="py-8 md:py-12">
      <div className="mx-auto max-w-5xl px-4 sm:px-6">
        <div className="mx-auto flex max-w-3xl flex-col items-center gap-4 text-center">
          <div className="inline-flex items-center gap-2 rounded-full border border-amber-200/60 bg-amber-50 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-amber-800 dark:border-amber-900/50 dark:bg-amber-950/40 dark:text-amber-200">
            <Sparkles className="size-3.5" />
            Premium
          </div>
          <h2 className="text-pretty text-3xl font-bold tracking-tight text-slate-900 dark:text-white md:text-4xl">
            {heading}
          </h2>
          <p className="text-base text-[var(--muted-foreground)] md:text-lg">{description}</p>
        </div>

        <div className="mt-10 flex flex-col items-stretch justify-center gap-6 md:flex-row md:items-start">
          {plans.map((plan) => (
            <Card
              key={plan.id}
              className={cn(
                "relative flex w-full max-w-sm flex-col justify-between text-left transition hover:-translate-y-0.5 hover:shadow-lg md:w-80",
                plan.popular &&
                  "border-blue-500/50 shadow-md shadow-blue-500/10 ring-1 ring-blue-500/20 dark:border-blue-500/40",
              )}
            >
              {plan.popular && (
                <span className="absolute -top-3 left-1/2 -translate-x-1/2 rounded-full bg-blue-600 px-3 py-0.5 text-xs font-semibold text-white">
                  {popularLabel}
                </span>
              )}
              <CardHeader className="pb-4">
                <CardTitle className="text-xl">{plan.name}</CardTitle>
                <p className="text-sm text-[var(--muted-foreground)]">{plan.description}</p>
                <div className="pt-2">
                  <span className="text-4xl font-bold tracking-tight">{plan.priceLabel}</span>
                  <p className="mt-1 text-sm text-[var(--muted-foreground)]">{plan.periodLabel}</p>
                </div>
              </CardHeader>
              <CardContent>
                <Separator className="mb-5" />
                <ul className="space-y-3">
                  {plan.features.map((feature, index) => (
                    <li key={index} className="flex items-start gap-2.5 text-sm">
                      <CircleCheck className="mt-0.5 size-4 shrink-0 text-emerald-500" />
                      <span>{feature.text}</span>
                    </li>
                  ))}
                </ul>
              </CardContent>
              <CardFooter className="mt-auto">
                <Button
                  type="button"
                  disabled={processingId === plan.packageId}
                  onClick={() => onPurchase?.(plan)}
                  className="w-full border-0 bg-blue-600 text-white shadow-sm hover:bg-blue-700 hover:translate-none hover:shadow-md disabled:opacity-60"
                >
                  {processingId === plan.packageId ? "…" : purchaseLabel}
                  <ArrowRight className="ml-1 size-4" />
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      </div>
    </section>
  );
};

export { Pricing2 };
