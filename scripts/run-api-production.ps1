# Production modunda API baslatir. Once production.env dosyasini doldurun.
param(
    [string]$EnvFile = (Join-Path $PSScriptRoot "production.env")
)

$ErrorActionPreference = "Stop"
$apiProject = Join-Path $PSScriptRoot "..\AdvertisementApp\AdvertisementApp.API\AdvertisementApp.API.csproj"

if (-not (Test-Path $EnvFile)) {
    Write-Host "Dosya bulunamadi: $EnvFile"
    Write-Host "Ornek: Copy-Item (Join-Path `$PSScriptRoot 'production.env.example') `$EnvFile"
    exit 1
}

Get-Content $EnvFile | ForEach-Object {
    $line = $_.Trim()
    if ($line -eq "" -or $line.StartsWith("#")) { return }
    $eq = $line.IndexOf("=")
    if ($eq -lt 1) { return }
    $name = $line.Substring(0, $eq).Trim()
    $value = $line.Substring($eq + 1).Trim()
    if ($value.StartsWith('"') -and $value.EndsWith('"')) {
        $value = $value.Substring(1, $value.Length - 2)
    }
    [Environment]::SetEnvironmentVariable($name, $value, "Process")
}

$env:ASPNETCORE_ENVIRONMENT = "Production"
Write-Host "Production ortam degiskenleri yuklendi. API baslatiliyor..."
dotnet run --project $apiProject
