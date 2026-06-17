# Google OAuth kurulumu — Client ID'yi frontend ve API'ye yazar
param(
    [string]$ClientId = ""
)

$root = Split-Path $PSScriptRoot -Parent
$envFile = Join-Path $root "frontend\.env.local"
$apiSettings = Join-Path $root "AdvertisementApp\AdvertisementApp.API\appsettings.Development.json"

if (-not $ClientId) {
    Write-Host ""
    Write-Host "Google OAuth Client ID kurulumu" -ForegroundColor Cyan
    Write-Host "1. https://console.cloud.google.com/apis/credentials"
    Write-Host "2. Create Credentials -> OAuth client ID -> Web application"
    Write-Host "3. Authorized JavaScript origins: http://localhost:3000"
    Write-Host ""
    $ClientId = Read-Host "Client ID'yi yapistirin (xxx.apps.googleusercontent.com)"
}

$ClientId = $ClientId.Trim()
if (-not $ClientId -or $ClientId -notmatch '\.apps\.googleusercontent\.com$') {
    Write-Host "Gecersiz Client ID. Ornek: 123456-abc.apps.googleusercontent.com" -ForegroundColor Yellow
    exit 1
}

# frontend/.env.local — dizi olarak yaz (tek satir string birlesmesini onler)
$envLines = [System.Collections.Generic.List[string]]::new()
if (Test-Path $envFile) {
    foreach ($line in Get-Content $envFile) {
        if ($line -notmatch '^NEXT_PUBLIC_GOOGLE_CLIENT_ID=' -and $line.Trim().Length -gt 0) {
            if ($line -match '^NEXT_PUBLIC_API_URL=') {
                [void]$envLines.Add("NEXT_PUBLIC_API_URL=http://localhost:5050")
            } else {
                [void]$envLines.Add($line)
            }
        }
    }
}
if (-not ($envLines | Where-Object { $_ -match '^NEXT_PUBLIC_API_URL=' })) {
    [void]$envLines.Add("NEXT_PUBLIC_API_URL=http://localhost:5050")
}
[void]$envLines.Add("NEXT_PUBLIC_GOOGLE_CLIENT_ID=$ClientId")
$envLines | Set-Content $envFile -Encoding utf8NoBOM
Write-Host "Guncellendi: frontend/.env.local" -ForegroundColor Green

# API appsettings.Development.json
if (Test-Path $apiSettings) {
    $json = Get-Content $apiSettings -Raw | ConvertFrom-Json
    if (-not $json.Google) {
        $json | Add-Member -NotePropertyName Google -NotePropertyValue ([PSCustomObject]@{ ClientId = "" }) -Force
    }
    $json.Google.ClientId = $ClientId
    $json | ConvertTo-Json -Depth 10 | Set-Content $apiSettings -Encoding utf8NoBOM
    Write-Host "Guncellendi: AdvertisementApp.API/appsettings.Development.json" -ForegroundColor Green
}

Write-Host ""
Write-Host "Tamam! Frontend dev sunucusunu yeniden baslatin (npm run dev)." -ForegroundColor Cyan
