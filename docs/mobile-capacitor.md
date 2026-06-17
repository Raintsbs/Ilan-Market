# Capacitor ile mobil uygulama (PWA shell)

İlanMarket Next.js uygulamasını native kabuk içinde çalıştırmak için [Capacitor](https://capacitorjs.com/) kullanabilirsiniz.

## Ön koşullar

- Node.js 20+
- Android Studio veya Xcode (mağaza yayını için)
- Production frontend URL (`https://www.ornek.com`)

## Kurulum

```bash
cd frontend
npm install @capacitor/core @capacitor/cli @capacitor/android @capacitor/ios
npx cap init "İlanMarket" com.ilanmarket.app --web-dir=out
```

Statik export kullanıyorsanız `next.config` içinde `output: 'export'` gerekir. **Mevcut projede SSR/ISR sayfaları olduğu için önerilen yaklaşım:** Capacitor `server.url` ile canlı siteyi yüklemek:

`capacitor.config.ts`:

```ts
import type { CapacitorConfig } from "@capacitor/cli";

const config: CapacitorConfig = {
  appId: "com.ilanmarket.app",
  appName: "İlanMarket",
  webDir: "out",
  server: {
    url: process.env.CAPACITOR_SERVER_URL ?? "https://www.ornek.com",
    cleartext: false,
  },
};

export default config;
```

```bash
npx cap add android
npx cap add ios
npx cap sync
npx cap open android
```

## Deep link / push

- JWT API mevcut (`/api/auth/login`, refresh token) — native WebView oturumu `localStorage` ile paylaşılır.
- Web push (`WebPushSubscriptions`) PWA ile çalışır; native push için FCM/APNs eklentisi ayrıca gerekir.

## React Native alternatifi

Tam native deneyim için mevcut REST API (`AdvertisementApp.API`) doğrudan React Native istemcisinden tüketilebilir. Capacitor daha hızlı MVP; React Native uzun vadede performans ve mağaza deneyimi için uygundur.
