# Railway’de API kurulumu — baştan sona (detaylı)

Vercel’de site açık: **https://ilan-market.vercel.app**  
Ama ilanlar, giriş, mesajlar **çalışmıyorsa** sebep: **API henüz yok**.

---

## 1. Basit mantık (3 parça)

```
[Kullanıcı]  →  Vercel (frontend)     →  ekran, butonlar
                    ↓ istek atar
               Railway (API)           →  iş kuralları, giriş, ilan kaydı
                    ↓ okur/yazar
               Railway (SQL Server)    →  veritabanı (ilanlar, kullanıcılar)
```

| Parça | Nerede | Ne işe yarar |
|--------|--------|----------------|
| **Frontend** | Vercel | Sitenin görünen yüzü (zaten kurulu ✓) |
| **API** | Railway | Backend — `/api/...` isteklerini cevaplar |
| **Veritabanı** | Railway | SQL Server — verilerin saklandığı yer |

`NEXT_PUBLIC_API_URL` = Vercel’in “API nerede?” diye sorduğu adres.

---

## 2. Railway hesabı

1. [railway.app](https://railway.app) açın  
2. **Login with GitHub**  
3. GitHub’a Railway erişimine izin verin  

---

## 3. Yeni proje + GitHub bağlantısı

1. **New Project**  
2. **Deploy from GitHub repo**  
3. **Raintsbs/Ilan-Market** seçin  
4. Railway ilk servisi otomatik oluşturur — adını **`api`** yapın (üzerine tıklayıp Settings → name)

> İlk deploy muhtemelen **hata verir** — normal. Henüz ayar yapmadık.

---

## 4. ADIM A — Veritabanı (SQL Server)

API’nin veri saklayacağı yeri önce kuruyoruz.

### 4.1 Yeni servis
1. Proje ekranında sağ üst veya canvas’ta **+ Create** / **New**  
2. **Empty Service**  
3. Servis adı: tam olarak **`sqlserver`** (küçük harf, boşluksuz)

### 4.2 Docker image ile SQL kur
1. `sqlserver` servisine tıklayın  
2. **Settings** sekmesi  
3. **Source** bölümü → **Deploy from Docker Image** (veya Image)  
4. Image adı yapıştırın:
   ```
   mcr.microsoft.com/mssql/server:2022-latest
   ```
5. **Deploy** / kaydet

### 4.3 SQL şifresi
1. Aynı serviste **Variables** sekmesi  
2. **+ New Variable** ile ekleyin:

| Variable | Value |
|----------|--------|
| `ACCEPT_EULA` | `Y` |
| `MSSQL_SA_PASSWORD` | `GucluSifre123!Railway` |

> Bu şifreyi bir yere not edin — API connection string’de aynısını kullanacaksınız.

### 4.4 Public internete AÇMAYIN
SQL Server’a dışarıdan erişim gerekmez. **Networking → Public** açmayın. Sadece `api` servisi içeriden bağlanır.

Deploy bitince `sqlserver` yeşil / Active olmalı.

---

## 5. ADIM B — API servisini doğru klasörden build et

`api` servisine dönün.

### 5.1 Kaynak kod ayarı
**Settings:**

| Ayar | Değer | Açıklama |
|------|--------|----------|
| **Root Directory** | `AdvertisementApp` | Repo kökü değil, API’nin olduğu klasör |
| **Builder** | Dockerfile | Hazır Docker dosyası kullanılır |
| **Dockerfile Path** | `AdvertisementApp.API/Dockerfile` | API’nin Dockerfile yolu |

Kaydedince Railway yeniden build başlatır.

### 5.2 Ortam değişkenleri (Variables)

`api` → **Variables** → **RAW Editor** ile hepsini bir seferde yapıştırabilirsiniz.

**Önce şunları kendi değerlerinizle değiştirin:**
- `GucluSifre123!Railway` → SQL adımında yazdığınız şifre (aynı olmalı)
- `https://ilan-market.vercel.app` → sizin Vercel adresiniz
- `https://XXXX.up.railway.app` → API domain’i (5.3’te alacaksınız; önce boş bırakıp sonra güncelleyebilirsiniz)

```
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://+:8080

Jwt__Key=IlanMarket2026GizliAnahtarEnAz32Karakter!!

ConnectionStrings__DefaultConnection=Server=sqlserver;Database=AdvertisementAppDb;User Id=sa;Password=GucluSifre123!Railway;TrustServerCertificate=True;Encrypt=False;

Storage__Provider=Local
Payments__AllowDemo=true
Sms__Provider=dev
Email__Enabled=true
Email__PickupDirectory=email-pickup
Email__From=noreply@ilanmarket.staging

App__FrontendUrl=https://ilan-market.vercel.app
App__ApiUrl=https://BURAYA-API-DOMAIN.up.railway.app
Cors__AllowedOrigins__0=https://ilan-market.vercel.app
```

#### Her satır ne demek?

| Değişken | Ne işe yarar |
|----------|----------------|
| `ASPNETCORE_ENVIRONMENT=Staging` | Öğrenci/demo modu — Azure/Netgsm zorunlu değil |
| `Jwt__Key` | Giriş token’ları için gizli anahtar (32+ karakter) |
| `ConnectionStrings__...` | SQL’e nasıl bağlanacağı; **`Server=sqlserver`** = bir üstte kurduğunuz servis adı |
| `Storage__Provider=Local` | Resimler sunucuda dosya olarak (Azure gerekmez) |
| `Payments__AllowDemo=true` | Demo ödeme (iyzico anahtarı şart değil) |
| `Sms__Provider=dev` | SMS konsola/log’a yazılır |
| `App__FrontendUrl` | Sitenizin adresi (Vercel) |
| `App__ApiUrl` | API’nin kendi public adresi |
| `Cors__AllowedOrigins__0` | Vercel’den gelen isteklere izin ver |

> İki alt çizgi `__` = JSON’daki `:` yerine geçer. Örn. `Jwt__Key` → `Jwt:Key`

### 5.3 API’ye internet adresi ver
1. `api` servisi → **Settings** → **Networking**  
2. **Generate Domain**  
3. Örnek: `ilan-market-api-production.up.railway.app`  
4. Bu URL’yi kopyalayın  
5. **Variables**’a dönün → `App__ApiUrl` değerini bu URL ile güncelleyin (`https://` ile, sonda `/` yok)  
6. **Redeploy** (Deployments → üç nokta → Redeploy)

---

## 6. Deploy’un bitmesini bekle

1. `api` → **Deployments** → son satır **Success** / **Active** olmalı  
2. **View logs** — kırmızı hata varsa okuyun  

### Başarı testi (tarayıcı)

Şunu açın (kendi domain’inizle):
```
https://SIZIN-API-DOMAIN.up.railway.app/health
```

**Görmek istediğiniz:** `Healthy` veya benzeri sağlıklı yanıt.

Swagger (Staging’de açık):
```
https://SIZIN-API-DOMAIN.up.railway.app/swagger
```

---

## 7. Vercel’i API’ye bağla

1. [vercel.com](https://vercel.com) → **ilan-market** projesi  
2. **Settings** → **Environment Variables**  
3. `NEXT_PUBLIC_API_URL` düzenle:

```
https://SIZIN-API-DOMAIN.up.railway.app
```

- `http://localhost:5050` **olmamalı**  
- Sonda **`/`** veya **`)`** olmamalı  

4. **Deployments** → en son deploy → **⋯** → **Redeploy**

---

## 8. Son test

1. https://ilan-market.vercel.app açın  
2. Ana sayfada **ilanlar** görünmeli  
3. **Kayıt ol** / **Giriş** deneyin  

Admin (seed ile gelir):
- `admin@advertisement.local` / `Admin1234!`

---

## 9. Sık hatalar

| Belirti | Muhtemel sebep | Çözüm |
|---------|----------------|--------|
| **Build failed — Build image** | Root Directory yanlış veya Dockerfile bulunamadı | Bölüm 9.1 |
| Vercel açılıyor, veri yok | `NEXT_PUBLIC_API_URL` yanlış | Vercel env + redeploy |
| CORS hatası (F12 Console) | `Cors__AllowedOrigins__0` yanlış | Tam Vercel URL, redeploy API |
| API crash, DB hatası | SQL şifresi uyuşmuyor | Connection string’deki şifre = `MSSQL_SA_PASSWORD` |
| `Server=sqlserver` bulunamıyor | SQL servis adı farklı | Servis adını `sqlserver` yap veya connection string’i güncelle |
| Build fail | Root Directory yanlış | `AdvertisementApp` olmalı |
| Railway ücret / limit | Ücretsiz kredi bitti | Dashboard → Usage |

---

## 9.1 Build failed — Build image (sizin hata)

Railway tüm repoyu (frontend + backend) build etmeye çalışınca patlar.

**Çözüm A — Railway ayarı (önerilen)**  
`api` servisi → **Settings** → **Source**:
- **Root Directory:** boş bırakın (repo kökü) VEYA `AdvertisementApp`
- **Builder:** Dockerfile
- Root boşsa: `Dockerfile` (repo kökündeki yeni dosya)
- Root `AdvertisementApp` ise: `AdvertisementApp.API/Dockerfile`

**Çözüm B — GitHub’a fix push**  
Repoya kök `Dockerfile` + `railway.toml` eklendi. Push edin:

```powershell
git add Dockerfile railway.toml .dockerignore AdvertisementApp/railway.toml
git commit -m "fix: Railway Docker build from monorepo root"
git push origin master
```

Railway otomatik yeniden deploy eder.

**Variables** henüz yazmadıysanız deploy sonrası API yine düşebilir — `docs/RAILWAY-API-DETAYLI.md` Bölüm 5.2’yi tamamlayın.

---

## 10. Özet checklist

- [ ] Railway’de `sqlserver` servisi (Docker image + şifre)  
- [ ] Railway’de `api` servisi (Root: `AdvertisementApp`, Dockerfile)  
- [ ] Tüm Variables yazıldı  
- [ ] API **Generate Domain** alındı  
- [ ] `/health` çalışıyor  
- [ ] Vercel `NEXT_PUBLIC_API_URL` = API domain  
- [ ] Vercel redeploy  
- [ ] Site ilanları gösteriyor  

Takıldığınız adımın ekran görüntüsünü atın (Railway Deployments log veya Variables ekranı).
