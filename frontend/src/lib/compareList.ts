const KEY = "ilanmarket_compare";
const MAX = 4;

export function getCompareIds(): number[] {
  if (typeof window === "undefined") return [];
  try {
    const raw = localStorage.getItem(KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw) as number[];
    return Array.isArray(parsed) ? parsed.filter((n) => typeof n === "number" && n > 0).slice(0, MAX) : [];
  } catch {
    return [];
  }
}

export function isInCompare(id: number): boolean {
  return getCompareIds().includes(id);
}

export function toggleCompare(id: number): number[] {
  if (typeof window === "undefined") return [];
  const current = getCompareIds();
  const next = current.includes(id)
    ? current.filter((x) => x !== id)
    : current.length >= MAX
      ? [...current.slice(1), id]
      : [...current, id];
  localStorage.setItem(KEY, JSON.stringify(next));
  window.dispatchEvent(new CustomEvent("compare-list-changed"));
  return next;
}

export function removeFromCompare(id: number) {
  const next = getCompareIds().filter((x) => x !== id);
  localStorage.setItem(KEY, JSON.stringify(next));
  window.dispatchEvent(new CustomEvent("compare-list-changed"));
}

export function clearCompare() {
  if (typeof window === "undefined") return;
  localStorage.removeItem(KEY);
  window.dispatchEvent(new CustomEvent("compare-list-changed"));
}
