# Ilk kurulum: ornek config dosyalarini kopyalar
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent

$prodExample = Join-Path $PSScriptRoot "production.env.example"
$prodTarget = Join-Path $PSScriptRoot "production.env"
if (-not (Test-Path $prodTarget)) {
    Copy-Item $prodExample $prodTarget
    Write-Host "Olusturuldu: scripts/production.env (degerleri doldurun)"
}

$iyzicoExample = Join-Path $root "AdvertisementApp\AdvertisementApp.API\appsettings.Development.local.json.example"
$iyzicoTarget = Join-Path $root "AdvertisementApp\AdvertisementApp.API\appsettings.Development.local.json"
if (-not (Test-Path $iyzicoTarget)) {
    Copy-Item $iyzicoExample $iyzicoTarget
    Write-Host "Olusturuldu: appsettings.Development.local.json (iyzico sandbox anahtarlarini yazin)"
}

Write-Host "Tamam. API'yi yeniden baslatin."
