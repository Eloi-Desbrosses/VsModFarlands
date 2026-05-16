using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VsModFarlands;

public enum FarType
{
    None,
    Edge,           // Java Beta — Loop: wall + tunnels perpendicular
    Corner,         // Java Beta — Stack: 5 strata Y
    Vertex,         // Java Beta — Abyss: chaos
    Farther,        // Java — stretched diagonal
    Tunnel,         // Bedrock PE — flat ceiling
    Comb,           // Bedrock — combs, 1 column out of 4
    Pole,           // Bedrock — guillotine
    Strip,          // Bedrock — 1D panels
    Stripe,         // Bedrock — 1 bloc sur 2
    Nothingness,    // Bedrock — void total
    Skygrid,        // Bedrock — 4×4×8 ICONIQUE
    NetherGrid,     // Bedrock — 8×8×4
    EndIsland,      // Bedrock — chalk discs
    SixtyFourBit    // Java mod — extreme chaos
}

public class VsModFarlandsSystem : ModSystem
{
    private const int ChunkSize = GlobalConstants.ChunkSize; // 32

    private ICoreServerAPI? _sapi;
    private bool _ready;

    // Block IDs
    private int _air;
    private int _stone;
    private int _basalt;
    private int _chalk;
    private int _grass;
    private int _water;

    // Configuration: distance from world BORDER
    private int _mapSizeX;
    private int _mapSizeZ;
    private int _depth;     // total Far Lands ring thickness from border (inwards)
    private int _bandWidth; // width of each concentric band
    private int _numBands;
    private int _tunnelLift; // how high Tunnel Lands raise vanilla terrain

    // 7 bands × Corner = 7 types; 7 bands × Edge = 7 types. Total 14.
    // Band 0 = at border = most extreme; band 6 = mildest, closest to vanilla.
    private static readonly FarType[] CornerTypes =
    {
        FarType.Nothingness,  // band 0 — at the corner intersection
        FarType.Skygrid,      // band 1
        FarType.EndIsland,    // band 2
        FarType.Stripe,       // band 3
        FarType.Strip,        // band 4
        FarType.Corner,       // band 5 — the canonical "Stack"
        FarType.Farther       // band 6 — mildest, transitioning to vanilla
    };

    private static readonly FarType[] EdgeTypes =
    {
        FarType.SixtyFourBit, // band 0 — wall at the very edge
        FarType.NetherGrid,   // band 1
        FarType.Vertex,       // band 2
        FarType.Comb,         // band 3
        FarType.Pole,         // band 4
        FarType.Edge,         // band 5 — the canonical "Loop"
        FarType.Tunnel        // band 6 — mildest
    };

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

    public override double ExecuteOrder() => 1.0;

    public override void StartServerSide(ICoreServerAPI api)
    {
        _sapi = api;
        _tunnelLift = ReadIntEnv("VSFL_TUNNEL_LIFT", 40);

        api.Event.ChunkColumnGeneration(OnChunkColumnGen, EnumWorldGenPass.Vegetation, "standard");
        api.Event.ServerRunPhase(EnumServerRunPhase.RunGame, OnReady);

        RegisterCommands(api);
    }

    private void OnReady()
    {
        var api = _sapi!;
        _mapSizeX = api.WorldManager.MapSizeX;
        _mapSizeZ = api.WorldManager.MapSizeZ;

        int coveragePct = ReadCoveragePctFromConfig();
        RecomputeRing(coveragePct, useEnvOverrides: true);

        _air = 0;
        _stone  = Resolve("rock-granite", "rock-andesite");
        _basalt = Resolve("rock-basalt", "rock-andesite", "rock-granite");
        _chalk  = Resolve("snowblock", "rock-chalk", "rock-granite");
        _grass  = Resolve("forestfloor-1", "forestfloor-2", "rock-granite");
        _water  = Resolve("water-still-7", "water-flowing-7");

        api.Logger.Notification(
            "[FarLands] coverage={0}% mapSize=({1},{2}) depth={3} band={4} numBands={5}",
            coveragePct, _mapSizeX, _mapSizeZ, _depth, _bandWidth, _numBands);
        api.Logger.Notification(
            "[FarLands] ring on x in [0,{0}] u [{1},{2}], same on z",
            _depth, _mapSizeX - _depth, _mapSizeX);
        api.Logger.Debug(
            "[FarLands] block ids: air={0} stone={1} basalt={2} chalk={3} grass={4} water={5}",
            _air, _stone, _basalt, _chalk, _grass, _water);
        _ready = true;
    }

