/** Canlı API — Vercel env boş kalsa bile site bu adrese bağlanır. */
export const PRODUCTION_API_URL = "https://ilan-market-production-b980.up.railway.app";

const LOCAL_API_URL = "http://localhost:5050";

function normalizeOrigin(raw: string): string | null {
  const trimmed = raw.trim();
  if (!trimmed) return null;
  try {
    return new URL(trimmed).origin;
  } catch {
    return null;
  }
}

/** NEXT_PUBLIC_* build-time; Vercel'de env yoksa production API kullanılır. */
export function resolveApiUrl(): string {
  const fromEnv = normalizeOrigin(process.env.NEXT_PUBLIC_API_URL ?? "");
  if (fromEnv) return fromEnv;

  if (process.env.VERCEL === "1") return PRODUCTION_API_URL;

  return LOCAL_API_URL;
}

export const API_URL = resolveApiUrl();
