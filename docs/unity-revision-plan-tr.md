# KUT Unity — Revizyon planı (Master Plan + UX spec)

**Durum (önce):** Kut.Core + 20 level + art PNG’ler repoda; Unity tarafı **prototip kabuk** (metin kutusu UI, harita scroll kırık, tahta kaba, koleksiyon metin listesi).

**Hedef:** ZIP + Unity 2022.3.62f1 ile **oynanabilir dikey dilim** — görseller `Resources/Art`, akış `ux-ui-spec.md`.

## Faz U1 — Oynanabilirlik (P0)

| # | Sorun | Çözüm |
|---|--------|--------|
| U1.1 | UI tıklanmıyor | `EventSystem` + `StandaloneInputModule` (runtime) |
| U1.2 | Harita boş | `ScrollRect` + `ContentSizeFitter`, bölüm başlıkları + seviye satırları |
| U1.3 | Tahta oynanmıyor / kötü | Tahta tray sprite, hücre boyutu ekrana göre, seçim vurgusu, toast |
| U1.4 | Zafer/yenilgi | `ResultOverlayUi` + save + haritaya dönüş |

## Faz U2 — Görseller (P0)

| # | Spec | Uygulama |
|---|------|----------|
| U2.1 | Tahta tile/special/obstacle | `KutArtCatalog` → `BoardGridUi` (sprite + renk yedek) |
| U2.2 | Hayvan / totem | Portre `Resources/Art/Animals`, totem stage |
| U2.3 | Ana ekran | Hayvan portresi + ilerleme (S-02) |
| U2.4 | Koleksiyon | Relik grid + ikon (`Resources/Art/Relics`) |
| U2.5 | Tahta çerçevesi | `ui_board_tray` arka plan |

## Faz U3 — Spec tamamlama (P1)

- Harita: parallax yok; bölüm kilidi + ✓/→/🔒
- Level HUD: chapter etiketi, hedef TR açıklamaları
- Ayarlar ekranı (ses/haptics placeholder)
- `docs/ART_DIRECTION.md` ile asset id eşlemesi CI notu

## Faz U4 — Master Plan sapmaları (P2, Core)

- Fire Bomb 13 hücre (Manhattan ≤2) — Core + level validator
- UI Toolkit migrasyonu (opsiyonel; uGUI kalabilir MVP)
- Unity 6.3 hattı (Ventura+ makineler)

## ZIP kullanıcı kontrol listesi

1. `Assets/_Project/Scripts/` içinde 5 DLL + `Resources/Art/` dolu  
2. Hub → `unity/Kut` → **2022.3.62f1**  
3. **KUT → Create Main Scene And Open** → Play  
4. Ay/gün → **Ruhumu bul** → Harita → **→ 1. level_001** → komşu hücreye swap

**Bu revizyon:** U1 + U2 maddelerinin kod tarafı `main` branch’te güncellenir.
