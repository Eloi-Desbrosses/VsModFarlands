# v0.1.1 — Ring auto-scales with world size

Default ring depth is now derived from the world size instead of being a fixed
210 000 blocks. On the default 1 024 000-block world the Far Lands now cover
roughly 30% of the surface (down from 64% in v0.1.0), leaving 70% vanilla. On
larger worlds the ring scales up proportionally; on smaller worlds it shrinks
but stays wide enough for all 7 bands to be readable.

## What changed

- **Auto-scale default**: `VSFL_DEPTH` defaults to `min(mapX, mapZ) / 12`,
  clamped to the range `[60 000, 2 000 000]`. On the standard 1 024 000-block
  world that produces a 85 000-block-deep ring per border, with bands of about
  12 200 blocks each.
- **Band width follows depth**: `VSFL_BAND` defaults to `VSFL_DEPTH / 7`, so
  the canonical 14-biome layout is preserved at any ring size without manual
  tuning.
- **Bounded cost on large worlds**: the 2 000 000-block ceiling prevents the
  ring from consuming an unreasonable fraction of huge worlds (10M+).
- **Bounded readability on small worlds**: the 60 000-block floor keeps each
  band at least about 8 500 blocks wide, so all 7 biomes remain distinguishable.

## Updated coordinates for the default world

Previous coords (v0.1.0) used a 30 000-block band spacing. The new defaults
use ~12 200-block bands, so the centre of each band sits at different x/z
values. Use these instead:

```
/tp <you> =6000  =220 =6000        # Nothingness   (Corner band 0)
/tp <you> =18000 =220 =18000       # Skygrid       (Corner band 1)
/tp <you> =30000 =220 =30000       # EndIsland     (Corner band 2)
/tp <you> =42000 =220 =42000       # Stripe        (Corner band 3)
/tp <you> =54000 =220 =54000       # Strip         (Corner band 4)
/tp <you> =66000 =220 =66000       # Corner Stack  (Corner band 5)
/tp <you> =78000 =220 =78000       # Farther       (Corner band 6)

/tp <you> =6000  =220 =512000      # 64-bit        (Edge W band 0)
/tp <you> =78000 =220 =512000      # Tunnel        (Edge W band 6)
```

## Upgrade notes

- **Existing worlds**: chunks already generated under v0.1.0 keep their original
  layout. Only newly generated chunks use the v0.1.1 ring. A world started on
  v0.1.0 will therefore have two overlapping layouts at the boundary of the
  old explored area unless you regenerate.
- **Custom `VSFL_DEPTH` users**: if you set this env var in v0.1.0, your value
  still wins. Auto-scale only fills in the default.

## Asset

`vsmodfarlands_0.1.1.zip` — 18 731 bytes
SHA-256: `0a535407306fed5fa3307684d3b4cea87a80d80dbf19b94b7a70a61d413e897b`

## Compatibility

- Vintage Story 1.22.2.
- Server-side only, no client install required.
- Same stock-block palette as v0.1.0 (rock, basalt, snowblock, grass).
- Safe to add or remove on an existing world.

## License

MIT. See [LICENSE](https://github.com/Eloi-Desbrosses/VsModFarlands/blob/main/LICENSE).
