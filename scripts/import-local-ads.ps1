# Yerel SQL'den export edilen ilanları canlı API'ye aktarır.
param(
    [string]$ApiBase = "https://ilan-market-production-b980.up.railway.app",
    [string]$SeedFile = "$PSScriptRoot\..\AdvertisementApp\scripts\data\seed-advertisements.json"
)

$ErrorActionPreference = "Stop"
$categoryMap = @{ 229 = 186; 243 = 267; 323 = 585 }

function Get-Token($email, $password) {
    $body = @{ email = $email; password = $password } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$ApiBase/api/auth/login" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 30
    if (-not $res.success) { throw "Login failed for $email : $($res.message)" }
    return $res.data.token
}

function New-Ad($token, $ad, $categoryId) {
    $body = @{
        categoryId = $categoryId
        title = $ad.title
        description = $ad.description
        content = $ad.content
        listingDetailsJson = $ad.listingDetailsJson
        listingType = $ad.listingType
    }
    if ($ad.imagePath) { $body.imagePath = $ad.imagePath }
    if ($ad.imagePathsJson) { $body.imagePathsJson = $ad.imagePathsJson }
    $json = $body | ConvertTo-Json -Depth 5 -Compress
    $headers = @{ Authorization = "Bearer $token" }
    $res = Invoke-RestMethod -Uri "$ApiBase/api/advertisements" -Method Post -Body $json -ContentType "application/json" -Headers $headers -TimeoutSec 30
    if (-not $res.success) { throw "Create failed ($($ad.title)): $($res.message)" }
    return $res.data.id
}

function Approve-Ad($adminToken, $id) {
    $headers = @{ Authorization = "Bearer $adminToken" }
    Invoke-RestMethod -Uri "$ApiBase/api/admin/advertisements/$id/approve" -Method Post -Headers $headers -TimeoutSec 30 | Out-Null
}

$ads = Get-Content $SeedFile -Raw | ConvertFrom-Json
$adminToken = Get-Token "admin@advertisement.local" "Admin1234!"
$tokens = @{}

$created = 0
foreach ($ad in $ads) {
    $email = $ad.email
    if (-not $tokens.ContainsKey($email)) {
        try {
            $tokens[$email] = Get-Token $email "123456"
        }
        catch {
            Write-Host "Registering $email ..."
            $reg = @{
                email = $email
                password = "123456"
                firstName = ($email -split '@')[0]
                lastName = "User"
            } | ConvertTo-Json
            Invoke-RestMethod -Uri "$ApiBase/api/auth/register" -Method Post -Body $reg -ContentType "application/json" -TimeoutSec 30 | Out-Null
            $tokens[$email] = Get-Token $email "123456"
        }
    }

    $catId = $categoryMap[[int]$ad.categoryId]
    if (-not $catId) { Write-Warning "Skip $($ad.title): unknown category $($ad.categoryId)"; continue }

    $existing = Invoke-RestMethod -Uri "$ApiBase/api/advertisements?page=1&pageSize=100" -TimeoutSec 30
    $city = $null
    try { $city = ($ad.listingDetailsJson | ConvertFrom-Json).city } catch {}
    $dup = $existing.data.items | Where-Object {
        $_.title -eq $ad.title -and $_.categoryId -eq $catId -and ($null -eq $city -or $_.city -eq $city)
    }
    if ($dup) { Write-Host "Skip (exists): $($ad.title)"; continue }

    $id = New-Ad $tokens[$email] $ad $catId
    if ([int]$ad.status -eq 1 -and [bool]$ad.isActive) {
        Approve-Ad $adminToken $id
    }
    Write-Host "Created: $($ad.title) (id=$id)"
    $created++
}

Write-Host "Done. Created $created ads."
