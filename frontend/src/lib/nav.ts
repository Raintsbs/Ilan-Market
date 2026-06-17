import type { MessageKey } from "@/lib/i18n/messages";

export interface NavLink {
  href: string;
  labelKey: MessageKey;
  auth?: boolean;
}

/** lg+ üst barda; 2xl’de secondary linkler de görünür */
export const desktopPrimaryNavLinks: NavLink[] = [
  { href: "/", labelKey: "nav.ads" },
  { href: "/kategoriler", labelKey: "nav.categories" },
  { href: "/harita", labelKey: "nav.map" },
  { href: "/mesajlar", labelKey: "nav.messages", auth: true },
  { href: "/teklifler", labelKey: "nav.offers", auth: true },
  { href: "/blog", labelKey: "nav.blog" },
];

/** 2xl+ üst barda; dar ekranda hamburger menüde */
export const desktopSecondaryNavLinks: NavLink[] = [
  { href: "/favorilerim", labelKey: "nav.favorites", auth: true },
  { href: "/ilanlarim", labelKey: "nav.myAds", auth: true },
];

/** Hamburger menü — Siparişler İlanlarım sayfasındaki butondan da açılır */
export const mainNavLinks: NavLink[] = [
  ...desktopPrimaryNavLinks,
  { href: "/siparisler", labelKey: "nav.orders", auth: true },
  ...desktopSecondaryNavLinks,
  { href: "/hesabim", labelKey: "nav.account", auth: true },
];

export function getVisibleNavLinks(isAuthenticated: boolean, links: NavLink[] = mainNavLinks): NavLink[] {
  return links.filter((l) => !l.auth || isAuthenticated);
}
