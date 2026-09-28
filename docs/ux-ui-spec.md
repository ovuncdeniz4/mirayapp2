# KUT — UX / UI specification

Single source for **screens, flows, and copy** (TR). Implementation: CLI `--slice` + Unity runtime UI (`KutAppBootstrap`).

## Global

- **Disclaimer** (onboarding, footer on home):  
  *「Kurgusal oyun sistemi — tarihsel şaman geleneği değildir.」*
- **Save**: `animalId`, progress, collection, totem tier — **no birth date stored**.
- **Error pattern**: short toast/snackbar, no blocking modal for invalid swap.

## Information architecture

```
Ruh Töreni (once)
    → Ana Ekran
        → [Devam] → current level
        → [Harita] → level picker → Level
        → [Hayvan] → spirit animal sheet
        → [Totem]* → totem layer sheet
        → [Koleksiyon]* → relic grid
Level → victory → meta unlocks → Ana Ekran

* after level_010
```

## Screen specs

### S-01 Ruh Töreni (onboarding)

| Field | Spec |
|-------|------|
| Title | **KUT — Ruh Töreni** |
| Inputs | Ay (1–12), Gün (1–31), numeric keypads |
| Primary CTA | **Ruhumu bul** |
| Success | Full-screen reveal: **Ruh hayvanın: {DisplayNameTr}** + silhouette |
| Logic | `AnimalAssignment.Assign(month, day)` |

### S-02 Ana Ekran (home)

| Element | Content |
|---------|---------|
| Header | Animal name + small totem tier badge if &gt; 0 |
| Progress | `Seviye 1–{n} / {total} açık` |
| Primary | **Devam et** (largest button) |
| Secondary row | Harita · Hayvan |
| Meta row (gated) | Totem · Koleksiyon (disabled + lock icon until L10) |
| Footer | Disclaimer (muted) |

### S-03 Harita (map)

- Sections per **chapter** (`content/chapters.json`); locked chapter collapsed with lock.
- Row: status icon (✓ / → / 🔒), index, level id subtitle optional.
- Tap unlocked row → **S-04 Level**.

### S-04 Level (gameplay HUD)

| Zone | Content |
|------|---------|
| Top | Level id, chapter label, **moves** counter (red if ≤ 3) |
| Objectives | List with checkmarks; progress `current/target` |
| Center | 8×8 board (touch swap) |
| Bottom | Pause · **Quit** (confirm) |

**Interactions**

- Drag/adjacent swap → `SwapCommand`
- Tap special tile → `ActivateSpecialCommand`
- Invalid swap: brief shake, no move cost

**Outcomes**

- Victory: **ZAFER** overlay, stars optional v2, CTA **Devam**
- Defeat: **Hamle kalmadı** — Tekrar dene / Harita

### S-05 Hayvan (spirit sheet)

- Large animal placeholder art  
- **Kalıcı ruh eşleşmen** copy  
- Bonus line from `AnimalBonus.DescriptionTr`

### S-06 Totem (meta, post L10)

- Totem tier visual (layer 1 after Ch1, layer 2 after L20)  
- Copy: *Ruh yolu: Toprak/Su → Ateş/Rüzgar → Ruh (meta)*

### S-07 Koleksiyon (meta)

- Grid of relic cards from `content/collection/catalog.json`  
- Owned: full color + title; locked: silhouette + `?`  
- Counter: `{owned}/{total}`

## Copy deck (buttons)

| Key | TR |
|-----|-----|
| continue | Devam et |
| map | Harita |
| animal | Hayvan |
| totem | Totem |
| collection | Koleksiyon |
| find_spirit | Ruhumu bul |
| retry | Tekrar dene |
| back_home | Ana ekran |

## Accessibility (target)

- Min touch target 48dp  
- Color-blind: tile shapes differ by element (circle, drop, flame, swirl) when art replaces squares  
- Haptics on match (mobile build)

## Traceability

| UX id | Unity panel | CLI |
|-------|-------------|-----|
| S-01 | `OnboardingPanel` + reveal | onboarding prompts |
| S-02 | `HomePanel` (meta buttons lock until L10) | menu `[1]–[6]` |
| S-03 | `MapPanel` scroll + tap level | `[2]` map + pick |
| S-04 | `LevelPanel` + `BoardGridUi` + `ResultOverlayUi` | in-level loop |
| S-05 | `AnimalPanel` | `[3]` |
| S-06 | `TotemPanel` | `[5]` |
| S-07 | `CollectionPanel` | `[6]` |

**Board input (Unity):** tap cell → tap adjacent cell = swap; tap special twice = activate.
