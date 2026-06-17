"use client";

export type CategoryStat = {
  categoryId: number;
  categoryName: string;
  count: number;
};

export function AdminCategoryChart({ data }: { data: CategoryStat[] }) {
  if (data.length === 0) {
    return (
      <p className="py-8 text-center text-sm text-slate-500">Henüz ilan verisi yok.</p>
    );
  }

  const max = Math.max(...data.map((d) => d.count), 1);

  return (
    <div className="space-y-3">
      {data.map((item, index) => {
        const pct = Math.round((item.count / max) * 100);
        return (
          <div key={item.categoryId} className="group">
            <div className="mb-1 flex items-center justify-between gap-2 text-sm">
              <span className="truncate font-medium text-slate-200" title={item.categoryName}>
                {item.categoryName}
              </span>
              <span className="shrink-0 tabular-nums text-slate-400">{item.count}</span>
            </div>
            <div className="h-2 overflow-hidden rounded-full bg-white/[0.06]">
              <div
                className="admin-chart-bar h-full rounded-full bg-gradient-to-r from-blue-500 to-cyan-400"
                style={{
                  width: `${pct}%`,
                  animationDelay: `${index * 60}ms`,
                }}
              />
            </div>
          </div>
        );
      })}
    </div>
  );
}
