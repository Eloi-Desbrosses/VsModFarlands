# Far Lands

*Fourteen flavours of broken terrain, waiting just past the edge of the map.*

In 2010, a 32-bit floating-point bug in Minecraft Beta produced the Far Lands: a strip of corrupted, impossible terrain starting exactly 12 550 821 blocks from spawn. Walls of stone. Floating grid cities. Hollow tunnels stacked to the sky. For a generation of players, *Far Lands or Bust* was a pilgrimage.

This mod brings that pilgrimage to Vintage Story.

---

## What you get

The Far Lands no longer hide millions of blocks away. They wrap your world as a ring around the map border, sized by a single dropdown on the world-creation screen: **Far Lands Coverage**, with six steps from 0% (disabled) to 100% (entire world). The default 20% leaves vanilla as roughly 80% of the surface while still making the ring a serious border destination. Whatever you pick, the ring scales with your world size, so the same percentage feels proportionally similar on a 1M and a 65M world.

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

## Install location

Worldgen runs server-authoritatively, but the install location depends on how you play.

- **Single player**: drop `VsModFarlands.dll` into your local `%APPDATA%/VintagestoryData/Mods/` (Windows) or `~/.config/VintagestoryData/Mods/` (Linux). In single player the embedded server runs in the same process as the client, so a single install handles both. The Customize-World dropdown only renders when the file is installed as a **raw DLL** at this path — VS does not scan folder/ZIP mods client-side at world creation.
- **Dedicated server**: drop the released `.zip` into the server's `Mods/` folder. Clients can join **without** installing the mod locally — every generated block is a vanilla block, `requiredOnClient: false` is set in the mod metadata, and the mod adds no commands, no permissions, no networking. Coverage is picked by the admin in the Customize-World UI when the world is first created, or via the env-var overrides described below.

The mod writes no custom data to the save, so you can add or remove it on an existing world: already-generated chunks stay as they were.

---

## Configuration

A single dropdown on the **Customize World** screen controls everything:

| Setting | Values | Default | Meaning |
|---------|--------|---------|---------|
| Far Lands Coverage | `0%`, `20%`, `40%`, `60%`, `80%`, `100%` | `20%` | Fraction of map surface covered by Far Lands biomes. Scales with world size. |

Pick `0%` to disable the mod for a specific world. Pick `100%` if you want the entire map to be glitched. Most playthroughs land at `20%` or `40%`.

For dedicated servers or Docker deployments where the UI isn't practical, three environment variables override the dropdown: `VSFL_DEPTH` (ring thickness from each border in blocks), `VSFL_BAND` (band width, defaults to `VSFL_DEPTH / 7`), and `VSFL_TUNNEL_LIFT` (Tunnel raise height, default 40).

### In-game chat command

If you installed the mod via a 1-click installer that ships it as a ZIP, the Customize-World dropdown won't render in single player because VS doesn't scan ZIP/folder mods client-side at world creation. A chat command exists for that case (and for any later change at runtime):

```
/farlands coverage              -> show current value
/farlands coverage <0-100>      -> set coverage, persist to world config
/farlands status                -> full ring parameters (depth, band width, …)
```

Requires the `controlserver` privilege. The new value persists in the world config so it survives restarts. Newly generated chunks use the new ring; already-generated chunks stay as they were.

### Label rendering

Vintage Story renders the dropdown's label and values through its translation system, looking up keys like `worldattribute-Far Lands Coverage`. For raw-DLL mods like this one, VS reads the assembly's mod metadata via reflection without fully loading the assembly client-side before the Customize-World screen renders, so the runtime translation injection we ship can't fire in time. You'll see:

- Label as `worldattribute-Far Lands Coverage`
- Values as `worldconfig-Far Lands Coverage-0%`, `…-20%`, etc.

The text after the prefix is readable as the actual setting. If you want the prefixes gone, drop our 13 translation entries into `<Vintagestory>/assets/game/lang/en.json` (additive, gets wiped on VS update).

---

## Performance

Chunks outside the ring are untouched and cost nothing.

Chunks inside the ring add roughly 10 to 50 ms of worldgen per chunk on a modern CPU, depending on the biome. That cost is paid once, at chunk generation, and never again: the masks bake straight into the saved chunk data. Worldgen runs on background worker threads, so it never blocks the main game loop.

---

## Compatibility

Built and tested against Vintage Story 1.22.2. Worldgen runs server-authoritatively (see install section above for where to put the file). No conflicts expected: the mod writes blocks during chunk generation on a single pass (`Vegetation`), after vanilla terrain has been laid down. It does not subscribe to player events, tick handlers, or networking hooks.

Mods that add biomes or alter base terrain compose cleanly with this one. The Far Lands ring runs after them, so their content is what gets glitched.

---

## Source & license

MIT-licensed, open source.

Suggestions, bug reports, and screenshots of weird things you found at world's edge are all welcome.

## AI Use disclosure

The code for this mod has been 99% generated by Claude Opus 4.7 then reviewed by a software engineer (me). I'll be honest, the code could be better architectured, simplified, and faster; but I only made this mod for a quick fun afternoon session with friends so I don't care. I recommend forking this mod if you wanna improve on it.