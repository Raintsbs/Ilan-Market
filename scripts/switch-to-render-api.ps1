# Render API ayağa kalktıktan sonra Vercel'i kalıcı URL'ye bağlar.
param(
    [string]$ApiUrl = "https://ilan-market-api.onrender.com"
)

$ErrorActionPreference = "Stop"
Write-Host "Health check: $ApiUrl/health"
$health = Invoke-RestMethod -Uri "$ApiUrl/health" -TimeoutSec 60
Write-Host "OK: $health"

$root = Split-Path -Parent $PSScriptRoot
$apiUrlTs = Join-Path $root "frontend\src\lib\apiUrl.ts"
$vercelJson = Join-Path $root "frontend\vercel.json"

$apiUrlContent = @"
/** Canlı API — Render free (Railway trial bitince kalıcı host). */
export const PRODUCTION_API_URL = "$ApiUrl";

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

export function apiExtraHeaders(): Record<string, string> {
  if (API_URL.includes("loca.lt")) {
    return { "Bypass-Tunnel-Reminder": "true" };
  }
  return {};
}
"@

Set-Content -Path $apiUrlTs -Value $apiUrlContent -Encoding utf8
@{
  framework = "nextjs"
  buildCommand = "npm run build"
  installCommand = "npm install"
  env = @{ NEXT_PUBLIC_API_URL = $ApiUrl }
} | ConvertTo-Json | Set-Content -Path $vercelJson -Encoding utf8

Push-Location (Join-Path $root "frontend")
echo $ApiUrl | npx vercel env rm NEXT_PUBLIC_API_URL production --yes 2>$null
echo $ApiUrl | npx vercel env add NEXT_PUBLIC_API_URL production
npx vercel --prod --yes
Pop-Location

Write-Host "Canli API: $ApiUrl"
Write-Host "Site: https://ilan-market.vercel.app"
