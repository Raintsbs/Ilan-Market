# SQL Server Railway'de çalışmıyorsa (RAM)

SQL Server 2022 en az **~2 GB RAM** ister. Railway ücretsiz planda container sürekli yeniden başlar → API veritabanına bağlanamaz (500 hatası).

## Çözüm A — Railway'de RAM artır (en kolay)

1. [railway.app](https://railway.app) → proje **airy-smile**
2. **sqlserver** servisi → **Settings** → **Resources**
3. **Memory: 2 GB** (veya üzeri) seçin — Hobby plan / kredi gerekir
4. **Volume** ekleyin: mount `/var/opt/mssql` (veri kalıcı olsun)
5. Variables:
   ```
   MSSQL_MEMORY_LIMIT_MB=1536
   ```
6. **Redeploy** sqlserver → logda `SQL Server is now ready for client connections` görün
7. **Ilan-Market** servisini **Redeploy**

## Çözüm B — Azure SQL (ücretsiz öğrenci kredisi)

1. [portal.azure.com](https://portal.azure.com) → SQL databases → Create
2. Server + DB oluşturun (Basic veya serverless)
3. Firewall: **Allow Azure services** + IP'niz
4. Connection string kopyalayın
5. Railway → **Ilan-Market** → Variables:
   ```
   ConnectionStrings__DefaultConnection=<Azure connection string>
   ```
6. Redeploy API

Terminalden (Azure string'inizle):
```powershell
cd c:\Projects\advertisement
npx @railway/cli link -p airy-smile
npx @railway/cli service Ilan-Market
npx @railway/cli variables set "ConnectionStrings__DefaultConnection=Server=tcp:....database.windows.net,1433;..."
npx @railway/cli redeploy --yes
```

## Şu an çalışan adresler

| Servis | URL |
|--------|-----|
| Site | https://ilan-market.vercel.app |
| API | https://ilan-market-production-b980.up.railway.app |
| Health | https://ilan-market-production-b980.up.railway.app/health → **Healthy** |

İlanlar için DB bağlantısı şart — yukarıdaki A veya B'den birini yapın.
