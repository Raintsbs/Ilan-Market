import { Suspense } from "react";
import { HomePage } from "@/components/HomePage";
import { AdGridSkeleton } from "@/components/AdGridSkeleton";

export default function Page() {
  return (
    <Suspense
      fallback={
        <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
          <AdGridSkeleton showSummary />
        </div>
      }
    >
      <HomePage />
    </Suspense>
  );
}
