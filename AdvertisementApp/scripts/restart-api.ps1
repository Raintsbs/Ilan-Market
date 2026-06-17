# Çalışan API kilidini kaldırıp yeniden başlatır (derleme hatası MSB3027 için)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

Get-Process -Name "AdvertisementApp.API" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" -ErrorAction SilentlyContinue |
  Where-Object { $_.CommandLine -like '*AdvertisementApp.API*' -or $_.CommandLine -like '*AdvertisementApp.UI*' } |
  ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
Get-NetTCPConnection -LocalPort 5050 -ErrorAction SilentlyContinue |
  ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }

Start-Sleep -Seconds 2

Push-Location $root
dotnet build AdvertisementApp.API\AdvertisementApp.API.csproj -c Debug
if ($LASTEXITCODE -ne 0) { Pop-Location; exit $LASTEXITCODE }
Write-Host "API baslatiliyor: http://localhost:5050"
dotnet run --project AdvertisementApp.API --no-build
Pop-Location
