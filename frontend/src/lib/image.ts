import { API_URL } from "./api";

export function getImageUrl(imagePath?: string | null): string | null {
  if (!imagePath) return null;
  if (imagePath.startsWith("http")) return imagePath;
  return `${API_URL}${imagePath.startsWith("/") ? "" : "/"}${imagePath}`;
}

export function getMediaUrl(path?: string | null): string | null {
  return getImageUrl(path);
}

export function getVideoUrl(path?: string | null): string | null {
  return getMediaUrl(path);
}

export function getAdvertisementImageUrls(ad: {
  imagePath?: string | null;
  imagePaths?: string[];
}): string[] {
  if (ad.imagePaths?.length) {
    return ad.imagePaths.map((p) => getImageUrl(p)).filter((u): u is string => !!u);
  }
  const single = getImageUrl(ad.imagePath);
  return single ? [single] : [];
}
