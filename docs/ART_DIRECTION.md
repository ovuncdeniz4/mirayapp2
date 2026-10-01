# KUT — Art direction (from Master Plan v2.1 §8, §9, §17, §20–21)

Source of truth: **KUT Master Plan v2.1**. All assets use stable **asset ids** (logic never references file paths).

## Visual identity

- **Board:** carved **river stone** tray, recessed cells, inner shadow, subtle **Kut Marks** at corners.
- **Tiles:** enamel-like faces with **element silhouette + glyph**; distinct silhouettes for color-blind play (§8, §25).
- **UI:** parchment/wood **frames**, generous negative space; background deep `#1A1520`.
- **Tone:** mystical, tactile, premium, calm in meta, energetic in combos.
- **Forbidden:** candy gloss, neon casino, cluttered RPG panels, traced reference art, random “tribal” clip art (§20).

## Three-layer asset policy (§20)

1. **Gameplay logic** — tile/special ids only (`tile_registry.json`, level JSON).
2. **Presentation placeholders** — coherent KUT-direction colors (superseded when final art lands).
3. **Final art** — drop-in under `Resources/Art/{category}/{assetId}`; same ids.

## Match tiles (§8)

| TileId (spawn) | Visual asset id | Element | Silhouette (a11y) | Palette |
|----------------|-----------------|---------|-------------------|---------|
| `earth_moss` | `tile_earth_moss` | Earth | Rounded hex + leaf mark | `#4CAF6A` |
| `water_drop` | `tile_water_drop` | Water | Teardrop + ripple | `#3B7BDB` |
| `fire_ember` | `tile_fire_ember` | Fire | Ember flame + spark | `#E85D3B` |
| `wind_gust` | `tile_air_wisp` | Air | Swirl / wisp curl (plan: `air_wisp`) | `#7EC8E3` |
| (Ch3+) `metal_ingot` | `tile_metal_ingot` | Metal | Ingot + heat seam | TBD Ch3 |

## Special pieces (§9.1)

| Asset id | Creation | Read |
|----------|----------|------|
| `special_wind_chime` | Straight 4-match | Gold `#D4AF37`, wind spiral, chime silhouette |
| `special_shaman_drum_earth` | Straight 5, earth resonance | Moss drum head `#4CAF6A`, Kut Mark, earth glyph on face |
| `special_shaman_drum_water` | Straight 5, water resonance | Teal drum head `#3B7BDB`, water glyph on face |
| `special_fire_bomb` | L/T 5 when fire enabled | Ember core `#FF6B2C`, radial ember ring |

Runtime picks drum variant from tile `resonanceElement` (earth/water in Ch1–2).

## Obstacles & blockers

| Asset id | Role |
|----------|------|
| `obstacle_mud` | Layered mud `#6B4A2E` |
| `obstacle_vine` | Vine knot `#2E7D4A` |
| `obstacle_stone` | Indestructible stone `#5C5C66` |

## Spirit animals (§14–15)

MVP profile icons (portrait bust, stone-carved style):

`animal_wolf`, `animal_eagle`, `animal_bear`, `animal_deer`, `animal_salamander`

## Totem (§17)

Five visual stages aligned to chapters; animal carving appears **stage 2+**.

`totem_stage_1` … `totem_stage_5`

## Collection relics

One icon per `content/collection/catalog.json` entry (`relic_*` ids).

## UI chrome

| Asset id | Usage |
|----------|--------|
| `ui_board_tray` | 9:16-friendly stone board frame behind 8×8 grid |
| `ui_panel_frame` | Parchment/wood panel corner treatment (nine-slice) |
| `ui_button_primary` | Primary CTA nine-slice |
| `ui_cell_recess` | Board cell stone recess behind tiles |
| `ui_glyph_wheel` | Onboarding ceremony wheel |
| `map_bg_ch1` / `map_bg_ch2` | Map parallax backgrounds |
| `map_mist_overlay` | Locked chapter/level mist |

## Motion & VFX hooks (§22)

Art should leave room for: 80ms match flash, chime line sweep, bomb two-ring ember, drum element-colored pulse + glyph ring.

## File layout (Unity)

```
unity/Kut/Assets/_Project/Resources/Art/
  Tiles/
  Specials/
  Obstacles/
  Animals/
  Totem/
  Relics/
  UI/
```

Load via `Resources.Load<Sprite>("Art/{Category}/{assetId}")` — see `KutArtCatalog.cs`.
