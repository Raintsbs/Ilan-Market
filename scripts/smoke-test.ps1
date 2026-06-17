# API smoke test — CI veya deploy sonrasi calistirin.
param([string]$BaseUrl = "http://localhost:5050")

$ErrorActionPreference = "Stop"

function Test-Endpoint($path, $expectStatus = 200) {
    $url = "$BaseUrl$path"
    $r = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 15
    if ($r.StatusCode -ne $expectStatus) {
        throw "$url beklenen $expectStatus, gelen $($r.StatusCode)"
    }
    Write-Host "OK $path"
}

Test-Endpoint "/health"
Test-Endpoint "/api/categories/tree"
$swagger = Invoke-WebRequest -Uri "$BaseUrl/swagger/v1/swagger.json" -UseBasicParsing -TimeoutSec 15
if ($swagger.StatusCode -ne 200) { throw "swagger.json failed" }
Write-Host "OK /swagger/v1/swagger.json"
Write-Host "Smoke test basarili."
