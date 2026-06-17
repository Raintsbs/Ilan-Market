# Vercel: DEPLOYMENT_NOT_FOUND düzeltmesi

`ilanmarket.vercel.app` → **404 DEPLOYMENT_NOT_FOUND** = bu domaine bağlı **hiç başarılı deploy yok** (proje silinmiş, build patlamış veya repo bağlanmamış).

## Hızlı çözüm (önerilen)

### 1. Vercel dashboard
1. [vercel.com/dashboard](https://vercel.com/dashboard) → **ilanmarket** projesi var mı?
   - **Yoksa:** Add New → Project → GitHub repo veya **Upload** / CLI deploy
   - **Varsa:** **Deployments** sekmesi → son deploy **Failed** mı? Log’a bakın

### 2. Proje ayarları (kritik)
| Ayar | Değer |
|------|--------|
| **Root Directory** | `frontend` |
| **Framework** | Next.js |
| **Build Command** | `npm run build` |
| **Output** | (otomatik — `output: standalone` Vercel’de sorun çıkarırsa kaldırın) |

### 3. Environment Variables
| Key | Value |
|-----|--------|
| `NEXT_PUBLIC_API_URL` | Railway/API URL (örn. `https://xxx.up.railway.app`) |

API yoksa geçici: `http://localhost:5050` — sadece UI açılır, veri gelmez.

### 4. Redeploy
Deployments → **Redeploy** veya yeni commit push.

Domain **Settings → Domains** altında `ilanmarket.vercel.app` **Production** deployment’a bağlı olmalı.

---

## CLI ile deploy (GitHub yoksa)

```powershell
cd c:\Projects\advertisement\frontend
npx vercel login
npx vercel --prod
```

İlk soruda:
- Link to existing project? → **Y** → `ilanmarket` seçin  
- Yoksa yeni proje adı: `ilanmarket`

---

## Build yerelde test

```powershell
cd frontend
npm run build
```

Başarılıysa sorun %99 Vercel proje/root/env ayarıdır.

---

## API henüz yoksa

Sadece frontend deploy ederseniz site açılır ama ilanlar yüklenmez. Sıra:
1. Railway’de API + SQL (`docs/CANIYA-ALMA-HOCA.md`)
2. `NEXT_PUBLIC_API_URL` güncelle → Vercel redeploy
