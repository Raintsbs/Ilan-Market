/** Admin paneli — paylaşılan yüzey ve form sınıfları */

export const adminPage = "mx-auto max-w-7xl";

export const adminCard =
  "rounded-2xl border border-white/[0.08] bg-white/[0.03] shadow-[0_1px_0_0_rgba(255,255,255,0.05)_inset]";

export const adminCardPad = `${adminCard} p-6`;

export const adminInput =
  "h-10 min-w-[10rem] rounded-xl border border-white/10 bg-white/[0.04] px-3.5 text-sm text-white outline-none transition placeholder:text-slate-500 focus:border-blue-500/40 focus:ring-2 focus:ring-blue-500/15";

export const adminSelect = adminInput;

export const adminTextarea =
  "w-full rounded-xl border border-white/10 bg-white/[0.04] px-3.5 py-2.5 text-sm text-white outline-none transition placeholder:text-slate-500 focus:border-blue-500/40 focus:ring-2 focus:ring-blue-500/15";

export const adminBtnPrimary =
  "inline-flex h-10 items-center justify-center gap-2 rounded-xl bg-blue-600 px-4 text-sm font-semibold text-white shadow-lg shadow-blue-600/20 transition hover:bg-blue-500 disabled:opacity-50";

export const adminBtnSecondary =
  "inline-flex h-10 items-center justify-center gap-2 rounded-xl border border-white/10 bg-white/[0.04] px-4 text-sm font-medium text-slate-200 transition hover:bg-white/[0.08] disabled:opacity-50";

export const adminBtnSuccess =
  "inline-flex items-center justify-center rounded-lg bg-emerald-600/90 px-3 py-1.5 text-sm font-medium text-white hover:bg-emerald-500";

export const adminBtnWarning =
  "inline-flex items-center justify-center rounded-lg bg-amber-600/90 px-3 py-1.5 text-sm font-medium text-white hover:bg-amber-500";

export const adminBtnDanger =
  "inline-flex items-center justify-center rounded-lg bg-rose-600/90 px-3 py-1.5 text-sm font-medium text-white hover:bg-rose-500";

export const adminTableWrap = "overflow-x-auto rounded-2xl border border-white/[0.08] bg-white/[0.02]";

export const adminTable = "w-full text-left text-sm";

export const adminTh = "px-4 py-3 text-xs font-semibold uppercase tracking-wider text-slate-400";

export const adminTr = "border-t border-white/[0.06] transition hover:bg-white/[0.03]";

export const adminTd = "px-4 py-3 text-slate-200";

export const adminBadge = (tone: "blue" | "green" | "amber" | "rose" | "slate" = "slate") => {
  const tones = {
    blue: "bg-blue-500/15 text-blue-300 ring-blue-500/20",
    green: "bg-emerald-500/15 text-emerald-300 ring-emerald-500/20",
    amber: "bg-amber-500/15 text-amber-300 ring-amber-500/20",
    rose: "bg-rose-500/15 text-rose-300 ring-rose-500/20",
    slate: "bg-white/10 text-slate-300 ring-white/10",
  };
  return `inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset ${tones[tone]}`;
};

export const adminAlertInfo = "rounded-xl border border-blue-500/20 bg-blue-500/10 px-4 py-3 text-sm text-blue-200";

export const adminInputFull = `${adminInput} w-full min-w-0`;

export const adminModalOverlay =
  "fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4 backdrop-blur-sm";

export const adminModalPanel =
  "max-h-[90vh] w-full overflow-auto rounded-2xl border border-white/10 bg-[#121a2e] p-6 shadow-2xl";

export const adminListItem = `${adminCard} flex items-center justify-between px-4 py-3`;

export const adminBtnChip =
  "rounded-lg border border-white/10 bg-white/[0.04] px-2.5 py-1 text-xs font-medium text-slate-200 transition hover:bg-white/[0.08]";

export const adminBtnChipDanger =
  "rounded-lg border border-rose-500/20 bg-rose-500/10 px-2.5 py-1 text-xs font-medium text-rose-300 transition hover:bg-rose-500/20";

export const adminPaginationBtn =
  "rounded-xl border border-white/10 bg-white/[0.04] px-3.5 py-1.5 text-sm text-slate-300 transition hover:bg-white/[0.08] disabled:opacity-40";
