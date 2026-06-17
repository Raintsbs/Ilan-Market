# Sahibinden.com Karşılaştırması — İlanMarket Durumu

Son güncelleme: 2026-06-03

## Özet

| Alan | Sahibinden | İlanMarket | Not |
|------|------------|------------|-----|
| İlan CRUD + onay | ✅ | ✅ | Tam |
| Kategori ağacı UX | ✅ | ✅ | 3 sütunlu seçici, alt kategori araması |
| Parametrik filtreler | ✅ | ✅ | Marka, yıl, km vb. |
| Kayıtlı arama | ✅ | ✅ | API + ana sayfa paneli |
| Mesaj / teklif | ✅ | ✅ | Tam |
| Param Güvende / escrow | ✅ | ⚠️ | Stripe ile gerçek; yoksa demo |
| Öne çıkarma ödemesi | ✅ | ⚠️ | Stripe Checkout |
| Tramer (araç) | ✅ | ⚠️ | API key ile gerçek; yoksa simülasyon |
| SMS doğrulama | ✅ | ⚠️ | Netgsm veya dev mod |
| Push bildirimleri | ✅ | ⚠️ | VAPID + SW; prod'da etkin |
| SEO landing (şehir+kategori) | ✅ | ❌ | Plan: `docs/SEO-LANDING-ACIKLAMA.md` |
| Konut projeleri vitrini | ✅ | 🔒 | Gizlendi (nav/form yok) |
| Mobil uygulama | ✅ | ❌ | PWA + responsive web |
| e-Devlet doğrulama | ✅ | ❌ | `IsVerified` alanı var, akış yok |
| Iyzico | ✅ | ❌ | Stripe tercih edildi; Iyzico eklenebilir |

## Tamamlanan (bu sprint)

1. **Kategori ağacı** — `CategoryTreePicker`, ilan formu + ana sayfa filtresi
2. **Alt kategori filtreleme** — `GetDescendantCategoryIdsAsync` ilan listesinde
3. **Kayıtlı arama** — `/api/saved-searches`, `SavedSearchPanel`
4. **81 il** — Proje oluşturma sayfası provinces API kullanıyor
5. **Stripe escrow** — Sipariş ödemesi Checkout; demo fallback
6. **Netgsm SMS** — `Sms:Provider=netgsm` + credentials
7. **Web Push** — Abonelik kaydı + VAPID gönderimi
8. **Tramer** — `Tramer:ApiUrl` + `Tramer:ApiKey` (gerçek HTTP)

## Yapılandırma (`appsettings.json`)

```json
"Stripe": { "SecretKey": "...", "PublishableKey": "...", "WebhookSecret": "..." },
"Tramer": { "ApiUrl": "https://...", "ApiKey": "..." },
"Sms": { "Provider": "netgsm", "Netgsm": { "Usercode": "...", "Password": "...", "MsgHeader": "..." } },
"WebPush": { "VapidPublicKey": "...", "VapidPrivateKey": "...", "Subject": "mailto:..." }
```

VAPID anahtar üretimi: `npx web-push generate-vapid-keys`

## Kalan işler

- SEO landing sayfaları (`/[sehir]/[kategori]`)
- Iyzico alternatif ödeme
- Kayıtlı arama → yeni ilan e-posta/push job'ı (`NotifyOnNew`)
- e-Devlet satıcı doğrulama
- EF migration konsolidasyonu (seeder SQL → tek migration)

## Dosya referansları

| Özellik | Backend | Frontend |
|---------|---------|----------|
| Kategori ağacı | `CategoryService.GetTreeAsync` | `CategoryTreePicker.tsx` |
| Alt kategori arama | `AdvertisementService.GetPagedAsync` | — |
| Kayıtlı arama | `SavedSearchesController` | `SavedSearchPanel.tsx` |
| Escrow ödeme | `PaymentService.CreateEscrowCheckoutAsync` | `satin-al/page.tsx` |
| Push | `WebPushNotificationService` | `PushSubscribeButton.tsx` |
