using Vintagestory.API.Common;

[assembly: ModInfo(
    "Far Lands",
    "vsmodfarlands",
    Version = "0.2.0",
    Description = "Replicates the 14 iconic Minecraft Far Lands biomes as a worldgen ring around the map border.",
    Authors = new[] { "Spinnn" },
    Side = "Universal",
    RequiredOnClient = false,
    // VS Client renders raw translation keys for raw-DLL mods (no client-side
    // hook fires before Customize-World renders). Code and names are written
    // as the labels we want to read, with spaces and % included — the
    // hardcoded "worldattribute-" / "worldconfig-<code>-" prefix is still
    // shown, but everything after reads as natural English.
    WorldConfig = """
    {
      "playstyles": [],
      "worldConfigAttributes": [
        {
          "category": "worldgen",
          "code": "Far Lands Coverage",
          "dataType": "dropdown",
          "values": ["0", "20", "40", "60", "80", "100"],
          "names": ["0%", "20%", "40%", "60%", "80%", "100%"],
          "default": "20",
          "onlyDuringWorldCreate": true
        }
      ]
    }
    """
)]
