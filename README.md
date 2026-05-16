# Far Lands

A server-side Vintage Story mod that injects 14 different glitched terrain
biomes into a ring around the world border, faithfully recreating the iconic
Minecraft Far Lands.

## What it does

Every chunk whose closest world border is within `VSFL_DEPTH` blocks (default
210 000) is rewritten in a worldgen pass with one of 14 distinct terrain masks.
The mask is picked from two parameters:

- **Distance band**: 7 bands of `VSFL_BAND` blocks each (default 30 000). Band
  0 sits right at the border (the most extreme glitch); band 6 sits furthest
  inland (the mildest, blending toward vanilla).
- **Corner vs Edge**: chunks close to *two* borders use the **Corner** series;
  chunks close to *one* border use the **Edge** series. Edge patterns are
  axis-aware — tunnels and stripes align with the closest border axis.

| Band | Corner type     | Edge type   |
|------|-----------------|-------------|
| 0    | Nothingness     | 64-bit      |
| 1    | Skygrid         | NetherGrid  |
| 2    | EndIsland       | Vertex      |
| 3    | Stripe          | Comb        |
| 4    | Strip           | Pole        |
| 5    | Corner (Stack)  | Edge (Loop) |
| 6    | Farther         | Tunnel      |

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

Three environment variables tune the ring before launch. All values are
integers in blocks.

| Variable           | Default  | Meaning                                        |
|--------------------|----------|------------------------------------------------|
| `VSFL_DEPTH`       | 210 000  | Total ring thickness from each border inwards. |
| `VSFL_BAND`        | 30 000   | Width of each of the 7 concentric bands.       |
| `VSFL_TUNNEL_LIFT` | 40       | How many blocks Tunnel raises vanilla terrain. |

`VSFL_DEPTH / VSFL_BAND` should equal 7 for the canonical 14-type layout. Set
a smaller depth to push the Far Lands tight to the border, or a wider band to
spread each biome further inland.

The default Vintage Story world is 1 024 000 × 1 024 000 blocks centered at
(512 000, 512 000). With defaults you can find every biome between coords
5 000 and 200 000 along either axis.

## Suggested coordinates

Replace `<player>` with your in-game name.

```
/tp <player> =5000 =220 =5000        # Nothingness   (Corner band 0)
/tp <player> =40000 =220 =40000      # Skygrid       (Corner band 1, the iconic one)
/tp <player> =70000 =220 =70000      # EndIsland     (Corner band 2)
/tp <player> =100000 =220 =100000    # Stripe        (Corner band 3)
/tp <player> =130000 =220 =130000    # Strip         (Corner band 4)
/tp <player> =160000 =220 =160000    # Corner Stack  (Corner band 5)
/tp <player> =200000 =220 =200000    # Farther       (Corner band 6)

/tp <player> =5000 =220 =512000      # 64-bit        (Edge W band 0)
/tp <player> =40000 =220 =512000     # NetherGrid    (Edge W band 1)
/tp <player> =70000 =220 =512000     # Vertex        (Edge W band 2)
/tp <player> =100000 =220 =512000    # Comb          (Edge W band 3)
/tp <player> =130000 =220 =512000    # Pole          (Edge W band 4)
/tp <player> =160000 =220 =512000    # Edge Loop     (Edge W band 5)
/tp <player> =200000 =220 =512000    # Tunnel        (Edge W band 6)
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
