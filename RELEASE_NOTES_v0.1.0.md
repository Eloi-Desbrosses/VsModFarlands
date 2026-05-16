# v0.1.0 — Initial release

Adds 14 Minecraft Far Lands biomes to Vintage Story as a worldgen ring around the world border.

## Install

**Singleplayer:** drop `vsmodfarlands_0.1.0.zip` into `%APPDATA%/VintagestoryData/Mods/` on Windows, or `~/.config/VintagestoryData/Mods/` on Linux. Start any world.

**Dedicated server:** same path on the server install. Clients do not install the mod and never download it.

## What's in this release

- 14 distinct terrain generators: Nothingness, Skygrid, EndIsland, Stripe, Strip, Corner Stack, Farther, 64-bit, NetherGrid, Vertex, Comb, Pole, Edge Loop, Tunnel.
- Two axis-aware layouts: a Corner series triggers near two borders at once, an Edge series near a single border.
- Tunnel biome lifts vanilla terrain by 40 blocks and repeats it along the closest border axis.
- Configurable ring via `VSFL_DEPTH`, `VSFL_BAND`, `VSFL_TUNNEL_LIFT` environment variables.
- Water cleanup pass on every biome except Tunnel (where the water is part of the lifted pattern).

## Compatibility

- Vintage Story 1.22.2.
- Server-side only. Players need nothing installed.
- Uses stock Vintage Story blocks (rock, basalt, snowblock, grass). No missing-block warnings on connecting clients.
- Safe to add or remove on an existing world: already-generated chunks are left as they were, only new chunks inside the ring are masked.

## Where to find the biomes

The default Vintage Story world is 1 024 000 × 1 024 000 blocks. Every Far Lands biome sits between coordinate 5 000 and 200 000 of any axis. Three quick samples:

```
/tp <you> =40000  =220 =40000     -> Skygrid       (Corner band 1)
/tp <you> =200000 =220 =512000    -> Tunnel        (Edge band 6)
/tp <you> =5000   =220 =5000      -> Nothingness   (Corner band 0)
```

The full 14-biome coordinate list lives in the [README](https://github.com/Eloi-Desbrosses/VsModFarlands/blob/main/README.md).

## Performance

Chunks outside the ring are untouched and cost nothing. Chunks inside add roughly 10 to 50 ms of worldgen on a modern CPU, paid once at chunk generation and baked into the saved chunk data. Worldgen runs on background worker threads.

## Asset

`vsmodfarlands_0.1.0.zip` — 18 456 bytes
SHA-256: `ba2ff2f4b8ef51eced7bc128cc3882296a092418ac1412aaed5683e1719f7fdf`

## Known limitations

- Tunnel biome snapshots a single vanilla column per chunk; chunk-edge seams can appear where adjacent chunks pick neighbouring columns. Visual only, not gameplay-affecting.
- Tested on Vintage Story 1.22.2. Other 1.22.x patches likely work but are not validated.

## License

MIT. See [LICENSE](https://github.com/Eloi-Desbrosses/VsModFarlands/blob/main/LICENSE).
