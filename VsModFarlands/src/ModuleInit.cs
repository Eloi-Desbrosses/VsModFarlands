using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace VsModFarlands;

/// <summary>
/// Runs at CLR assembly-load time. Two complementary translation-injection
/// strategies, both best-effort:
///
/// 1. Eager-inject the embedded en.json into every locale's live entryCache.
/// 2. Harmony-patch <c>TranslationService.GetUnformatted</c> so any later
///    lookup for one of our keys returns our literal even if (1) missed.
///
/// Neither runs on the VS Client before the Customize-World screen renders
/// (the client reads <c>[assembly: ModInfo]</c> via metadata-only reflection
/// and doesn't JIT-load this assembly until the embedded server starts), so
/// raw translation keys are still visible at first-launch Customize. Both
/// hooks do fire on the server and on the client after entering any world.
/// </summary>
internal static class ModuleInit
{
    private static Dictionary<string, string>? _entries;

    [ModuleInitializer]
    internal static void Run()
    {
        try
        {
            _entries = LoadEmbeddedTranslations();
            if (_entries == null) return;
            EagerInject();
            ApplyHarmonyPatch();
        }
        catch
        {
            // Best-effort. If injection fails, raw keys still render.
        }
    }

    private static Dictionary<string, string>? LoadEmbeddedTranslations()
    {
        var asm = typeof(ModuleInit).Assembly;
        var resourceName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("en.json"));
        if (resourceName == null) return null;

        using var stream = asm.GetManifestResourceStream(resourceName);
        if (stream == null) return null;
        using var reader = new StreamReader(stream);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd());
    }

    private static void EagerInject()
    {
        if (_entries == null || Lang.AvailableLanguages == null) return;

        foreach (var ts in Lang.AvailableLanguages.Values)
        {
            if (ts == null) continue;
            try
            {
                var live = ts.GetAllEntries();
                if (live == null) continue;
                foreach (var kvp in _entries) live[kvp.Key] = kvp.Value;
            }
            catch { /* per-locale failure shouldn't abort the loop */ }
        }
    }

    private static void ApplyHarmonyPatch()
    {
        var target = AccessTools.Method(typeof(TranslationService), nameof(TranslationService.GetUnformatted), new[] { typeof(string) });
        if (target == null) return;

        var postfix = AccessTools.Method(typeof(ModuleInit), nameof(GetUnformattedPostfix));
        new Harmony("vsmodfarlands.translations").Patch(target, postfix: new HarmonyMethod(postfix));
    }

    public static void GetUnformattedPostfix(string key, ref string __result)
    {
        if (_entries == null) return;
        if (!ReferenceEquals(__result, key)) return; // Already translated — leave it.

        if (_entries.TryGetValue("game:" + key, out var prefixed))
        {
            __result = prefixed;
            return;
        }
        if (_entries.TryGetValue(key, out var bare))
        {
            __result = bare;
        }
    }
}
