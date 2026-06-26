import type { NextConfig } from "next";
import { API_URL, PRODUCTION_API_URL } from "./src/lib/apiUrl";

const defaultApiUrl = process.env.VERCEL === "1" ? PRODUCTION_API_URL : "http://localhost:5050";

function resolveApiUrl(): string {
  try {
    return new URL(API_URL).origin;
  } catch {
    console.warn(`[next.config] Invalid API URL "${API_URL}" — using ${defaultApiUrl}`);
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