    /// <summary>
    /// Reads the persisted coverage % from the savegame's worldconfig. Set
    /// either by the Customize World UI (dropdown) at creation, or by
    /// /farlands coverage at runtime. Defaults to 20 when the key is absent.
    /// Reads SaveGame.WorldConfiguration (the canonical persisted tree) to
    /// match the vanilla VS pattern in vsessentialsmod / vssurvivalmod.
    /// </summary>
    private int ReadCoveragePctFromConfig()
    {
        string raw = _sapi!.WorldManager.SaveGame.WorldConfiguration.GetString("Far Lands Coverage", "20");
        if (!int.TryParse(raw, out int pct)) pct = 20;
        return Math.Clamp(pct, 0, 100);
    }

    /// <summary>
    /// Re-derives ring depth, band width and band count from a coverage %.
    /// Math: vanilla side = world × sqrt(1 - C), so depth = world × (1 − √(1 − C)) / 2.
    /// When useEnvOverrides is true, VSFL_DEPTH / VSFL_BAND env vars win (startup
    /// path; useful for Docker/headless deployments). When false (chat command
    /// path), env vars are ignored so /farlands coverage always takes effect at
    /// runtime even on a server that set env vars.
    /// </summary>
    private void RecomputeRing(int coveragePct, bool useEnvOverrides)
    {
        double coverage = coveragePct / 100.0;
        int worldMin = Math.Min(_mapSizeX, _mapSizeZ);
        int derivedDepth = (int)(worldMin * (1.0 - Math.Sqrt(1.0 - coverage)) / 2.0);

        _depth     = useEnvOverrides ? ReadIntEnv("VSFL_DEPTH", derivedDepth) : derivedDepth;
        _bandWidth = useEnvOverrides ? ReadIntEnv("VSFL_BAND",  Math.Max(1, _depth / 7)) : Math.Max(1, _depth / 7);
        _numBands  = Math.Max(1, _depth / Math.Max(1, _bandWidth));
    }

    // ====================== chat commands ======================

    private void RegisterCommands(ICoreServerAPI api)
    {
        var parsers = api.ChatCommands.Parsers;

        api.ChatCommands.Create("farlands")
            .WithDescription("Far Lands ring controls")
            .RequiresPrivilege(Privilege.controlserver)
            .BeginSubCommand("coverage")
                .WithDescription("Show or set the Far Lands coverage percentage (0-100). Persists in the world config; future chunks use the new value, already-generated chunks stay as they were.")
                .WithArgs(parsers.OptionalIntRange("percent", 0, 100))
                .HandleWith(OnCmdCoverage)
            .EndSubCommand()
            .BeginSubCommand("status")
                .WithDescription("Show the current Far Lands ring parameters (coverage, depth, band width).")
                .HandleWith(OnCmdStatus)
            .EndSubCommand();
    }

    private TextCommandResult OnCmdCoverage(TextCommandCallingArgs args)
    {
        var api = _sapi!;
        var arg = args.Parsers[0].GetValue();

        if (arg == null)
        {
            // No argument → report current value.
            int current = ReadCoveragePctFromConfig();
            return TextCommandResult.Success(
                $"Far Lands coverage is {current}%. Depth {_depth} blocks, band width {_bandWidth} blocks. " +
                $"Use /farlands coverage <0-100> to change it.");
        }

        int newPct = Math.Clamp((int)arg, 0, 100);
        string newStr = newPct.ToString();

        // VS uses two worldconfig trees: WorldManager.SaveGame.WorldConfiguration
        // is the canonical persisted store that the savegame file serializes;
        // World.Config is a runtime mirror that VS populates from SaveGame on
        // world load but never syncs back on save. Writing only to World.Config
        // produces a runtime-visible change that is silently dropped on the
        // next world load. Write both: SaveGame for persistence, World.Config
        // so any code path that reads the live mirror (including our own
        // ReadCoveragePctFromConfig) sees the new value immediately.
        api.WorldManager.SaveGame.WorldConfiguration.SetString("Far Lands Coverage", newStr);
        api.World.Config.SetString("Far Lands Coverage", newStr);
        RecomputeRing(newPct, useEnvOverrides: false);

        api.Logger.Notification(
            "[FarLands] /farlands coverage set to {0}% by {1}. depth={2} band={3}",
            newPct, args.Caller.GetName(), _depth, _bandWidth);

        return TextCommandResult.Success(
            $"Far Lands coverage set to {newPct}% (depth {_depth} blocks, band width {_bandWidth} blocks). " +
            "Newly generated chunks will use the new value; already-generated chunks stay as they were.");
    }

