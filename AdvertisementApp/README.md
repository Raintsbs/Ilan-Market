# AdvertisementApp

ASP.NET Core 8 ilan uygulaması — **MVC UI** + isteğe bağlı **REST API (JWT)**.

## Projeler

| Proje | Durum |
|--------|--------|
| `AdvertisementApp.UI` | **Ana arayüz** — MVC Razor (Visual Studio ile çalıştır) |
| `AdvertisementApp.API` | REST + JWT + Swagger |
| `AdvertisementApp.Business` | Servisler, validation, AutoMapper |
| `AdvertisementApp.DataAccess` | EF Core, Identity, seed |
| `AdvertisementApp.Dtos` | DTO’lar |
| `AdvertisementApp.Entities` | Entity modelleri |
| `AdvertisementApp.Common` | Result, ApiResponse, roller |
| `AdvertisementApp.Tests` | xUnit |

## MVC UI çalıştırma (önerilen)

Visual Studio 2022’de `AdvertisementApp.UI` projesini başlangıç projesi yapıp **F5**.

Veya terminal:

```powershell
cd c:\Projects\AdvertisementApp
dotnet ef database update --project AdvertisementApp.DataAccess --startup-project AdvertisementApp.UI
dotnet run --project AdvertisementApp.UI
```

Tarayıcı: **http://localhost:5076**

**Admin paneli:** Giriş yaptıktan sonra menüde **Admin** → `/Admin/Dashboard` (area Admin)

- Admin: `admin@advertisement.local` / `Admin123!`

## API çalıştırma (isteğe bağlı)

```powershell
dotnet ef database update --project AdvertisementApp.DataAccess --startup-project AdvertisementApp.API
dotnet run --project AdvertisementApp.API
```

Swagger: **http://localhost:5050/swagger**

## Connection string

`AdvertisementApp.UI/appsettings.json` veya `AdvertisementApp.API/appsettings.json`:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=AdvertisementAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

## Seed

- Admin: `admin@advertisement.local` / `Admin123!`
- Roller: `Admin`, `User`

## Test

```powershell
dotnet test
```

## Docker

```powershell
docker compose up -d
```

## Migration

```powershell
dotnet ef migrations add Ad --project AdvertisementApp.DataAccess --startup-project AdvertisementApp.UI --output-dir Migrations
dotnet ef database update --project AdvertisementApp.DataAccess --startup-project AdvertisementApp.UI
```
