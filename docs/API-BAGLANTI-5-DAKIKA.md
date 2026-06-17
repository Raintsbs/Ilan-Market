# 5 dakikada API bağlantısı (kopyala-yapıştır)

Vercel site: **https://ilan-market.vercel.app** (hazır)

API için Railway’de **2 servis** + **Variables yapıştır** + **Vercel env** yeterli.

---

## ADIM 1 — Railway’de sqlserver

1. [railway.app](https://railway.app) → projeniz → **+ New** → **Empty Service**
2. Ad: `sqlserver`
3. Settings → **Docker Image**: `mcr.microsoft.com/mssql/server:2022-latest`
4. **Variables** → RAW Editor → `scripts/railway-sqlserver-variables.raw` içeriğini yapıştır:

```
ACCEPT_EULA=Y
MSSQL_SA_PASSWORD=GucluSifre123!Railway
```

5. Deploy bitsin (yeşil)

---

## ADIM 2 — Railway’de api

1. Aynı projede **api** servisi (GitHub repo Ilan-Market)
2. Settings:
   - Root Directory: **boş** (repo kökü)
   - Builder: **Dockerfile**
3. **Variables** → RAW Editor → `scripts/railway-api-variables.raw` dosyasının **tamamını** yapıştır

4. **Networking** → **Generate Domain** → URL kopyala  
   Örnek: `https://ilan-market-production-xxxx.up.railway.app`

5. Deploy **Success** olunca tarayıcıda: `https://API-URL/health`

---

## ADIM 3 — Vercel’e API adresini yaz

1. [vercel.com](https://vercel.com) → **ilan-market** → Settings → Environment Variables
2. `NEXT_PUBLIC_API_URL` = Railway API URL (https ile, sonda / yok)
3. **Deployments** → **Redeploy**

---

## Otomatik (Railway giriş yaptıktan sonra)

Terminalde bir kez:

```powershell
npx @railway/cli login
powershell -ExecutionPolicy Bypass -File scripts/setup-railway-vercel.ps1
```

---

## Kontrol

- https://ilan-market.vercel.app → ilanlar görünmeli
- F12 → Network → istekler Railway API’ye gitmeli (localhost değil)

Takılırsanız Railway **api → Deployments → View logs** son 20 satırı gönderin.
