# SQL Server yedekleme — örnek kullanım:
# .\scripts\backup-database.ps1 -Server ".\SQLEXPRESS" -Database "AdvertisementAppDb"

param(
    [string]$Server = ".\SQLEXPRESS",
    [string]$Database = "AdvertisementAppDb",
    [string]$OutputDir = "$PSScriptRoot\..\backups"
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$file = Join-Path $OutputDir "$Database-$stamp.bak"

Write-Host "Yedek aliniyor: $file"
sqlcmd -S $Server -Q "BACKUP DATABASE [$Database] TO DISK = N'$file' WITH INIT, COMPRESSION"
Write-Host "Tamamlandi."
