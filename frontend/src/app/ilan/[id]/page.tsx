import type { Metadata } from "next";
import { AdDetailClient } from "./AdDetailClient";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5050";
const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

type PageProps = {
  params: Promise<{ id: string }>;
};

type AdMeta = {
  title: string;
  description?: string;
  listingDetails?: { price?: number };
  imagePath?: string;
  categoryName?: string;
};

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { id } = await params;
  try {
    const res = await fetch(`${API_URL}/api/advertisements/${id}`, {
      next: { revalidate: 300 },
    });
    const body = await res.json();
    if (body.success && body.data) {
      const ad = body.data as AdMeta;
      const description = (ad.description ?? ad.title).slice(0, 160);
      return {
        title: `${ad.title} | İlanMarket`,
        description,
        openGraph: {
          title: ad.title,
          description,
          url: `${SITE_URL}/ilan/${id}`,
          type: "website",
        },
      };
    }
  } catch {
    /* fallback */
  }
  return { title: "İlan | İlanMarket" };
}

export default async function AdDetailPage({ params }: PageProps) {
  const { id } = await params;
  let jsonLd: object | null = null;

  try {
    const res = await fetch(`${API_URL}/api/advertisements/${id}`, { next: { revalidate: 300 } });
    const body = await res.json();
    if (body.success && body.data) {
      const ad = body.data as AdMeta;
      jsonLd = {
        "@context": "https://schema.org",
        "@type": "Product",
        name: ad.title,
        description: ad.description,
        category: ad.categoryName,
        offers: ad.listingDetails?.price
          ? {
              "@type": "Offer",
              price: ad.listingDetails.price,
              priceCurrency: "TRY",
              availability: "https://schema.org/InStock",
            }
          : undefined,
        image: ad.imagePath ? `${SITE_URL}${ad.imagePath.startsWith("/") ? "" : "/"}${ad.imagePath}` : undefined,
      };
    }
  } catch {
    /* ignore */
  }

  return (
    <>
      {jsonLd && (
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }}
        />
      )}
      <AdDetailClient id={Number(id)} />
    </>
  );
}
