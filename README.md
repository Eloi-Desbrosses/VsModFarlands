# Far Lands

A server-side Vintage Story mod that injects 14 different glitched terrain
biomes into a ring around the world border, faithfully recreating the iconic
Minecraft Far Lands.

## What it does

Every chunk whose closest world border is within `VSFL_DEPTH` blocks is rewritten
in a worldgen pass with one of 14 distinct terrain masks. The mask is picked
from two parameters:

- **Distance band**: 7 bands of `VSFL_BAND` blocks each. Band 6 sits furthest
  inland (the mildest, blending toward vanilla); band 0 sits right at the
  border (the most extreme glitch). The table below is ordered the way a player
  encounters them: outward from spawn toward the edge.
- **Corner vs Edge**: chunks close to *two* borders use the **Corner** series;
  chunks close to *one* border use the **Edge** series. Edge patterns are
  axis-aware — tunnels and stripes align with the closest border axis.

| Band | Corner type     | Edge type   |
|------|-----------------|-------------|
| 6    | Farther         | Tunnel      |
| 5    | Corner (Stack)  | Edge (Loop) |
| 4    | Strip           | Pole        |
| 3    | Stripe          | Comb        |
| 2    | EndIsland       | Vertex      |
| 1    | Skygrid         | NetherGrid  |
| 0    | Nothingness     | 64-bit      |

The Tunnel band uniquely takes vanilla terrain (including any caves rolled by
the rest of the worldgen pipeline), lifts it 40 blocks, and repeats it along
the border axis — a literal "world raised and stuttering" effect.

## Installation

1. Drop the released `.zip` into `%APPDATA%/VintagestoryData/Mods/` on Windows
   (or `~/.config/VintagestoryData/Mods/` on Linux).
2. Start any world. The mod is **server-side only** — clients do not need it.

The first chunk inside the ring takes a fraction of a second longer to
generate than vanilla; chunks past the ring are untouched.

## Configuration

The ring auto-scales with your world size — by default Far Lands cover ~30% of
the map surface, leaving ~70% vanilla. Three environment variables override the
defaults if you want a different ratio. All values are integers in blocks.

| Variable           | Default                              | Meaning                                        |
|--------------------|--------------------------------------|------------------------------------------------|
| `VSFL_DEPTH`       | `min(mapX, mapZ) / 12` (clamped 60k–2M) | Total ring thickness from each border inwards. |
| `VSFL_BAND`        | `VSFL_DEPTH / 7`                     | Width of each of the 7 concentric bands.       |
| `VSFL_TUNNEL_LIFT` | 40                                   | How many blocks Tunnel raises vanilla terrain. |

The default Vintage Story world is 1 024 000 × 1 024 000 blocks centered at
(512 000, 512 000). On that world the ring is **85 000 blocks deep** from each
border, with **bands of ~12 200 blocks** each. Every biome sits between
coordinate 0 and 85 000 along either axis.

For larger worlds the ring grows proportionally up to a 2 000 000-block cap;
for smaller worlds the ring floors at 60 000 blocks so the bands stay wide
enough to read.

## Suggested coordinates

Replace `<player>` with your in-game name. Coordinates below target the
centre of each band on the default 1 024 000-block world.

```
/tp <player> =6000  =220 =6000       # Nothingness   (Corner band 0)
/tp <player> =18000 =220 =18000      # Skygrid       (Corner band 1, the iconic one)
/tp <player> =30000 =220 =30000      # EndIsland     (Corner band 2)
/tp <player> =42000 =220 =42000      # Stripe        (Corner band 3)
/tp <player> =54000 =220 =54000      # Strip         (Corner band 4)
/tp <player> =66000 =220 =66000      # Corner Stack  (Corner band 5)
/tp <player> =78000 =220 =78000      # Farther       (Corner band 6)

/tp <player> =6000  =220 =512000     # 64-bit        (Edge W band 0)
/tp <player> =18000 =220 =512000     # NetherGrid    (Edge W band 1)
/tp <player> =30000 =220 =512000     # Vertex        (Edge W band 2)
/tp <player> =42000 =220 =512000     # Comb          (Edge W band 3)
/tp <player> =54000 =220 =512000     # Pole          (Edge W band 4)
/tp <player> =66000 =220 =512000     # Edge Loop     (Edge W band 5)
/tp <player> =78000 =220 =512000     # Tunnel        (Edge W band 6)
```

## Building from source

Requires Docker.

```
docker compose run --rm build
```

Produces `build/VsModFarlands.dll` + `build/modinfo.json`. Package as a `.zip`
with both at the archive root.

## License

MIT. See [LICENSE](LICENSE).
