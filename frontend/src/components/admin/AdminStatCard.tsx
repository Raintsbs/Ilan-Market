import Link from "next/link";
import type { LucideIcon } from "lucide-react";

type AdminStatCardProps = {
  label: string;
  value: number | string;
  href?: string;
  icon: LucideIcon;
  accent?: "blue" | "amber" | "emerald" | "rose" | "violet" | "slate";
};

const accents = {
  blue: "from-blue-500/20 to-blue-600/5 text-blue-400",
  amber: "from-amber-500/20 to-amber-600/5 text-amber-400",
  emerald: "from-emerald-500/20 to-emerald-600/5 text-emerald-400",
  rose: "from-rose-500/20 to-rose-600/5 text-rose-400",
  violet: "from-violet-500/20 to-violet-600/5 text-violet-400",
  slate: "from-slate-500/20 to-slate-600/5 text-slate-400",
};

export function AdminStatCard({
  label,
  value,
  href,
  icon: Icon,
  accent = "blue",
}: AdminStatCardProps) {
  const inner = (
    <>
      <div
        className={`flex h-11 w-11 items-center justify-center rounded-xl bg-gradient-to-br ${accents[accent]}`}
      >
        <Icon className="size-5" strokeWidth={2} />
      </div>
      <div className="mt-4">
        <p className="text-sm font-medium text-slate-400">{label}</p>
        <p className="mt-1 text-3xl font-bold tabular-nums tracking-tight text-white">{value}</p>
      </div>
    </>
  );

  const className =
    "group block rounded-2xl border border-white/[0.08] bg-white/[0.03] p-5 transition hover:border-white/[0.14] hover:bg-white/[0.05] hover:shadow-lg hover:shadow-black/20";

  if (href) {
    return (
      <Link href={href} className={className}>
        {inner}
      </Link>
    );
  }

  return <div className={className}>{inner}</div>;
}
