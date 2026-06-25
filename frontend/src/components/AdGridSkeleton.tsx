import { AdCardSkeleton } from "./AdCardSkeleton";
import { gridAds3 } from "@/lib/uiStyles";

type AdGridSkeletonProps = {
  count?: number;
  className?: string;
  showSummary?: boolean;
};

export function AdGridSkeleton({
  count = 12,
  className = "",
  showSummary = false,
}: AdGridSkeletonProps) {
  return (
    <div aria-busy="true" aria-live="polite">
      {showSummary && (
        <div className="mb-4 h-4 w-32 animate-pulse rounded-md bg-slate-200 dark:bg-slate-700" />
      )}
      <div className={`${gridAds3} ${className}`.trim()}>
        {Array.from({ length: count }, (_, index) => (
          <AdCardSkeleton key={index} />
        ))}
      </div>
    </div>
  );
}
