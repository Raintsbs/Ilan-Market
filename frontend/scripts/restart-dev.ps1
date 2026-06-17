# Port 3000'deki eski Next.js sürecini kapatıp tek dev sunucusu başlatır
$ErrorActionPreference = "SilentlyContinue"
Get-NetTCPConnection -LocalPort 3000 |
  ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
Start-Sleep -Seconds 1
Set-Location (Split-Path -Parent $PSScriptRoot)
npx next dev
