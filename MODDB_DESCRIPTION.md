# Far Lands

*Fourteen flavours of broken terrain, waiting just past the edge of the map.*

In 2010, a 32-bit floating-point bug in Minecraft Beta produced the Far Lands: a strip of corrupted, impossible terrain starting exactly 12 550 821 blocks from spawn. Walls of stone. Floating grid cities. Hollow tunnels stacked to the sky. For a generation of players, *Far Lands or Bust* was a pilgrimage.

This mod brings that pilgrimage to Vintage Story.

---

## What you get

The Far Lands no longer hide millions of blocks away. They wrap your world as a 210 000-block ring around the map border. Within that ring, every chunk is rewritten by one of 14 distinct terrain generators, each modelled on a real Minecraft Far Lands variant (Beta Java, modern Bedrock, and a handful of classic engine-mod oddities).

Walk inland from any border and you cross seven concentric bands. Each band is 30 000 blocks wide. Each band has a different glitch.

### The Corner series (two borders nearby)

| Band | Biome | Why it's broken |
|------|-------|-----------------|
| 0 | Nothingness | Pure void from bedrock to sky. Don't fall. |
| 1 | Skygrid | Floating 4×4×8 grass cubes. The iconic one. |
| 2 | EndIsland | Hovering white chalk discs in a black sky. |
| 3 | Stripe | Half the world is missing in a tight 3D checkerboard. |
| 4 | Strip | Thin 1-block grid panels reaching to Y=200. |
| 5 | Corner (Stack) | Five horizontal slabs of terrain, separated by nothing. |
| 6 | Farther | Stretched, taffy-like terrain bleeding back toward vanilla. |

### The Edge series (one border nearby)

| Band | Biome | Why it's broken |
|------|-------|-----------------|
| 0 | 64-bit | Maximum-entropy noise. The world looks like static. |
| 1 | NetherGrid | A spaced 8×8×4 basalt lattice. Walkable, just barely. |
| 2 | Vertex | Chaotic granular salt-and-pepper. |
| 3 | Comb | Stone teeth, one column out of four. |
| 4 | Pole | Alternating 32-block walls and 32-block gaps. |
| 5 | Edge (Loop) | The original Beta wall, pierced with horizontal tunnels. |
| 6 | Tunnel | Vanilla terrain (caves, ores and all) lifted 40 blocks and repeated forever along the border axis. |

Edge patterns are axis-aware: tunnels and stripes always align with the closest border. The Tunnel biome feels like the world itself has glitched and started looping.

---

## Where to find them

The default Vintage Story world is 1 024 000 × 1 024 000 blocks. With the ring on by default, every Far Lands biome sits between coordinate 5 000 and 200 000 of any axis.

```
/tp <you> =40000  =220 =40000     -> Skygrid       (Corner band 1)
/tp <you> =200000 =220 =512000    -> Tunnel        (Edge band 6)
/tp <you> =5000   =220 =5000      -> Nothingness   (Corner band 0)
```

The center of the world stays vanilla. Players who never wander far see no glitched chunks, and pay zero worldgen cost for the mod.

---

## Multiplayer / dedicated server

**Server-side only.** Drop the zip into your server's `Mods/` folder. That's all.

Clients do not install the mod and never even download it. All 14 biomes use stock Vintage Story blocks (rock, basalt, snowblock, grass), so connecting players see no missing-block warnings. The mod adds no commands, no permissions, no networking. It writes no custom data to the save, so you can add or remove it on an existing world: already-generated chunks stay as they were.

---

## Configuration

Three environment variables, set on the server before launch:

| Variable | Default | Meaning |
|----------|---------|---------|
| `VSFL_DEPTH` | 210 000 | Total ring thickness from each border. |
| `VSFL_BAND`  | 30 000  | Width of each of the 7 concentric bands. |
| `VSFL_TUNNEL_LIFT` | 40 | Blocks the Tunnel biome raises vanilla terrain by. |

To pull the Far Lands tight against the border, drop `VSFL_DEPTH` to 70 000. For wider, milder bands, raise `VSFL_BAND` and lower `VSFL_DEPTH` to keep the 7:1 ratio.

---

## Performance

Chunks outside the ring are untouched and cost nothing.

Chunks inside the ring add roughly 10 to 50 ms of worldgen per chunk on a modern CPU, depending on the biome. That cost is paid once, at chunk generation, and never again: the masks bake straight into the saved chunk data. Worldgen runs on background worker threads, so it never blocks the main game loop.

---

## Compatibility

Built and tested against Vintage Story 1.22.2. Server side only. No conflicts expected: the mod writes blocks during chunk generation on a single pass (`Vegetation`), after vanilla terrain has been laid down. It does not subscribe to player events, tick handlers, or networking hooks.

Mods that add biomes or alter base terrain compose cleanly with this one. The Far Lands ring runs after them, so their content is what gets glitched.

---

## Source & license

MIT-licensed, open source.

Suggestions, bug reports, and screenshots of weird things you found at world's edge are all welcome.
