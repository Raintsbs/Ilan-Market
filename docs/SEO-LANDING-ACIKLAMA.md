# SEO Landing Sayfaları (Şehir + Kategori) — Ne Demek?

## Kısa özet

Sahibinden ve benzeri büyük ilan sitelerinde **milyonlarca otomatik sayfa** vardır. Örneğin:

- `/istanbul/elektronik`
- `/ankara/araba`
- `/izmir/konut/kadikoy`

Her URL, o **şehir + kategori** kombinasyonuna özel bir liste sayfasıdır. Google’da “İstanbul ikinci el telefon”, “Ankara satılık araba” gibi aramalarda bu sayfalar sıralanır ve siteye **organik trafik** gelir.

## Nasıl çalışır?

1. **Veri:** 81 il × yüzlerce alt kategori = on binlerce / milyonlarca kombinasyon.
2. **URL yapısı:** Anlamlı, sabit adresler (`/sehir/kategori` veya `/sehir/ilce/kategori`).
3. **İçerik:** O bölgedeki gerçek ilanlar listelenir; başlık ve meta açıklama şehir + kategori ile üretilir.
4. **Sitemap:** Tüm landing URL’leri `sitemap.xml` içinde arama motorlarına bildirilir.
5. **İç linkleme:** Ana sayfa, kategori ağacı ve ilan detayları bu landing’lere link verir.

## Sizin projede ne gerektirir?

| Parça | Açıklama |
|--------|----------|
| Next.js dinamik route | Örn. `app/[sehir]/[kategori]/page.tsx` |
| Slug eşlemesi | İstanbul → `istanbul`, Elektronik → `elektronik` |
| API filtresi | `city` + `categoryId` (ve alt kategoriler) ile ilan listesi |
| `generateMetadata` | Her sayfa için benzersiz title/description |
| Sitemap üretimi | Build veya cron ile URL listesi |
| (İsteğe bağlı) ISR | Çok sayfa olduğu için statik üretim yerine istekte veya periyodik önbellek |

## Örnek kullanıcı yolculuğu

Kullanıcı Google’da **“Bursa ikinci el laptop”** arar → `/bursa/elektronik/bilgisayar` açılır → Bursa’daki laptop ilanlarını görür → bir ilana tıklar → satın alır veya mesaj atar.

## Riskler / dikkat

- **İnce içerik:** İlan yoksa boş sayfa indexlenmemeli (`noindex` veya 404).
- **Duplicate:** Aynı ilan hem genel kategori hem landing’de görünür; canonical URL ile tekrar cezası önlenir.
- **Ölçek:** Milyon URL tek seferde build edilmez; öncelikli şehir+kategori veya sadece ilan olan kombinasyonlar üretilir.

## Durum (güncellendi)

Landing sayfaları uygulandı:

- Route: `app/[sehir]/[[...kategoriSlug]]/page.tsx` — örn. `/istanbul/elektronik/cep-telefonu/apple`
- API: `GET /api/seo/landing?city=&categoryPath=`
- Boş kombinasyonlarda `noindex` (`shouldIndex: false`)
- Sitemap: `/api/seo/sitemap-entries` ile şehir × kategori URL'leri
