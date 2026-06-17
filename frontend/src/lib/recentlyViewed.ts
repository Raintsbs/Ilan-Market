const KEY = "ilanmarket_recent";
const MAX = 12;

export type RecentAdSnapshot = {
  id: number;
  title: string;
  imagePath?: string;
  price?: number | null;
  viewedAt: string;
};

export function getRecentlyViewed(): RecentAdSnapshot[] {
  if (typeof window === "undefined") return [];
  try {
    const raw = localStorage.getItem(KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw) as RecentAdSnapshot[];
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

export function addRecentlyViewed(ad: Omit<RecentAdSnapshot, "viewedAt">) {
  if (typeof window === "undefined" || !ad.id) return;
  const list = getRecentlyViewed().filter((x) => x.id !== ad.id);
  list.unshift({ ...ad, viewedAt: new Date().toISOString() });
  localStorage.setItem(KEY, JSON.stringify(list.slice(0, MAX)));
  window.dispatchEvent(new CustomEvent("recently-viewed-changed"));
}

export function clearRecentlyViewed() {
  if (typeof window === "undefined") return;
  localStorage.removeItem(KEY);
  window.dispatchEvent(new CustomEvent("recently-viewed-changed"));
}
