# Ötüken Kut'u: Erlik'in Gazabı — rebuild status

Development branch: `codex/erlik-rebuild`.

## Implemented foundation

- Save schema v5 with constellation, gold, booster inventory, generated-level results, generator version, normalization, checksum, temporary-file writes, and last-known-good backup recovery.
- Five `ZodiacType` powers with deterministic integer/fixed-point modifiers.
- Layered board-cell model for movable rune, ground, overlay, blocker, and occupant state.
- Match shape classifier for 3/4/5/L/T groups.
- Generic data-driven obstacle progression and the Season 1 Dry Soil → Wet Soil → Planted Seed → Giant Flower recipe.
- Ultimate charge lifecycle, atomic booster economy, deterministic boss damage/phases, and a 20,000 HP Erlik definition.
- Data-driven ten-season catalog and deterministic generation metadata for levels 1–1000.
- Unity onboarding changed from birth-date assignment to player-selected constellations.
- Original Season 1 gameplay and constellation-selection benchmark backgrounds, imported through the art catalog with provenance.

## Verification

- Core regression suite: 39 tests passing.
- Existing level validator: levels 1–20 valid.
- Generator test: levels 1–1000 deterministic and structurally valid.
- Art manifest: 42/42 entries resolve.

## Remaining production gates

- Route Unity gameplay from the legacy `BoardEngine` to `LayeredBoardState` and the new command/event session.
- Author the first 30 curated Season 1 levels and generator overrides for 31–100.
- Connect generic obstacle recipes, ultimate, boosters, damage, and boss turns to the playable Unity session.
- Replace runtime-built screens with editable prefabs and complete responsive navigation/overlays.
- Produce the remaining Season 1 rune, obstacle-state, boss, VFX, map, and audio benchmark assets.
- Perform Unity EditMode/PlayMode, device-layout, performance, and full Season 1 balance passes.
- Produce and approve Seasons 2–10 through their independent quality gates.

The new architecture is intentionally alongside the legacy playable loop until parity is reached. Do not remove the legacy implementation before the new Unity session passes the full vertical-slice acceptance flow.
