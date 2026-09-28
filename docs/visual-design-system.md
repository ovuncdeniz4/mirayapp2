# KUT — Visual design system (portrait mobile)

Fictional **Beş Element** shamanic match-3. All visuals are **original game fiction**, not historical ritual documentation.

## Platform & canvas

| Token | Value |
|--------|--------|
| Orientation | **Portrait** only (9:16) |
| Reference resolution | **1080 × 1920** |
| Safe area | Respect notch/home indicator; primary actions in lower third |
| Board zone | Center 70% width; 8×8 grid with 8px gutter between cells |

## Color — elements (match tiles)

| Element | Hex | Usage |
|---------|-----|--------|
| Earth | `#4CAF6A` | Moss tile, collect UI |
| Water | `#3B7BDB` | Drop tile |
| Fire | `#E85D3B` | Ember tile (Ch2+) |
| Air (Wind) | `#7EC8E3` | Gust tile (Ch2+) |
| Mud obstacle | `#6B4A2E` | Layered brown |
| Vine obstacle | `#2E7D4A` | Layered green |
| Stone blocker | `#5C5C66` | `#` cells |

## Color — specials

| Special | Hex | Icon hint |
|---------|-----|-----------|
| Wind Chime | `#D4AF37` | Cross / chime silhouette |
| Shaman Drum | `#9B3A6E` | Drum circle |
| Fire Bomb | `#FF6B2C` | Radial burst |

## Color — UI chrome

| Role | Hex |
|------|-----|
| Background deep | `#1A1520` |
| Panel surface | `#2A2235` @ 92% opacity |
| Primary accent | `#C9A227` (spirit gold) |
| Text primary | `#F5F0E8` |
| Text muted | `#A89FB0` |
| Danger / low moves | `#E05252` |

## Typography (Unity)

| Role | Font | Size (1080p) |
|------|------|----------------|
| Title | **LegacyRuntime** or bundled **Noto Sans** (TR) | 48–56 sp |
| Body | Same | 32 sp |
| HUD / counters | Same, bold | 36 sp |
| Disclaimer | Same, muted | 24 sp |

Turkish copy is authoritative in [`ux-ui-spec.md`](ux-ui-spec.md).

## Spirit animals (totem icons)

Final portrait sprites: `Resources/Art/Animals/animal_{id}` per [`ART_DIRECTION.md`](ART_DIRECTION.md). Token colors remain fallback if a sprite is missing.

| id | Display TR | Accent |
|----|------------|--------|
| wolf | Kurt | `#8B9DAF` |
| eagle | Kartal | `#B89B4C` |
| bear | Ayı | `#6B5344` |
| deer | Geyik | `#C4A882` |
| salamander | Semender | `#E85D3B` |

## Motion (Phase 2 targets)

- Swap: 120ms ease-out  
- Match clear: 180ms scale + fade  
- Gravity: 200ms stagger per row  
- Special activate: 350ms pulse + screen shake (light)

Unity stub: `PresentationAnimationQueue` — replace logs with tweens when art lands.

## Audio (planned)

- UI tap: soft wood knock  
- Match: element-specific one-shots (earth thud, water splash, fire crackle, air whoosh)  
- Chime/drum/bomb: distinct stingers  
- No licensed “shaman” field recordings — synthetic/fantasy only
