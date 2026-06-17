# İlanMarket — hocaya 7/24 açık demo (Railway + Vercel)

Hocanın istediği zaman girebilmesi için bilgisayarınızın açık kalması gerekmez. **Frontend Vercel**, **API + SQL Railway** üzerinde sürekli çalışır.

Tahmini süre: **45–60 dakika** (ilk kez).

---

## 1. Kodu GitHub’a yükleyin

```powershell
cd c:\Projects\advertisement
git init
git add .
git commit -m "İlanMarket proje teslimi"
# GitHub’da yeni repo oluşturun, sonra:
git remote add origin https://github.com/KULLANICI/ilanmarket.git
git push -u origin main
```

---

## 2. API + veritabanı (Railway)

1. [railway.app](https://railway.app) → GitHub ile giriş
2. **New Project** → **Deploy from GitHub repo** → bu repoyu seçin
3. **Add Service** → **Database** → **SQL Server** (veya Docker template)
4. API servisi için **Settings**:
   - Root Directory: `AdvertisementApp`
   - Dockerfile: `AdvertisementApp.API/Dockerfile`
5. **Variables** (Staging — demo için yeterli):

| Değişken | Örnek |
|----------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Staging` |
| `Jwt__Key` | 32+ karakter rastgele |
| `ConnectionStrings__DefaultConnection` | Railway SQL connection string |
| `App__FrontendUrl` | Vercel URL (adım 3’ten sonra güncelle) |
| `App__ApiUrl` | `https://xxx.up.railway.app` |
| `Cors__AllowedOrigins__0` | Vercel URL |
| `Storage__Provider` | `Local` |
| `Payments__AllowDemo` | `true` |
| `Sms__Provider` | `dev` |

6. Deploy bitince **Settings → Networking → Generate Domain** → API URL’yi kopyalayın
7. `GET https://API-URL/health` → `Healthy` olmalı

Şablon: `scripts/staging-student.env.example`

---

## 3. Frontend (Vercel)

1. [vercel.com](https://vercel.com) → GitHub ile giriş
2. **Add New Project** → repo → **Root Directory: `frontend`**
3. Environment variable:
   - `NEXT_PUBLIC_API_URL` = Railway API URL (https://...)
4. Deploy → `https://xxx.vercel.app`

5. Railway’de `App__FrontendUrl` ve `Cors__AllowedOrigins__0` değerlerini Vercel URL ile güncelleyin → API’yi redeploy edin.

---

## 4. Smoke test

```powershell
.\scripts\smoke-test.ps1 -BaseUrl https://API-URL
```

Tarayıcıda Vercel adresini açın → kayıt / ilan listesi çalışmalı.

**Demo admin:** `admin@advertisement.local` / `Admin1234!`

---

## 5. Hocaya mail

Metin: `docs/HOCAYA-MAIL-SABLONU.md` — Vercel linkini yapıştırıp gönderin.

---

## Alternatif: Tek VPS (Docker)

Sunucunuz varsa:

```bash
cd AdvertisementApp
docker compose up -d
# frontend ayri build + nginx reverse proxy
```

---

## Notlar

- `AdvertisementApp.UI` deploy **etmeyin** — sadece `frontend` + `AdvertisementApp.API`
- Tam production (`Production` env) için Azure Blob, Netgsm, iyzico gerekir — öğrenci demosu için `Staging` yeterli
- Ücretsiz planlarda Railway/Vercel uyku moduna girebilir; ilk istek 10–30 sn sürebilir (hocaya mailde belirtin)