    private TextCommandResult OnCmdStatus(TextCommandCallingArgs args)
    {
        int current = ReadCoveragePctFromConfig();
        return TextCommandResult.Success(
            $"Far Lands ring — coverage {current}%, map ({_mapSizeX}, {_mapSizeZ}), " +
            $"depth {_depth}, band width {_bandWidth}, {_numBands} bands, tunnel lift {_tunnelLift}.");
    }

    private int Resolve(params string[] codes)
    {
        var api = _sapi!;
        foreach (var c in codes)
        {
            var b = api.World.GetBlock(new AssetLocation(c));
            if (b != null) return b.Id;
        }
        api.Logger.Warning("[VsModFarlands] no block matched: {0}", string.Join(", ", codes));
        return 0;
    }

    // ====================== dispatch ======================

    private void OnChunkColumnGen(IChunkColumnGenerateRequest req)
    {
        if (!_ready) return;
        int wx = req.ChunkX * ChunkSize;
        int wz = req.ChunkZ * ChunkSize;

        int bx = Math.Min(wx, _mapSizeX - wx);
        int bz = Math.Min(wz, _mapSizeZ - wz);

        bool xClose = bx < _depth;
        bool zClose = bz < _depth;
        if (!xClose && !zClose) return; // vanilla center

        var chunks = req.Chunks;
        if (chunks == null || chunks.Length == 0) return;
        int totalY = chunks.Length * ChunkSize;

        FarType type;
        bool axisIsX;
        int distFromBorder;
        if (xClose && zClose)
        {
            // Corner: use MAX of both → distance from corner intersection
            distFromBorder = Math.Max(bx, bz);
            int band = Math.Min(distFromBorder / _bandWidth, CornerTypes.Length - 1);
            type = CornerTypes[band];
            axisIsX = bx <= bz; // tiebreaker, used only by Edge
        }
        else if (xClose)
        {
            distFromBorder = bx;
            int band = Math.Min(distFromBorder / _bandWidth, EdgeTypes.Length - 1);
            type = EdgeTypes[band];
            axisIsX = true;
        }
        else // zClose
        {
            distFromBorder = bz;
            int band = Math.Min(distFromBorder / _bandWidth, EdgeTypes.Length - 1);
            type = EdgeTypes[band];
            axisIsX = false;
        }

        Apply(req, type, axisIsX, totalY);
    }

    private void Apply(IChunkColumnGenerateRequest req, FarType type, bool axisIsX, int totalY)
    {
        var chunks = req.Chunks;
        int bx = req.ChunkX * ChunkSize;
        int bz = req.ChunkZ * ChunkSize;

        switch (type)
        {
            case FarType.Edge:         MaskEdge(chunks, bx, bz, axisIsX); break;
            case FarType.Corner:       MaskCorner(chunks, bx, bz, totalY); break;
            case FarType.Vertex:       MaskVertex(chunks, bx, bz, totalY); break;
            case FarType.Farther:      MaskFarther(chunks, bx, bz, totalY, axisIsX); break;
            case FarType.Tunnel:       MaskTunnel(chunks, totalY, axisIsX); break;
            case FarType.Comb:         MaskComb(chunks, bx, bz, totalY); break;
            case FarType.Pole:         MaskPole(chunks, bx, bz, totalY, axisIsX); break;
            case FarType.Strip:        MaskStrip(chunks, bx, bz, totalY); break;
            case FarType.Stripe:       MaskStripe(chunks, bx, bz, totalY); break;
            case FarType.Nothingness:  MaskNothingness(chunks, totalY); break;
            case FarType.Skygrid:      MaskSkygrid(chunks, bx, bz, totalY); break;
            case FarType.NetherGrid:   MaskNetherGrid(chunks, bx, bz, totalY); break;
            case FarType.EndIsland:    MaskEndIsland(chunks, bx, bz, totalY); break;
            case FarType.SixtyFourBit: MaskSixtyFourBit(chunks, bx, bz, totalY); break;
        }

        // Tunnel replicates vanilla including water as part of the pattern; all other masks post-clean.
        if (type != FarType.Tunnel) PostCleanWater(chunks);
    }

