using System;
using HarmonyLib;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
public static class BackCompatibility_BackCompatibleDefName
{
    public static void Postfix(Type defType, string defName, ref string __result)
    {
        if (defType != typeof(TerrainDef) || defName.NullOrEmpty() || __result.NullOrEmpty())
        {
            return;
        }

        if (!defName.Contains("_HalfBlend_") || DefDatabase<TerrainDef>.GetNamedSilentFail(__result) != null)
        {
            return;
        }

        var split = defName.Split(["_HalfBlend_"], StringSplitOptions.None);
        if (split.Length != 2)
        {
            return;
        }

        if (TryUseTerrain(split[0], ref __result))
        {
            return;
        }

        var right = split[1];
        right = right
            .Replace("NorthEast_", string.Empty)
            .Replace("NorthWest_", string.Empty)
            .Replace("SouthEast_", string.Empty)
            .Replace("SouthWest_", string.Empty);

        if (TryUseTerrain(right, ref __result))
        {
            return;
        }

        while (right.Contains("_"))
        {
            right = right[..right.LastIndexOf('_')];
            if (TryUseTerrain(right, ref __result))
            {
                return;
            }
        }
    }

    private static bool TryUseTerrain(string candidate, ref string result)
    {
        if (candidate.NullOrEmpty())
        {
            return false;
        }

        if (DefDatabase<TerrainDef>.GetNamedSilentFail(candidate) == null)
        {
            return false;
        }

        result = candidate;
        return true;
    }
}