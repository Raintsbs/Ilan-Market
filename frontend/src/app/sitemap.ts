import type { MetadataRoute } from "next";
import { API_URL } from "@/lib/apiUrl";
const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

async function fetchJson(url: string, revalidate: number) {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), 5000);
  try {
    const res = await fetch(url, {
      signal: controller.signal,
      next: { revalidate },
    });
    if (!res.ok) return null;
    return await res.json();
  } catch {
    return null;
  } finally {
    clearTimeout(timer);
  }
}

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const staticRoutes: MetadataRoute.Sitemap = [
    { url: SITE_URL, changeFrequency: "daily", priority: 1 },
    { url: `${SITE_URL}/kategoriler`, changeFrequency: "weekly", priority: 0.8 },
    { url: `${SITE_URL}/harita`, changeFrequency: "weekly", priority: 0.7 },
    { url: `${SITE_URL}/blog`, changeFrequency: "weekly", priority: 0.6 },
  ];

  const routes: MetadataRoute.Sitemap = [...staticRoutes];

  try {
    const seoBody = await fetchJson(`${API_URL}/api/seo/sitemap-entries?max=300`, 86400);
    if (seoBody?.success && Array.isArray(seoBody.data)) {
      for (const entry of seoBody.data as { citySlug: string; categoryPath: string }[]) {
        routes.push({
          url: `${SITE_URL}/${entry.citySlug}/${entry.categoryPath}`,
          changeFrequency: "daily",
          priority: 0.7,
        });
      }
    }
  } catch {
    /* API offline */
  }

  try {
    const body = await fetchJson(`${API_URL}/api/advertisements?page=1&pageSize=200`, 3600);
    if (body?.success && body.data?.items) {
      const listingRoutes = body.data.items.map((ad: { id: number }) => ({
        url: `${SITE_URL}/ilan/${ad.id}`,
        changeFrequency: "daily" as const,
        priority: 0.9,
      }));
      return [...routes, ...listingRoutes];
    }
  } catch {
    /* API offline */
  }

  return routes;
}
