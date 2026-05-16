# v0.2.0 — Chat-command coverage + auto-scaling ring

First configurable release since v0.1.0. The ring depth auto-scales with
world size, and coverage is set with a single in-game admin chat command
(`/farlands coverage <0-100>`). Default `20%` until you change it. No
Customize-World dropdown, no client-side install, no translation-key
quirks — pure server-side, ZIP installs work everywhere including
1-click installers.

This release supersedes the unreleased v0.1.1 and v0.1.2 drafts.

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

### In-game chat command (this session)

- **New `/farlands coverage <0-100>` chat command** sets the coverage
  percentage and persists it to the savegame. Newly generated chunks use
  the new ring; already-generated chunks stay as they were. Requires the
  `controlserver` privilege (admin / single-player host).
- **New `/farlands status`** prints the full ring parameters (current
  coverage, map size, ring depth, band width, band count, tunnel lift).
- **Default coverage** is `20%` until the command sets a value.
- **Persistence** writes to both `WorldManager.SaveGame.WorldConfiguration`
  (canonical, persisted to disk) and `World.Config` (runtime mirror), so
  the change is both immediately visible and survives restart.

### Customize-World dropdown removed (this session)

- Earlier drafts shipped a `[assembly: ModInfo(WorldConfig = …)]`
  dropdown on the Customize-World screen. It worked in single player only
  if the file was installed as a raw DLL at `%APPDATA%/Mods/`, and even
  then VS rendered raw translation keys (`worldattribute-Far Lands Coverage`
  …) because VS reads raw-DLL `[ModInfo]` via metadata-only reflection
  and never JIT-loads the assembly before the screen renders.
- ZIP installs from 1-click installers (the ModDB path) couldn't reach
  the dropdown at all.
- Dropping the dropdown removes both classes of problem and gives a
  single, consistent config path. The chat command works regardless of
  install format (ZIP, folder, raw DLL).

## What the percentages mean

| Coverage   | Ring depth (1024k world) | Vanilla area | Far Lands area |
|------------|--------------------------|--------------|----------------|
| 0%         | 0                        | 100%         | 0% (mod off)   |
| 20% (default) | 54 000                | 80%          | 20%            |
| 40%        | 117 000                  | 60%          | 40%            |
| 60%        | 188 000                  | 40%          | 60%            |
| 80%        | 273 000                  | 20%          | 80%            |
| 100%       | 512 000                  | 0%           | 100%           |

## Upgrade notes

- **Existing v0.1.0 worlds**: keep their already-generated chunks. New
  chunks use the auto-scaled ring depth derived from the persisted
  coverage value (or env vars). A world started on v0.1.0 will have two
  overlapping layouts at the border of the previously explored area
  unless you regenerate.
- **Custom `VSFL_DEPTH` users**: your env var still wins at startup over
  the persisted coverage value. The chat command itself ignores env vars,
  so a runtime `/farlands coverage` call takes effect immediately; on the
  next restart the env vars reapply.
- **No more `worldconfig` slot**: if you hand-edited a `worldconfig` file
  from a draft build, the key `Far Lands Coverage` is still respected
  (the mod still reads it from the savegame at startup), but the
  Customize-World UI no longer surfaces it. Use the chat command.

## Compatibility

- Vintage Story 1.22.2.
- **Server-side only.** Single-player install:
  `%APPDATA%/VintagestoryData/Mods/<zip>`. Dedicated server install:
  the server's `Mods/<zip>`. Clients can join a dedicated server
  without the mod (`requiredOnClient: false`, all blocks are vanilla).
- Safe to add or remove on an existing world (newly generated chunks
  use the new ring, previously generated chunks stay vanilla).

## Asset

`vsmodfarlands_0.2.0.zip` — 327 591 bytes (modinfo + DLL + modicon + LICENSE)
SHA-256: `de4e54d42f63b7b2f124b5d041d54500beae4fc605b320f1fb64d6ac9536a402`

## License

MIT. See [LICENSE](https://github.com/Eloi-Desbrosses/VsModFarlands/blob/main/LICENSE).
