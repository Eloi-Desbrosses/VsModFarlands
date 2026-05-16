# Far Lands

*Fourteen flavours of broken terrain, waiting just past the edge of the map.*

In 2010, a 32-bit floating-point bug in Minecraft Beta produced the Far Lands: a strip of corrupted, impossible terrain starting exactly 12 550 821 blocks from spawn. Walls of stone. Floating grid cities. Hollow tunnels stacked to the sky. For a generation of players, *Far Lands or Bust* was a pilgrimage.

This mod brings that pilgrimage to Vintage Story.

---

## What you get

The Far Lands no longer hide millions of blocks away. They wrap your world as a ring around the map border, sized by a single in-game chat command: `/farlands coverage <0-100>`. The default 20% leaves vanilla as roughly 80% of the surface while still making the ring a serious border destination. Whatever you pick, the ring scales with your world size, so the same percentage feels proportionally similar on a 1M and a 65M world.

Within that ring, every chunk is rewritten by one of 14 distinct terrain generators, each modelled on a real Minecraft Far Lands variant (Beta Java, modern Bedrock, and a handful of classic engine-mod oddities).

Walk outward from the centre toward any border and you cross seven concentric bands, each one-seventh of the ring depth. Each band has a different glitch, escalating as you approach the edge of the world.

### The Corner series (two borders nearby)

| Band | Biome | Why it's broken |
|------|-------|-----------------|
| 6 | Farther | Stretched, taffy-like terrain bleeding back toward vanilla. |
| 5 | Corner (Stack) | Five horizontal slabs of terrain, separated by nothing. |
| 4 | Strip | Thin 1-block grid panels reaching to Y=200. |
| 3 | Stripe | Half the world is missing in a tight 3D checkerboard. |
| 2 | EndIsland | Hovering white chalk discs in a black sky. |
| 1 | Skygrid | Floating 4×4×8 grass cubes. The iconic one. |
| 0 | Nothingness | Pure void from bedrock to sky. Don't fall. |

### The Edge series (one border nearby)

| Band | Biome | Why it's broken |
|------|-------|-----------------|
| 6 | Tunnel | Vanilla terrain (caves, ores and all) lifted 40 blocks and repeated forever along the border axis. |
| 5 | Edge (Loop) | The original Beta wall, pierced with horizontal tunnels. |
| 4 | Pole | Alternating 32-block walls and 32-block gaps. |
| 3 | Comb | Stone teeth, one column out of four. |
| 2 | Vertex | Chaotic granular salt-and-pepper. |
| 1 | NetherGrid | A spaced 8×8×4 basalt lattice. Walkable, just barely. |
| 0 | 64-bit | Maximum-entropy noise. The world looks like static. |

Edge patterns are axis-aware: tunnels and stripes always align with the closest border. The Tunnel biome feels like the world itself has glitched and started looping.

---

## Where to find them

On the default 1 024 000 × 1 024 000 world at the 20% default coverage, the ring is about 54 000 blocks deep from each border, with bands of roughly 7 700 blocks each. Every Far Lands biome sits between coordinate 0 and 54 000 of any axis on that world. Three sample teleports:

```
/tp <you> =11500 =220 =11500      -> Skygrid       (Corner band 1)
/tp <you> =50000 =220 =512000     -> Tunnel        (Edge band 6)
/tp <you> =3800  =220 =3800       -> Nothingness   (Corner band 0)
```

The center of the world stays vanilla. Players who never wander far see no glitched chunks, and pay zero worldgen cost for the mod. If you set the coverage higher, the ring deepens proportionally and the band coordinates above shift outward.

---

## Installation

Server-side only. Drop the released `.zip` into your `Mods/` folder.

- **Single player**: `%APPDATA%/VintagestoryData/Mods/` (Windows) or `~/.config/VintagestoryData/Mods/` (Linux).
- **Dedicated server**: the server's `Mods/` folder.

Clients can join a dedicated server **without** installing the mod — every generated block is vanilla, and `requiredOnClient: false` is set in the mod metadata. The mod writes no custom data to the save, so you can add or remove it on an existing world: already-generated chunks stay as they were.

---

## Configuration

Once you've joined the world, set coverage with an in-game chat command:

```
/farlands coverage <0-100>      -> set coverage, persist to the savegame
/farlands status                -> show current coverage and full ring parameters
```

Requires the `controlserver` privilege (admin / single-player host). The default until you change it is `20%`. The value persists in the savegame so it survives restarts. Newly generated chunks use the new ring; already-generated chunks stay as they were.

Pick `0` to disable the mod for a specific world. Pick `100` if you want the entire map to be glitched. Most playthroughs land at `20` or `40`.

For dedicated servers or Docker deployments where running a chat command on every boot is impractical, three environment variables override the chat-command value at startup: `VSFL_DEPTH` (ring thickness from each border in blocks), `VSFL_BAND` (band width, defaults to `VSFL_DEPTH / 7`), and `VSFL_TUNNEL_LIFT` (Tunnel raise height, default 40). The chat command itself ignores env vars, so a runtime `/farlands coverage` call always takes effect immediately; on the next restart the env vars reapply.

---

## Performance

Chunks outside the ring are untouched and cost nothing.

Chunks inside the ring add roughly 10 to 50 ms of worldgen per chunk on a modern CPU, depending on the biome. That cost is paid once, at chunk generation, and never again: the masks bake straight into the saved chunk data. Worldgen runs on background worker threads, so it never blocks the main game loop.

---

## Compatibility

Built and tested against Vintage Story 1.22.2. Server-side only. No conflicts expected: the mod writes blocks during chunk generation on a single pass (`Vegetation`), after vanilla terrain has been laid down. It does not subscribe to player events, tick handlers, or networking hooks, and adds only two admin chat subcommands (`/farlands coverage`, `/farlands status`).

Mods that add biomes or alter base terrain compose cleanly with this one. The Far Lands ring runs after them, so their content is what gets glitched.

---

## Source & license

MIT-licensed, open source.

Suggestions, bug reports, and screenshots of weird things you found at world's edge are all welcome.

## AI Use disclosure

The code for this mod has been 99% generated by Claude Opus 4.7 then reviewed by a software engineer (me). I'll be honest, the code could be better architectured, simplified, and faster; but I only made this mod for a quick fun afternoon session with friends so I don't care. I recommend forking this mod if you wanna improve on it.