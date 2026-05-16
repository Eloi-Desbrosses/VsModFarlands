# v0.2.0 — Customize-World dropdown + auto-scaling ring

First UI-configurable release since v0.1.0. The ring depth is now picked from
a dropdown on the **Customize World** screen and auto-scales with world size,
so solo players no longer need to touch environment variables. Env-var
overrides remain available for headless deployments.

This release supersedes the unreleased v0.1.1 and v0.1.2 drafts and bundles
their changes together with this session's UI/UX polish.

## What changed since v0.1.0

### Auto-scaling ring (formerly draft v0.1.1)

- **Default ring depth** is now derived from world size, not a fixed
  210 000 blocks. The formula is `depth = worldMin × (1 − √(1 − C/100)) / 2`
  where C is the chosen coverage percentage.
- **Bounded extremes**: on huge worlds the ring stays large but tractable;
  on small worlds each of the 7 bands stays wide enough to be readable.
- **Band width follows depth** — `VSFL_BAND` defaults to `VSFL_DEPTH / 7` so
  the canonical 14-biome layout is preserved at any ring size without manual
  tuning.

### Customize-World dropdown (formerly draft v0.1.2)

- **New world-config attribute** `Far Lands Coverage` under the **Worldgen**
  category. Dropdown values: `0%`, `20%`, `40%`, `60%`, `80%`, `100%`.
- **Default**: `20%` (a strong border phenomenon while keeping ~80% of the
  playable surface vanilla).
- **`onlyDuringWorldCreate: true`**: locked once the world is created, so
  chunks generated at different times never disagree about ring depth.
- **Env vars still win** when set (`VSFL_DEPTH`, `VSFL_BAND`,
  `VSFL_TUNNEL_LIFT`).

### UI/UX polish (this session)

- **Dropdown trimmed from 11 to 6 values** (0%, 20%, 40%, 60%, 80%, 100%)
  to reduce vertical clipping in the Customize-World dialog. Vanilla VS
  doesn't scroll that dialog, and the Worldgen category already has ~18
  vanilla entries; halving our option count keeps the screen usable on
  smaller resolutions.
- **Worldconfig key chosen for legibility** — `code: "Far Lands Coverage"`
  with spaces, and dropdown names `"0%"…"100%"`, so the raw-key fallback
  reads as natural English (see "Translation-key limitation" below).
- **Best-effort translation injection** via `[ModuleInitializer]` and a
  Harmony postfix on `TranslationService.GetUnformatted`. Works on the
  server side (and on the client *after* entering any world). Does **not**
  fire on the client before the Customize-World screen renders, because VS
  reads `[assembly: ModInfo]` via metadata-only reflection without JIT-
  loading raw-DLL mod assemblies at that point.

## What the percentages mean

| Coverage   | Ring depth (1024k world) | Vanilla area | Far Lands area |
|------------|--------------------------|--------------|----------------|
| 0%         | 0                        | 100%         | 0% (mod off)   |
| 20% (default) | 54 000                | 80%          | 20%            |
| 40%        | 117 000                  | 60%          | 40%            |
| 60%        | 188 000                  | 40%          | 60%            |
| 80%        | 273 000                  | 20%          | 80%            |
| 100%       | 512 000                  | 0%           | 100%           |

## Translation-key limitation

Dropdown label and values render with their VS-hardcoded prefixes when no
translation entry exists in the `game` domain:

- Label: `worldattribute-Far Lands Coverage`
- Values: `worldconfig-Far Lands Coverage-0%`, `…-20%`, etc.

The text after the prefix is readable. For a clean label, add the matching
entries to `<Vintagestory install>/assets/game/lang/en.json` (additive, gets
wiped on VS update). This is a VS limitation for raw-DLL mods — see README
for the mechanism.

## Upgrade notes

- **Existing v0.1.0 worlds**: keep their already-generated chunks. New
  chunks use the new auto-scaled ring depth derived from the slider (or env
  vars). A world started on v0.1.0 will have two overlapping layouts at the
  border of the previously explored area unless you regenerate.
- **Custom `VSFL_DEPTH` users**: your env var still wins over the slider.
- **World-config key renamed**: previously-drafted v0.1.1 / v0.1.2 used the
  key `farLandsCoverage`. v0.2.0 uses `Far Lands Coverage` (with spaces).
  This only matters if you hand-edited a `worldconfig` file from a draft
  build — released v0.1.0 didn't have any world-config key.

## Compatibility

- Vintage Story 1.22.2.
- Server-side only for worldgen; raw-DLL install required at
  `%APPDATA%/VintagestoryData/Mods/VsModFarlands.dll` for the slider to
  appear in singleplayer Customize-World.
- Safe to add or remove on an existing world (newly generated chunks use
  the new ring, previously generated chunks stay vanilla).

## Asset

`vsmodfarlands_0.2.0.zip` — 328 172 bytes (modinfo + DLL + modicon + LICENSE)
SHA-256: `46b5b4038d239a40d2de78282c86a78263ec73ea400dc145178c2c1db26e83ce`

## License

MIT. See [LICENSE](https://github.com/Eloi-Desbrosses/VsModFarlands/blob/main/LICENSE).
