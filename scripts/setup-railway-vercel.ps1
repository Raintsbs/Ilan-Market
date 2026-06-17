# Railway + Vercel baglanti kurulumu (tek seferlik)
# Kullanim: powershell -ExecutionPolicy Bypass -File scripts/setup-railway-vercel.ps1

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

Write-Host "=== IlanMarket Railway + Vercel kurulumu ===" -ForegroundColor Cyan

# 1. Railway login
Write-Host "`n[1/4] Railway giris (tarayici acilacak)..." -ForegroundColor Yellow
npx @railway/cli login
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`nRailway projesine baglanin (Ilan-Market projesini secin)..." -ForegroundColor Yellow
Push-Location $root
npx @railway/cli link
Pop-Location

# 2. API variables
Write-Host "`n[2/4] API variables yukleniyor..." -ForegroundColor Yellow
$apiVars = Join-Path $PSScriptRoot "railway-api-variables.raw"
if (-not (Test-Path $apiVars)) { throw "Dosya yok: $apiVars" }

Push-Location $root
npx @railway/cli service api 2>$null
Get-Content $apiVars | ForEach-Object {
    $line = $_.Trim()
    if ($line -eq "" -or $line.StartsWith("#")) { return }
    $eq = $line.IndexOf("=")
    if ($eq -lt 1) { return }
    $name = $line.Substring(0, $eq).Trim()
    $value = $line.Substring($eq + 1).Trim()
    Write-Host "  set $name"
    npx @railway/cli variables set "${name}=${value}" 2>&1 | Out-Null
}
Pop-Location

# 3. Domain
Write-Host "`n[3/4] API public domain..." -ForegroundColor Yellow
Push-Location $root
npx @railway/cli domain 2>&1
$apiUrl = (npx @railway/cli domain --json 2>$null | ConvertFrom-Json).domain
if (-not $apiUrl) {
    Write-Host "Domain alinamadi — Railway dashboard: api → Networking → Generate Domain" -ForegroundColor Red
    $apiUrl = Read-Host "API URL'yi elle girin (https://....up.railway.app)"
} else {
    $apiUrl = "https://$apiUrl"
    Write-Host "API URL: $apiUrl" -ForegroundColor Green
}
Pop-Location

# 4. Vercel env
Write-Host "`n[4/4] Vercel NEXT_PUBLIC_API_URL guncelleniyor..." -ForegroundColor Yellow
Push-Location (Join-Path $root "frontend")
echo $apiUrl | npx vercel env add NEXT_PUBLIC_API_URL production --force 2>&1
echo $apiUrl | npx vercel env add NEXT_PUBLIC_API_URL preview --force 2>&1
npx vercel --prod 2>&1
Pop-Location

Write-Host "`n=== Tamam ===" -ForegroundColor Green
Write-Host "Site:  https://ilan-market.vercel.app"
Write-Host "API:   $apiUrl"
Write-Host "Test:  $apiUrl/health"
