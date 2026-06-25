/** Paylaşılan yüzey ve buton sınıfları — kullanıcı arayüzü. */

const pagePadX = "px-3 sm:px-4 md:px-6 lg:px-8";
const pagePadY = "py-5 sm:py-6 md:py-8";

/** Sayfa genişliği — Bootstrap container yerine saf Tailwind */
export const siteShell = `mx-auto w-full max-w-7xl ${pagePadX}`;

export const siteShellMd = `mx-auto w-full max-w-3xl ${pagePadX}`;

export const siteShellSm = `mx-auto w-full max-w-2xl ${pagePadX}`;

export const surfaceCard =
  "rounded-2xl border border-slate-200/80 bg-white shadow-sm ring-1 ring-slate-900/[0.04] dark:border-slate-700/80 dark:bg-slate-900 dark:ring-white/[0.06]";

/** Kart yüzeyi + mobil uyumlu iç boşluk */
export const surfaceCardPad = `${surfaceCard} p-4 sm:p-6`;

export const surfaceElevated =
  "rounded-2xl border border-slate-200 bg-white shadow-md shadow-slate-900/5 dark:border-slate-700 dark:bg-slate-900 dark:shadow-black/20";

export const btnBrand =
  "inline-flex w-full items-center justify-center gap-1.5 rounded-xl bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-slate-800 active:scale-[0.98] disabled:opacity-50 sm:w-auto dark:bg-blue-600 dark:hover:bg-blue-500";

export const btnBrandSm =
  "inline-flex h-10 w-full items-center justify-center gap-1 rounded-xl bg-slate-900 px-3.5 text-sm font-semibold text-white shadow-sm transition hover:bg-slate-800 active:scale-[0.98] disabled:opacity-50 sm:h-9 sm:w-auto dark:bg-blue-600 dark:hover:bg-blue-500";

export const btnOutline =
  "inline-flex w-full items-center justify-center rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 shadow-sm transition hover:border-slate-300 hover:bg-slate-50 disabled:opacity-50 sm:w-auto dark:border-slate-600 dark:bg-slate-800 dark:text-slate-200 dark:hover:bg-slate-700";

export const inputField =
  "h-10 w-full min-w-0 rounded-xl border border-slate-200 bg-white px-3.5 text-sm font-medium text-slate-950 outline-none transition placeholder:font-normal placeholder:text-slate-500 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-50 dark:placeholder:text-slate-500";

export const pageTitle =
  "text-xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-2xl md:text-3xl";

export const pageSubtitle = "mt-1 text-sm text-slate-600 dark:text-slate-400";

export const pageContainer = `${siteShell} ${pagePadY}`;

export const pageContainerMd = `${siteShellMd} ${pagePadY}`;

export const pageContainerSm = `${siteShellSm} ${pagePadY}`;

export const pageContainerAuth =
  "mx-auto flex min-h-[calc(100vh-8rem)] w-full max-w-md flex-col justify-center px-3 py-8 sm:px-4 sm:py-12";

/** İlan kartı grid — mobil 1, tablet 2, masaüstü 3 kolon */
export const gridAds3 =
  "grid grid-cols-1 gap-4 sm:grid-cols-2 sm:gap-5 lg:grid-cols-3";

/** İlan kartı grid — mobil 1, tablet 2, masaüstü 3–4 kolon */
export const gridAds =
  "grid grid-cols-1 gap-4 sm:grid-cols-2 sm:gap-5 lg:grid-cols-3 xl:grid-cols-4";

/** Yatay aksiyon satırı — mobilde dikey */
export const actionsRow =
  "flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-center sm:gap-3";

/** Tablo taşması için sarmalayıcı */
export const tableWrap = "-mx-3 overflow-x-auto px-3 sm:mx-0 sm:px-0";

export const linkBack =
  "inline-flex items-center gap-1 text-sm font-medium text-slate-500 transition hover:text-blue-600 dark:text-slate-400 dark:hover:text-blue-400";

export const listItemCard =
  "rounded-2xl border border-slate-200/80 bg-white p-4 shadow-sm ring-1 ring-slate-900/[0.03] transition hover:border-blue-200/80 hover:shadow-md hover:shadow-blue-500/5 dark:border-slate-700/80 dark:bg-slate-900 dark:ring-white/[0.04] dark:hover:border-blue-700/60";

export const listItemLink = `${listItemCard} block`;

export const alertWarning =
  "rounded-2xl border border-amber-200 bg-amber-50 px-4 py-6 text-center text-amber-800 sm:px-6 sm:py-8 dark:border-amber-900 dark:bg-amber-950 dark:text-amber-200";

export const alertInfo =
  "rounded-xl border border-blue-200 bg-blue-50/80 p-4 dark:border-blue-900 dark:bg-blue-950/60";

export const categoryCard =
  "group rounded-2xl border border-slate-200/80 bg-white p-4 shadow-sm ring-1 ring-slate-900/[0.04] transition hover:-translate-y-0.5 hover:border-blue-200 hover:shadow-md hover:shadow-blue-500/10 sm:p-6 dark:border-slate-700/80 dark:bg-slate-900 dark:ring-white/[0.06]";