    // ====================== mask implementations ======================

    /// <summary>Edge (Loop): tunnels perpendicular to wall (= along the closest border axis).</summary>
    private void MaskEdge(IServerChunk[] chunks, int bx, int bz, bool axisIsX)
    {
        if (axisIsX)
        {
            // Wall faces X → tunnels run along X axis. At fixed (Y, Z), carve all X.
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int zMod = ModN(bz + lz, 16);
                if (zMod >= 3) continue;
                for (int wy = 1; wy < 200; wy++)
                {
                    int yMod = wy % 20;
                    if (yMod < 1 || yMod > 4) continue;
                    for (int lx = 0; lx < ChunkSize; lx++) Carve(chunks, lx, wy, lz);
                }
            }
        }
        else
        {
            // Wall faces Z → tunnels run along Z axis.
            for (int lx = 0; lx < ChunkSize; lx++)
            {
                int xMod = ModN(bx + lx, 16);
                if (xMod >= 3) continue;
                for (int wy = 1; wy < 200; wy++)
                {
                    int yMod = wy % 20;
                    if (yMod < 1 || yMod > 4) continue;
                    for (int lz = 0; lz < ChunkSize; lz++) Carve(chunks, lx, wy, lz);
                }
            }
        }
    }

    private void MaskCorner(IServerChunk[] chunks, int bx, int bz, int totalY)
    {
        ReadOnlySpan<int> bandCenters = stackalloc int[] { 30, 70, 110, 150, 190 };
        for (int lx = 0; lx < ChunkSize; lx++)
        {
            int wx = bx + lx;
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int wz = bz + lz;
                int wave = (int)(3 * Math.Sin(wx * 0.1) * Math.Cos(wz * 0.1));
                for (int wy = 1; wy < totalY; wy++)
                {
                    bool inBand = false;
                    foreach (var c in bandCenters)
                    {
                        int dy = wy - c - wave;
                        if (dy >= -5 && dy <= 5) { inBand = true; break; }
                    }
                    if (!inBand) Carve(chunks, lx, wy, lz);
                }
            }
        }
    }

    private void MaskVertex(IServerChunk[] chunks, int bx, int bz, int totalY)
    {
        for (int lx = 0; lx < ChunkSize; lx++)
        {
            int wx = bx + lx;
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int wz = bz + lz;
                for (int wy = 1; wy < totalY; wy++)
                {
                    int h = (wx * 73856093) ^ (wz * 19349663) ^ (wy * 83492791);
                    if ((uint)(h * 2654435769u) % 20 < 13) Carve(chunks, lx, wy, lz);
                }
            }
        }
    }

    /// <summary>Farther: diagonal stretched stripes, perpendicular to axis.</summary>
    private void MaskFarther(IServerChunk[] chunks, int bx, int bz, int totalY, bool axisIsX)
    {
        for (int lx = 0; lx < ChunkSize; lx++)
        {
            int wx = bx + lx;
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int wz = bz + lz;
                double v = axisIsX
                    ? Math.Sin(wx * 0.05 + wz * 0.4)
                    : Math.Sin(wz * 0.05 + wx * 0.4);
                if (v > 0.3) continue;
                for (int wy = 1; wy < totalY; wy++) Carve(chunks, lx, wy, lz);
            }
        }
    }

    /// <summary>Vanilla terrain lifted and repeated along the perpendicular axis. Snapshots one
    /// column of vanilla and duplicates it: the world looks raised and the pattern glitches forever.</summary>
    private void MaskTunnel(IServerChunk[] chunks, int totalY, bool axisIsX)
    {
        int lift = _tunnelLift;
        var col = new int[totalY];

        for (int outer = 0; outer < ChunkSize; outer++)
        {
            int snapLx = axisIsX ? 0 : outer;
            int snapLz = axisIsX ? outer : 0;
            for (int wy = 0; wy < totalY; wy++)
            {
                int cy = wy / ChunkSize;
                int idx = (wy % ChunkSize * ChunkSize + snapLz) * ChunkSize + snapLx;
                col[wy] = chunks[cy].Data[idx];
            }
            for (int inner = 0; inner < ChunkSize; inner++)
            {
                int dstLx = axisIsX ? inner : outer;
                int dstLz = axisIsX ? outer : inner;
                for (int wy = 0; wy < totalY; wy++)
                {
                    int srcY = wy - lift;
                    int blk = (srcY >= 0 && srcY < totalY) ? col[srcY] : _air;
                    Replace(chunks, dstLx, wy, dstLz, blk);
                }
            }
        }
    }

    private void MaskComb(IServerChunk[] chunks, int bx, int bz, int totalY)
    {
        for (int lx = 0; lx < ChunkSize; lx++)
        {
            int wx = bx + lx;
            bool xGap = ModN(wx, 4) == 0;
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int wz = bz + lz;
                bool zGap = ModN(wz, 4) == 0;
                if (xGap || zGap)
                    for (int wy = 1; wy < totalY; wy++) Carve(chunks, lx, wy, lz);
            }
        }
    }

    /// <summary>Pole: guillotine — clear half the chunk perpendicular to axis (closer half to border).</summary>
    private void MaskPole(IServerChunk[] chunks, int bx, int bz, int totalY, bool axisIsX)
    {
        // Carve every other 32-block band along the axis
        if (axisIsX)
        {
            for (int lx = 0; lx < ChunkSize; lx++)
            {
                int wx = bx + lx;
                if (((wx / 32) & 1) == 0) continue; // keep alternating bands
                for (int lz = 0; lz < ChunkSize; lz++)
                for (int wy = 1; wy < totalY; wy++)
                    Carve(chunks, lx, wy, lz);
            }
        }
        else
        {
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int wz = bz + lz;
                if (((wz / 32) & 1) == 0) continue;
                for (int lx = 0; lx < ChunkSize; lx++)
                for (int wy = 1; wy < totalY; wy++)
                    Carve(chunks, lx, wy, lz);
            }
        }
    }

    private void MaskStrip(IServerChunk[] chunks, int bx, int bz, int totalY)
    {
        for (int lx = 0; lx < ChunkSize; lx++)
        {
            int wx = bx + lx;
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int wz = bz + lz;
                bool keep = ModN(wx, 16) == 0 || ModN(wz, 16) == 0;
                if (keep) continue;
                for (int wy = 1; wy < totalY; wy++) Carve(chunks, lx, wy, lz);
            }
        }
    }

    private void MaskStripe(IServerChunk[] chunks, int bx, int bz, int totalY)
    {
        for (int lx = 0; lx < ChunkSize; lx++)
        {
            int wx = bx + lx;
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int wz = bz + lz;
                for (int wy = 1; wy < totalY; wy++)
                {
                    if (((wx + wy + wz) & 1) == 1) Carve(chunks, lx, wy, lz);
                }
            }
        }
    }

    private void MaskNothingness(IServerChunk[] chunks, int totalY)
    {
        for (int lx = 0; lx < ChunkSize; lx++)
        for (int lz = 0; lz < ChunkSize; lz++)
        for (int wy = 1; wy < totalY; wy++)
            Carve(chunks, lx, wy, lz);
    }

    private void MaskSkygrid(IServerChunk[] chunks, int bx, int bz, int totalY)
    {
        for (int lx = 0; lx < ChunkSize; lx++)
        {
            int wx = bx + lx;
            bool xOn = ModN(wx, 4) == 0;
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int wz = bz + lz;
                bool zOn = ModN(wz, 4) == 0;
                for (int wy = 1; wy < totalY; wy++)
                {
                    bool yOn = wy % 8 == 0;
                    if (xOn && zOn && yOn) ReplaceSolid(chunks, lx, wy, lz, _grass);
                    else Carve(chunks, lx, wy, lz);
                }
            }
        }
    }

    private void MaskNetherGrid(IServerChunk[] chunks, int bx, int bz, int totalY)
    {
        for (int lx = 0; lx < ChunkSize; lx++)
        {
            int wx = bx + lx;
            bool xOn = ModN(wx, 8) == 0;
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int wz = bz + lz;
                bool zOn = ModN(wz, 8) == 0;
                for (int wy = 1; wy < totalY; wy++)
                {
                    bool yOn = wy % 4 == 0;
                    if (xOn && zOn && yOn) ReplaceSolid(chunks, lx, wy, lz, _basalt);
                    else Carve(chunks, lx, wy, lz);
                }
            }
        }
    }

    private void MaskEndIsland(IServerChunk[] chunks, int bx, int bz, int totalY)
    {
        for (int lx = 0; lx < ChunkSize; lx++)
        {
            int wx = bx + lx;
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int wz = bz + lz;
                int ix = FloorDiv(wx, 24);
                int iz = FloorDiv(wz, 24);
                int dx = wx - ix * 24 - 12;
                int dz = wz - iz * 24 - 12;
                bool inDisc = dx * dx + dz * dz < 60;
                for (int wy = 1; wy < totalY; wy++)
                {
                    if (inDisc) ReplaceSolid(chunks, lx, wy, lz, _chalk);
                    else Carve(chunks, lx, wy, lz);
                }
            }
        }
    }

    private void MaskSixtyFourBit(IServerChunk[] chunks, int bx, int bz, int totalY)
    {
        for (int lx = 0; lx < ChunkSize; lx++)
        {
            int wx = bx + lx;
            bool xOnGrid = ModN(wx, 8) == 0;
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int wz = bz + lz;
                double cosWz = Math.Cos(wz * 1.3);
                double sinXZ = Math.Sin((wx + wz) * 0.13);
                bool xzOnGrid = xOnGrid && ModN(wz, 8) == 0;
                for (int wy = 1; wy < totalY; wy++)
                {
                    double f1 = Math.Sin(wx * 1.7 + wy * 0.3) * cosWz;
                    double f2 = sinXZ * Math.Cos(wy * 0.5);
                    bool keep = (f1 + f2) > 0.2 || (xzOnGrid && wy < 180);
                    if (!keep) Carve(chunks, lx, wy, lz);
                }
            }
        }
    }

    // ====================== helpers ======================

    private void Carve(IServerChunk[] chunks, int lx, int wy, int lz)
    {
        int chunkY = wy / ChunkSize;
        if (chunkY < 0 || chunkY >= chunks.Length) return;
        int idx = (wy % ChunkSize * ChunkSize + lz) * ChunkSize + lx;
        if (chunks[chunkY].Data[idx] == _air) return;
        chunks[chunkY].Data[idx] = _air;
    }

    private void ReplaceSolid(IServerChunk[] chunks, int lx, int wy, int lz, int newId)
    {
        int chunkY = wy / ChunkSize;
        if (chunkY < 0 || chunkY >= chunks.Length) return;
        int idx = (wy % ChunkSize * ChunkSize + lz) * ChunkSize + lx;
        int cur = chunks[chunkY].Data[idx];
        if (cur == _air) return;
        chunks[chunkY].Data[idx] = newId;
    }

    /// <summary>Unconditional overwrite (skips the air guard). Required when the caller must be
    /// able to copy explicit air (e.g. lifting vanilla columns).</summary>
    private static void Replace(IServerChunk[] chunks, int lx, int wy, int lz, int newId)
    {
        int chunkY = wy / ChunkSize;
        if (chunkY < 0 || chunkY >= chunks.Length) return;
        int idx = (wy % ChunkSize * ChunkSize + lz) * ChunkSize + lx;
        chunks[chunkY].Data[idx] = newId;
    }

    /// <summary>Remove any remaining water blocks. Masks like Corner/Comb/Pole/Strip/Stripe/64bit
    /// leave non-carved cells intact, so vanilla water would survive without this pass.
    /// Tunnel is excluded (its water is part of the lifted pattern).</summary>
    private void PostCleanWater(IServerChunk[] chunks)
    {
        int sectionLen = ChunkSize * ChunkSize * ChunkSize;
        for (int cy = 0; cy < chunks.Length; cy++)
        {
            var data = chunks[cy].Data;
            for (int i = 0; i < sectionLen; i++)
                if (data[i] == _water) data[i] = _air;
        }
    }

    private static int ModN(int a, int m) => ((a % m) + m) % m;

    private static int FloorDiv(int a, int b)
    {
        int q = a / b;
        if ((a ^ b) < 0 && q * b != a) q--;
        return q;
    }

    private static int ReadIntEnv(string name, int fallback)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        return int.TryParse(raw, out var v) ? v : fallback;
    }
}
