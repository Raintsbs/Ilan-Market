# Advertisement — İlan Platformu

Monorepo yapısı:

| Klasör | Teknoloji | Açıklama |
|--------|-----------|----------|
| `AdvertisementApp/` | ASP.NET Core 8 | REST API (JWT), EF Core |
| `frontend/` | Next.js 16 + Tailwind | **Birincil web arayüzü** |
| `AdvertisementApp.UI/` | ASP.NET MVC | **Deprecated** — yeni geliştirme yapmayın |

## Faz 1 kurulum (tek seferlik)

```powershell
.\scripts\setup-local-config.ps1
# appsettings.Development.local.json → iyzico sandbox anahtarlari
# scripts/production.env → production degerleri (Azure, SMTP, Netgsm)
```

iyzico local callback: `ngrok http 5050` → `App__ApiUrl` = ngrok HTTPS URL

## Production checklist

1. `scripts/production.env.example` dosyasini `scripts/production.env` olarak kopyalayin ve doldurun.
2. Production API: `.\scripts\run-api-production.ps1`
3. Eksik ayar varsa API baslamaz — `ProductionConfigurationValidator` hata verir.

### Zorunlu ortam degiskenleri

| Degisken | Aciklama |
|----------|----------|
| `Jwt__Key` | En az 32 karakter |
| `ConnectionStrings__DefaultConnection` | SQL Server |
| `Storage__AzureConnectionString` | Azure Blob (Provider=AzureBlob) |
| `Email__SmtpHost`, `Email__Username`, `Email__Password` | SMTP |
| `Sms__Provider=netgsm` + `Sms__Netgsm__*` | Netgsm SMS |
| `Iyzico__ApiKey`, `Iyzico__SecretKey` | Turkiye odemesi (tercih edilen) |
| `Payments__AllowDemo=false` | Demo odeme kapali |
| `Seed__RunCategoryCatalog=false` | Kategori sync icin tek seferlik `true` |

Yedek: `.\scripts\backup-database.ps1`  
Smoke test: `.\scripts\smoke-test.ps1`  
Health: `GET /health`

### Odeme entegrasyonu

Kod hazir — production'da anahtarlari `production.env` ile verin:

| Saglayici | Kullanim | Endpoint'ler |
|-----------|----------|--------------|
| **iyzico** (varsayilan) | One cikan paket + Param Guvende escrow | `POST /api/payments/checkout`, iyzico callback |
| **Stripe** | Alternatif | Checkout Session + `POST /api/payments/stripe-webhook` |
| **Demo** | Yalnizca dev/staging (`Payments__AllowDemo=true`) | Production'da kapali |

Production iyzico API: `Iyzico__BaseUrl=https://api.iyzipay.com` (sandbox degil).

## Hızlı başlangıç

### 1. Veritabanı ve API

```powershell
cd AdvertisementApp
dotnet ef database update --project AdvertisementApp.DataAccess --startup-project AdvertisementApp.API
dotnet run --project AdvertisementApp.API
```

API: http://localhost:5050 — Swagger: http://localhost:5050/swagger

### 2. Frontend

```powershell
cd frontend
npm install
npm run dev
```

Uygulama: http://localhost:3000

`.env.local` içinde `NEXT_PUBLIC_API_URL=http://localhost:5050` olmalı (varsayılan hazır).

### Google ile giriş / kayıt (bir kez)

1. [Google Cloud Console](https://console.cloud.google.com/apis/credentials) → **OAuth client ID** (Web)
2. **Authorized JavaScript origins:** `http://localhost:3000`
3. Proje kökünde:

```powershell
.\scripts\setup-google-oauth.ps1
```

Client ID'yi yapıştırın, ardından `npm run dev` yeniden başlatın.

Alternatif: `frontend/.env.local` → `NEXT_PUBLIC_GOOGLE_CLIENT_ID=...` veya API `appsettings.Development.json` → `Google:ClientId`.

### iyzico sandbox (gercek odeme testi)

1. [merchant.iyzipay.com](https://merchant.iyzipay.com) → Sandbox API anahtarlari
2. Ornek dosyayi kopyalayin:

```powershell
Copy-Item AdvertisementApp\AdvertisementApp.API\appsettings.Development.local.json.example `
  AdvertisementApp\AdvertisementApp.API\appsettings.Development.local.json
```

3. `Iyzico:ApiKey` ve `Iyzico:SecretKey` degerlerini yazin, API'yi yeniden baslatin.
4. Odeme artik iyzico sayfasina yonlendirir (demo degil).


- Admin: `admin@advertisement.local` / `Admin1234!`

### Kategori kataloğu güncelleme

Production'da her restart'ta çalışmaz. Geliştirmede `Seed:RunCategoryCatalog=true` (Development varsayılan). Manuel:

```powershell
$env:Seed__RunCategoryCatalog="true"
dotnet run --project AdvertisementApp.API
```

## Frontend sayfaları

- `/` — İlan listesi
- `/ilan/[id]` — Detay
- `/ilan/yeni`, `/ilan/[id]/duzenle` — Oluştur / düzenle
- `/ilanlarim` — Kullanıcı ilanları
- `/giris`, `/kayit` — Kimlik doğrulama (JWT)

Detaylar için `frontend/README.md` ve `AdvertisementApp/README.md`.
