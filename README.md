# Far Lands

A Vintage Story mod with server-authoritative worldgen that injects 14
different glitched terrain biomes into a ring around the world border,
faithfully recreating the iconic Minecraft Far Lands.

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

Server-side only. Drop the released `.zip` into your `Mods/` folder:

- **Single player**: `%APPDATA%/VintagestoryData/Mods/` on Windows,
  `~/.config/VintagestoryData/Mods/` on Linux.
- **Dedicated server**: the server's `Mods/` folder.

Clients can join a dedicated server **without** installing the mod —
every generated block is a vanilla block, and `requiredOnClient: false`
is set in the mod metadata.

The first chunk inside the ring takes a fraction of a second longer to
generate than vanilla; chunks past the ring are untouched.

## Configuration

Coverage is set with an in-game chat command after you've joined the
world. Default is `20%` until you change it.

```
/farlands coverage <0-100>      # set coverage and persist to world config
/farlands status                # show current coverage and full ring parameters
```

Requires the `controlserver` privilege (admin / single-player host). The
value persists in the savegame, so it survives restarts. Newly generated
chunks use the new ring; already-generated chunks stay as they were.

| Coverage | What you get |
|----------|--------------|
| 0%       | Mod is effectively disabled for this world. |
| 20% (default) | Far Lands feel like a strong border phenomenon; vanilla dominates the playable area (~80%). |
| 40%      | Border ring is roughly half the playable surface. |
| 60–80%   | Vanilla becomes the minority biome. |
| 100%     | Every chunk of the world is a Far Lands biome. No vanilla center exists. |

The ring scales with your world size, so the same percentage on a
1 024 000-block world and a 65 000 000-block world both feel
proportionally similar — a border crust occupying the same fraction of
the playable surface. On the default 1 024 000-block world at 20%
coverage, the ring is about 54 000 blocks deep from each border, with
bands of about 7 700 blocks each.

### Advanced overrides (servers, Docker)

For headless / dedicated deployments where running a chat command on
every boot is impractical, three environment variables override the
chat-command value at startup:

| Variable           | Default                          | Meaning                                        |
|--------------------|----------------------------------|------------------------------------------------|
| `VSFL_DEPTH`       | derived from coverage % + world  | Total ring thickness from each border inwards. |
| `VSFL_BAND`        | `VSFL_DEPTH / 7`                 | Width of each of the 7 concentric bands.       |
| `VSFL_TUNNEL_LIFT` | 40                               | How many blocks Tunnel raises vanilla terrain. |

Env vars take precedence over the persisted coverage value at every
server start. The chat command itself ignores env vars, so a runtime
`/farlands coverage` call always takes effect immediately; on the next
restart the env vars will reapply.

## Suggested coordinates

Replace `<player>` with your in-game name. Coordinates below target the
centre of each band on the default 1 024 000-block world **at the 20%
default coverage** (ring depth ~54 000 blocks, band width ~7 700 blocks).
If you change the coverage, the band centres shift outward proportionally.

```
/tp <player> =3800  =220 =3800       # Nothingness   (Corner band 0)
/tp <player> =11500 =220 =11500      # Skygrid       (Corner band 1, the iconic one)
/tp <player> =19200 =220 =19200      # EndIsland     (Corner band 2)
/tp <player> =26900 =220 =26900      # Stripe        (Corner band 3)
/tp <player> =34600 =220 =34600      # Strip         (Corner band 4)
/tp <player> =42300 =220 =42300      # Corner Stack  (Corner band 5)
/tp <player> =50000 =220 =50000      # Farther       (Corner band 6)

/tp <player> =3800  =220 =512000     # 64-bit        (Edge W band 0)
/tp <player> =11500 =220 =512000     # NetherGrid    (Edge W band 1)
/tp <player> =19200 =220 =512000     # Vertex        (Edge W band 2)
/tp <player> =26900 =220 =512000     # Comb          (Edge W band 3)
/tp <player> =34600 =220 =512000     # Pole          (Edge W band 4)
/tp <player> =42300 =220 =512000     # Edge Loop     (Edge W band 5)
/tp <player> =50000 =220 =512000     # Tunnel        (Edge W band 6)
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
