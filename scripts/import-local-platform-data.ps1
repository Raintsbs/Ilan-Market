# Favoriler, teklif ve mesaj geçmişini canlı API'ye aktarır.
param(
    [string]$ApiBase = "https://ilan-market-production-b980.up.railway.app"
)

$ErrorActionPreference = "Stop"

function Get-Token($email, $password) {
    $body = @{ email = $email; password = $password } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "$ApiBase/api/auth/login" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 30
    if (-not $res.success) { throw "Login failed for ${email}: $($res.message)" }
    return $res.data.token
}

function Ensure-User($email, $firstName, $lastName, $password = "123456") {
    try {
        return Get-Token $email $password
    }
    catch {
        $reg = @{
            email = $email
            password = $password
            firstName = $firstName
            lastName = $lastName
        } | ConvertTo-Json
        Invoke-RestMethod -Uri "$ApiBase/api/auth/register" -Method Post -Body $reg -ContentType "application/json" -TimeoutSec 30 | Out-Null
        return Get-Token $email $password
    }
}

function Send-ThreadMessage($token, $threadId, $adId, $body) {
    $payload = @{ threadId = $threadId; advertisementId = $adId; body = $body } | ConvertTo-Json
    $headers = @{ Authorization = "Bearer $token" }
    $res = Invoke-RestMethod -Uri "$ApiBase/api/messages" -Method Post -Body $payload -ContentType "application/json" -Headers $headers -TimeoutSec 30
    if (-not $res.success) { throw "Message failed: $($res.message)" }
}

# Production eşlemeleri (yerel SQL export)
$favoriteAdId = 2          # gfd / Oyun & Konsol / Diyarbakır
$threadAdId = 11           # 4 / Bilgisayar / Eskişehir

$tahaToken = Ensure-User "tahatokay2006@gmail.com" "Taha" "Tokay"
$egeToken = Ensure-User "Ege@gmail.com" "Ege" "arslan"

# --- Favori ---
$favHeaders = @{ Authorization = "Bearer $tahaToken" }
$favStatus = Invoke-RestMethod -Uri "$ApiBase/api/favorites/$favoriteAdId/status" -Headers $favHeaders -TimeoutSec 30
if (-not $favStatus.data) {
    Invoke-RestMethod -Uri "$ApiBase/api/favorites/$favoriteAdId/toggle" -Method Post -Headers $favHeaders -TimeoutSec 30 | Out-Null
    Write-Host "Favorite added: ad $favoriteAdId (gfd)"
}
else {
    Write-Host "Favorite already exists: ad $favoriteAdId"
}

# --- Mesajlar + teklif ---
$egeHeaders = @{ Authorization = "Bearer $egeToken" }
$tahaHeaders = @{ Authorization = "Bearer $tahaToken" }

$existingThreads = Invoke-RestMethod -Uri "$ApiBase/api/messages/threads" -Headers $tahaHeaders -TimeoutSec 30
$thread = $existingThreads.data | Where-Object { $_.advertisementId -eq $threadAdId } | Select-Object -First 1

if ($thread) {
    $threadId = $thread.id
    Write-Host "Thread already exists: $threadId"
}
else {
    $offerBody = @{ advertisementId = $threadAdId; amount = 232 } | ConvertTo-Json
    $offerRes = Invoke-RestMethod -Uri "$ApiBase/api/offers" -Method Post -Body $offerBody -ContentType "application/json" -Headers $egeHeaders -TimeoutSec 30
    if (-not $offerRes.success) { throw "Offer create failed: $($offerRes.message)" }
    Write-Host "Offer created: id=$($offerRes.data.id)"

    $threadsAfter = Invoke-RestMethod -Uri "$ApiBase/api/messages/threads" -Headers $tahaHeaders -TimeoutSec 30
    $thread = $threadsAfter.data | Where-Object { $_.advertisementId -eq $threadAdId } | Select-Object -First 1
    if (-not $thread) { throw "Thread not found after offer" }
    $threadId = $thread.id
    Write-Host "Thread created: $threadId"
}

$chatMessages = @(
    @{ token = $egeToken; body = "merhaba" },
    @{ token = $tahaToken; body = "greg" },
    @{ token = $tahaToken; body = "grgre" },
    @{ token = $tahaToken; body = "grgrgr" },
    @{ token = $tahaToken; body = "fgfgfg" },
    @{ token = $tahaToken; body = "www" },
    @{ token = $egeToken; body = "bfb" },
    @{ token = $tahaToken; body = "fdfd" },
    @{ token = $tahaToken; body = "fdfd" },
    @{ token = $tahaToken; body = "ngffg" }
)

$existingMsgs = Invoke-RestMethod -Uri "$ApiBase/api/messages/threads/$threadId" -Headers $tahaHeaders -TimeoutSec 30
$existingBodies = @($existingMsgs.data | ForEach-Object { $_.body })

$sent = 0
foreach ($m in $chatMessages) {
    if ($existingBodies -contains $m.body) { continue }
    Send-ThreadMessage $m.token $threadId $threadAdId $m.body
    $sent++
}

Write-Host "Chat messages sent: $sent"

# Teklifi reddet (yerelde reddedilmişti)
$incoming = Invoke-RestMethod -Uri "$ApiBase/api/offers/incoming" -Headers $tahaHeaders -TimeoutSec 30
$pendingOffer = $incoming.data | Where-Object { $_.advertisementId -eq $threadAdId -and $_.status -eq 0 } | Select-Object -First 1
if ($pendingOffer) {
    Invoke-RestMethod -Uri "$ApiBase/api/offers/$($pendingOffer.id)/reject" -Method Post -Headers $tahaHeaders -TimeoutSec 30 | Out-Null
    Write-Host "Offer $($pendingOffer.id) rejected"
}
else {
    Write-Host "No pending offer to reject (already handled)"
}

# Doğrulama
$favs = Invoke-RestMethod -Uri "$ApiBase/api/favorites" -Headers $favHeaders -TimeoutSec 30
$finalMsgs = Invoke-RestMethod -Uri "$ApiBase/api/messages/threads/$threadId" -Headers $tahaHeaders -TimeoutSec 30
Write-Host "Favorites count: $($favs.data.Count)"
Write-Host "Messages in thread: $($finalMsgs.data.Count)"
