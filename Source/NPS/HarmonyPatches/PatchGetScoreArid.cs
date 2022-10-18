using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace TKKN_NPS;

[HarmonyPatch(typeof(BiomeWorker_AridShrubland))]
[HarmonyPatch("GetScore")]
public static class PatchGetScoreArid
{
    [HarmonyPostfix]
    public static void Postfix(Tile tile, ref float __result)
    {
        if (__result is -100f or 0f)
        {
            return;
        }

        if (tile.rainfall >= 1200f || tile.temperature < 23f)
        {
            __result = 0f;
        }
        else
        {
            __result = (float)(22.5 + ((tile.temperature - 20.0) * 2.2000000476837158) +
                               ((tile.rainfall - 600.0) / 100.0));
        }
    }
}