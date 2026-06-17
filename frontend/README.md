# İlanMarket — Next.js Frontend

AdvertisementApp REST API (`http://localhost:5050`) ile çalışan modern ilan arayüzü.

## Gereksinimler

- Node.js 18+
- Çalışan API: `AdvertisementApp.API`

## Kurulum

```powershell
cd c:\Projects\advertisement\frontend
npm install
cp .env.example .env.local   # gerekirse API URL düzenleyin
```

## Çalıştırma

**1. Backend API** (ayrı terminal):

```powershell
cd c:\Projects\advertisement\AdvertisementApp
dotnet run --project AdvertisementApp.API
```

Swagger: http://localhost:5050/swagger

**2. Frontend**:

```powershell
cd c:\Projects\advertisement\frontend
npm run dev
```

Tarayıcı: http://localhost:3000

## Sayfalar

| Sayfa | Açıklama |
|--------|----------|
| `/` | İlan listesi, arama ve kategori filtresi |
| `/ilan/[id]` | İlan detayı |
| `/ilan/yeni` | Yeni ilan (giriş gerekli) |
| `/ilan/[id]/duzenle` | İlan düzenleme |
| `/ilanlarim` | Kullanıcının ilanları |
| `/giris` | JWT giriş |
| `/kayit` | Kayıt |

## API endpoint’leri

- `POST /api/auth/login`, `POST /api/auth/register`, `GET /api/auth/me`
- `GET /api/advertisements`, `GET /api/advertisements/my`, `GET /api/advertisements/{id}`
- `POST /api/advertisements`, `POST /api/advertisements/with-image`
- `PUT /api/advertisements/{id}`, `DELETE /api/advertisements/{id}`
- `GET /api/categories`
