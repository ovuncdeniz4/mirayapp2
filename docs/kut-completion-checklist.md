# KUT — Completion checklist (App Store build hariç)

Unity **2022.3.62f1**, proje: `unity/Kut`.

## Core & content

- [ ] `dotnet test` yeşil
- [ ] `dotnet run --project tools/Kut.LevelValidator` hatasız
- [ ] Fire Bomb Manhattan ≤ 2 (13 hücre) testleri geçiyor
- [ ] L1–20 JSON validator’dan geçiyor

## Unity akış

- [ ] Splash → onboarding / home
- [ ] Ruh töreni → reveal → home
- [ ] Harita scroll, bölüm kilidi, level seçimi
- [ ] Level: swap (tap + sürükle), special activate, animasyon kuyruğu
- [ ] Geçersiz swap: sallanma
- [ ] Duraklat / çık onayı
- [ ] Zafer / yenilgi overlay
- [ ] L10 totem + koleksiyon sekmeleri
- [ ] Ayarlar: ses, titreşim, reduced motion, save sıfırla
- [ ] Tutorial L1 / L5 / L8 / L12 (ilk giriş)

## Art & manifest

- [ ] `./tools/validate-art-manifest.sh` OK (40 PNG id)
- [ ] Tahta tray + panel frame + button nine-slice
- [ ] Hücre recess (`ui_cell_recess`) tahtada görünür
- [ ] Harita parallax arka plan + mist
- [ ] Glyph wheel töreni (reduced motion: fade only)

## Visual / SFX polish

- [ ] Noto Sans TR (`Resources/Fonts/NotoSans-Regular.ttf`)
- [ ] §22 VFX: chime line, bomb ring, drum pulse (uGUI overlay)
- [ ] Ses: `Resources/Audio/SFX/*.wav` (procedural placeholder clip’ler)
- [ ] Harita müziği Ch1/Ch2 ambient loop

## CI

- [ ] GitHub Actions `KUT CI` workflow yeşil

## Mobil (store değil)

- [ ] [`mobile-dev-build.md`](mobile-dev-build.md) adımları ile development build (opsiyonel)
