import type { NextConfig } from "next";

const defaultApiUrl = "http://localhost:5050";

function resolveApiUrl(): string {
  const raw = (process.env.NEXT_PUBLIC_API_URL ?? defaultApiUrl).trim();
  try {
    const parsed = new URL(raw);
    return parsed.origin;
  } catch {
    console.warn(
      `[next.config] Invalid NEXT_PUBLIC_API_URL "${raw}" — using ${defaultApiUrl}`,
    );
    return defaultApiUrl;
  }
}

const apiUrl = resolveApiUrl();
const apiHost = new URL(apiUrl);

const nextConfig: NextConfig = {
  output: "standalone",
  images: {
    remotePatterns: [
      {
        protocol: apiHost.protocol.replace(":", "") as "http" | "https",
        hostname: apiHost.hostname,
        port: apiHost.port || undefined,
        pathname: "/uploads/**",
      },
    ],
  },
};

export default nextConfig;
