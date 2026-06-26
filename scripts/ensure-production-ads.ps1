# Canlı API'de ilan yoksa otomatik doldurur (boş DB / redeploy sonrası).
param(
    [string]$ApiBase = "https://ilan-market-production-b980.up.railway.app"
)

$ErrorActionPreference = "Stop"
$count = (Invoke-RestMethod -Uri "$ApiBase/api/advertisements?page=1&pageSize=1" -TimeoutSec 30).data.totalCount
if ($count -gt 0) {
    Write-Host "OK: $count ilan mevcut."
    exit 0
}

Write-Host "Ilan yok ($count) — seed import calistiriliyor..."
& "$PSScriptRoot\import-local-ads.ps1" -ApiBase $ApiBase
$count = (Invoke-RestMethod -Uri "$ApiBase/api/advertisements?page=1&pageSize=1" -TimeoutSec 30).data.totalCount
Write-Host "Tamam: $count ilan."
