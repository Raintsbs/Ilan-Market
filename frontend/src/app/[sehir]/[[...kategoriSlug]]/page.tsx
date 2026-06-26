import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { SeoLandingView } from "@/components/SeoLandingView";
import type { SeoLanding } from "@/lib/types";
import { API_URL } from "@/lib/apiUrl";
const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

type PageProps = {
  params: Promise<{ sehir: string; kategoriSlug?: string[] }>;
};

async function fetchLanding(city: string, categoryPath?: string): Promise<SeoLanding | null> {
  const q = new URLSearchParams({ city });
  if (categoryPath) q.set("categoryPath", categoryPath);
  try {
    const res = await fetch(`${API_URL}/api/seo/landing?${q}`, { next: { revalidate: 3600 } });
    const body = await res.json();
    if (body.success && body.data) return body.data as SeoLanding;
  } catch {
    /* API offline */
  }
  return null;
}

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { sehir, kategoriSlug } = await params;
  const categoryPath = kategoriSlug?.join("/");
  const landing = await fetchLanding(sehir, categoryPath);
  if (!landing) return { title: "Sayfa bulunamadı" };

  const parts = [landing.cityName];
  if (landing.categoryName) parts.push(landing.categoryName);
  const title = `${parts.join(" ")} İlanları | İlanMarket`;
  const description = `${parts.join(" ")} bölgesinde ${landing.totalCount} ilan. İkinci el ve sıfır ürünler — İlanMarket.`;

  return {
    title,
    description,
    robots: landing.shouldIndex ? { index: true, follow: true } : { index: false, follow: true },
    alternates: {
      canonical: `${SITE_URL}/${sehir}${categoryPath ? `/${categoryPath}` : ""}`,
    },
  };
}

export default async function SeoLandingPage({ params }: PageProps) {
  const { sehir, kategoriSlug } = await params;
  const categoryPath = kategoriSlug?.join("/");
  const landing = await fetchLanding(sehir, categoryPath);
  if (!landing) notFound();

  return <SeoLandingView landing={landing} />;
}
