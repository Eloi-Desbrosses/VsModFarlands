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

When you create a new world, the **Customize** screen has a single slider:

> **Far Lands coverage (%)** — 0 to 100, default **30**.

This is the fraction of your map's surface that the Far Lands ring will cover.
The ring is anchored at each world border and scales with your world size, so
30% on a 1 024 000-block world and 30% on a 65 000 000-block world both feel
proportionally similar — a border crust occupying the same fraction of the
playable surface.

| Coverage | What you get |
|----------|--------------|
| 0%       | Mod is effectively disabled for this world. |
| 10%      | Thin border crust. You'll only notice the Far Lands if you deliberately walk to a border. |
| 30%      | Recommended. Far Lands feel like a strong border phenomenon; vanilla still dominates the playable area (~70%). |
| 50%      | Far Lands and vanilla are roughly equal. The ring is a major destination. |
| 100%     | Every chunk of the world is a Far Lands biome. No vanilla center exists. |

On the default 1 024 000-block world at 30% coverage, the ring is about
85 000 blocks deep from each border, with bands of about 12 200 blocks each.
Every biome sits between coordinate 0 and 85 000 along either axis.

### Advanced overrides (servers, Docker)

For headless / dedicated deployments where the customize-world UI isn't
practical, three environment variables override the slider:

| Variable           | Default                          | Meaning                                        |
|--------------------|----------------------------------|------------------------------------------------|
| `VSFL_DEPTH`       | derived from coverage % + world  | Total ring thickness from each border inwards. |
| `VSFL_BAND`        | `VSFL_DEPTH / 7`                 | Width of each of the 7 concentric bands.       |
| `VSFL_TUNNEL_LIFT` | 40                               | How many blocks Tunnel raises vanilla terrain. |

Env vars take precedence over the world-config slider when present.

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
