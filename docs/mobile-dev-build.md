# KUT — Yerel mobil development build (App Store değil)

## Önkoşul

- Unity **2022.3.62f1**
- Proje: `unity/Kut`
- iOS: Xcode + Apple ID (development signing)
- Android: Android SDK + JDK

## iOS

1. **File → Build Settings → iOS → Switch Platform**
2. **Player Settings → Identification** — bundle id örn. `com.you.kut.dev`
3. **Build** → Xcode projesi klasörü
4. Xcode’da **Signing & Capabilities → Automatically manage signing**
5. Cihaz seç → **Run** (TestFlight / Archive / Upload **yapma**)

## Android

1. **Build Settings → Android → Switch Platform**
2. **Build** APK veya **Build And Run** (USB debug)

## Haptics

Ayarlar ekranından titreşim açık; gerçek motor sadece fiziksel cihazda anlamlıdır.
