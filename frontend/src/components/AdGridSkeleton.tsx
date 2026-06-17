import { AdCardSkeleton } from "./AdCardSkeleton";

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
      <div className={`grid gap-6 sm:grid-cols-2 lg:grid-cols-3 ${className}`.trim()}>
        {Array.from({ length: count }, (_, index) => (
          <AdCardSkeleton key={index} />
        ))}
      </div>
    </div>
  );
}
