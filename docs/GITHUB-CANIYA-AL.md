# GitHub → Canlıya alma (Ilan-Market)

Repo: **https://github.com/Raintsbs/Ilan-Market**  
Dal: **`master`** (main değil!)

İki parça deploy edilir:
1. **Vercel** → `frontend/` (Next.js) — kullanıcının gördüğü site  
2. **Railway** → `AdvertisementApp/` (API + SQL Server) — veri ve iş mantığı  

---

## BÖLÜM A — Frontend (Vercel) ~15 dk

### 1. Projeyi bağla
1. [vercel.com/new](https://vercel.com/new) → **Import Git Repository**
2. `Raintsbs/Ilan-Market` seçin
3. **Configure Project** ekranında:

| Ayar | Değer |
|------|--------|
| Framework Preset | Next.js |
| **Root Directory** | `frontend` → Edit → `frontend` seçin |
| **Production Branch** | `master` |

### 2. Ortam değişkeni (şimdilik geçici)
| Name | Value |
|------|--------|
| `NEXT_PUBLIC_API_URL` | `https://PLACEHOLDER` |

API henüz yoksa önce `http://localhost:5050` yazabilirsiniz — site açılır ama ilanlar gelmez. Railway API URL’sini alınca güncelleyeceksiniz.

### 3. Deploy
**Deploy** → bitince `https://ilan-market.vercel.app` veya benzeri URL alırsınız.

**404 DEPLOYMENT_NOT_FOUND** görürseniz: Root Directory `frontend` değilse veya branch `main` seçiliyse olur → Settings’ten düzeltip **Redeploy**.

---

## BÖLÜM B — API + Veritabanı (Railway) ~30 dk

### 1. Proje oluştur
1. [railway.app/new](https://railway.app/new) → **Deploy from GitHub repo**
2. `Ilan-Market` repoyu seçin

### 2. SQL Server servisi
1. Projede **+ New** → **Empty Service**
2. Service adı: `sqlserver`
3. **Settings** → **Source** → **Docker Image**:
   - Image: `mcr.microsoft.com/mssql/server:2022-latest`
4. **Variables**:
   ```
   ACCEPT_EULA=Y
   MSSQL_SA_PASSWORD=GucluSifre123!Railway
   ```
5. **Networking** → bu servise public port **açmayın** (sadece iç ağ)

### 3. API servisi
1. **+ New** → **GitHub Repo** → aynı repo (ikinci servis)
2. Service adı: `api`
3. **Settings**:
   - **Root Directory**: `AdvertisementApp`
   - **Builder**: Dockerfile
   - **Dockerfile Path**: `AdvertisementApp.API/Dockerfile`
4. **Variables** (Railway → api → Variables):

```
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://+:8080

Jwt__Key=BurayaEnAz32KarakterRastgeleBirAnahtarYazin123

ConnectionStrings__DefaultConnection=Server=sqlserver;Database=AdvertisementAppDb;User Id=sa;Password=GucluSifre123!Railway;TrustServerCertificate=True;Encrypt=False;

Storage__Provider=Local
Payments__AllowDemo=true
Sms__Provider=dev
Email__Enabled=true
Email__PickupDirectory=email-pickup
Email__From=noreply@ilanmarket.staging

App__FrontendUrl=https://SIZIN-VERCEL-URL.vercel.app
App__ApiUrl=https://SIZIN-API-URL.up.railway.app
Cors__AllowedOrigins__0=https://SIZIN-VERCEL-URL.vercel.app
```

> `sqlserver` host adı, Railway’de SQL servisinin adıyla aynı olmalı. Farklı ad verdiyseniz connection string’de onu kullanın.

5. **Networking** → **Generate Domain** → örn. `ilan-market-api.up.railway.app`

6. **Deploy** bekleyin → tarayıcıda:
   ```
   https://ilan-market-api.up.railway.app/health
   ```
   `Healthy` dönmeli.

### 4. Vercel’i API’ye bağla
1. Vercel → Project → **Settings** → **Environment Variables**
2. `NEXT_PUBLIC_API_URL` = Railway API URL (`https://...up.railway.app`)
3. **Deployments** → son deploy → **Redeploy**

### 5. Railway CORS’u doğrula
Railway `api` variables’da `App__FrontendUrl` ve `Cors__AllowedOrigins__0` Vercel URL ile aynı olmalı → gerekirse **Redeploy**.

---

## BÖLÜM C — Test

```powershell
cd c:\Projects\advertisement
.\scripts\smoke-test.ps1 -BaseUrl https://SIZIN-API-URL.up.railway.app
```

Tarayıcıda Vercel adresini açın:
- Ana sayfada ilanlar
- `/giris` — kayıt/giriş
- Admin: `admin@advertisement.local` / `Admin1234!`

---

## Sık hatalar

| Sorun | Çözüm |
|--------|--------|
| Vercel 404 DEPLOYMENT_NOT_FOUND | Root=`frontend`, branch=`master`, Redeploy |
| Site açılıyor, veri yok | `vercel.json` içinde API URL var; API redeploy sonrası boşsa `scripts/ensure-production-ads.ps1` çalıştırın. Kalıcı DB için Railway → api → **Volumes** → mount `/app/data` |
| API CORS hatası | `Cors__AllowedOrigins__0` = tam Vercel URL (https, sondaki / yok) |
| API DB bağlanamıyor | SQL şifresi connection string ile aynı mı; host `sqlserver` |
| Railway build fail | Root Directory `AdvertisementApp` mi kontrol edin |

---

## Hocaya mail

`docs/HOCAYA-MAIL-SABLONU.md` — **Vercel URL**’yi yapıştırın.

Ücretsiz planda ilk giriş 10–30 sn sürebilir; mailde belirtin.

---

## Sonraki push’lar

GitHub’a `git push` → Vercel ve Railway otomatik yeniden deploy eder (GitHub bağlıysa).
