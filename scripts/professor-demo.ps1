# Hocaya gostermek icin API + frontend baslatir. Opsiyonel: ngrok / cloudflared tunel.
param(
    [switch]$Tunnel,
    [string]$TunnelTool = "auto"  # auto | ngrok | cloudflared
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$apiDir = Join-Path $root "AdvertisementApp\AdvertisementApp.API"
$feDir = Join-Path $root "frontend"

function Stop-Port($port) {
    Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue |
        ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
}

Write-Host "=== IlanMarket demo baslatiliyor ===" -ForegroundColor Cyan

Stop-Port 5050
Stop-Port 3000
Start-Sleep -Seconds 1

Push-Location (Join-Path $root "AdvertisementApp")
dotnet ef database update --project AdvertisementApp.DataAccess --startup-project AdvertisementApp.API | Out-Null
Pop-Location

$apiJob = Start-Job -ScriptBlock {
    Set-Location $using:apiDir
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    dotnet run 2>&1
}
Start-Sleep -Seconds 8

Push-Location $feDir
if (-not (Test-Path "node_modules")) { npm install | Out-Null }
$feProc = Start-Process -FilePath "npm" -ArgumentList "run","dev" -PassThru -WindowStyle Hidden
Pop-Location
Start-Sleep -Seconds 5

try {
    $health = Invoke-WebRequest -Uri "http://localhost:5050/health" -UseBasicParsing -TimeoutSec 10
    Write-Host "API:  http://localhost:5050  (health $($health.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "API baslamadi. Job log:" -ForegroundColor Red
    Receive-Job $apiJob -Keep | Select-Object -Last 20
    exit 1
}

try {
    $fe = Invoke-WebRequest -Uri "http://localhost:3000" -UseBasicParsing -TimeoutSec 10
    Write-Host "Web:  http://localhost:3000  ($($fe.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "Frontend henuz hazir degil — birkaç saniye bekleyip http://localhost:3000 acin." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Demo hesap: admin@advertisement.local / Admin1234!" -ForegroundColor DarkGray
Write-Host "Smoke: .\scripts\smoke-test.ps1" -ForegroundColor DarkGray

if ($Tunnel) {
    $tool = $TunnelTool
    if ($tool -eq "auto") {
        if (Get-Command ngrok -ErrorAction SilentlyContinue) { $tool = "ngrok" }
        elseif (Get-Command cloudflared -ErrorAction SilentlyContinue) { $tool = "cloudflared" }
        else {
            Write-Host "ngrok veya cloudflared bulunamadi. Kurulum:" -ForegroundColor Yellow
            Write-Host "  winget install ngrok.ngrok"
            Write-Host "  veya: winget install Cloudflare.cloudflared"
            exit 0
        }
    }
    if ($tool -eq "ngrok") {
        Write-Host "Frontend tuneli (3000) — bu URL'yi hocaya gonderin:" -ForegroundColor Cyan
        Start-Process ngrok -ArgumentList "http","3000"
    } elseif ($tool -eq "cloudflared") {
        Write-Host "Frontend tuneli (3000):" -ForegroundColor Cyan
        cloudflared tunnel --url http://localhost:3000
    }
}

Write-Host ""
Write-Host "Durdurmak icin: Get-Job | Stop-Job; Get-Process node,dotnet -ErrorAction SilentlyContinue | Where-Object {`$_.Path -like '*advertisement*'} | Stop-Process -Force"
