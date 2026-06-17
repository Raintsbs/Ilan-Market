$ErrorActionPreference = "Stop"
$r = Invoke-RestMethod "https://turkiyeapi.dev/api/v1/provinces"
$provinces = foreach ($p in $r.data) {
    @{
        id        = $p.id
        name      = $p.name
        plate     = "{0:D2}" -f $p.id
        districts = @(
            foreach ($d in $p.districts) {
                @{ id = $d.id; name = $d.name }
            }
        )
    }
}
$out = @{ provinces = $provinces }
$path = Join-Path $PSScriptRoot "data\turkey-locations.json"
New-Item -ItemType Directory -Force -Path (Split-Path $path) | Out-Null
$out | ConvertTo-Json -Depth 6 | Set-Content $path -Encoding UTF8
Write-Host "Saved $($provinces.Count) provinces to $path"
