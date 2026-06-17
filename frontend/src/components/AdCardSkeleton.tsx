export function AdCardSkeleton() {
  return (
    <div
      className="flex flex-col overflow-hidden rounded-2xl border border-slate-200/80 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-900"
      aria-hidden="true"
    >
      <div className="relative aspect-[16/10] animate-pulse bg-slate-100 dark:bg-slate-800">
        <div className="absolute left-3 top-3 h-6 w-16 rounded-full bg-slate-200/90 dark:bg-slate-700/90" />
      </div>
      <div className="flex flex-1 flex-col gap-3 p-4">
        <div className="h-6 w-24 animate-pulse rounded-lg bg-slate-200 dark:bg-slate-700" />
        <div className="space-y-2">
          <div className="h-4 w-full animate-pulse rounded-md bg-slate-200 dark:bg-slate-700" />
          <div className="h-4 w-[85%] animate-pulse rounded-md bg-slate-200 dark:bg-slate-700" />
        </div>
        <div className="space-y-2">
          <div className="h-3 w-full animate-pulse rounded bg-slate-100 dark:bg-slate-800" />
          <div className="h-3 w-[70%] animate-pulse rounded bg-slate-100 dark:bg-slate-800" />
        </div>
        <div className="mt-auto pt-2">
          <div className="h-3 w-20 animate-pulse rounded bg-slate-100 dark:bg-slate-800" />
        </div>
      </div>
    </div>
  );
}
