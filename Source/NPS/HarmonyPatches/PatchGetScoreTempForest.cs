using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace TKKN_NPS
{
    [HarmonyPatch(typeof(BiomeWorker_TemperateForest))]
    [HarmonyPatch("GetScore")]
    public static class PatchGetScoreTempForest
    {
        [HarmonyPostfix]
        public static void Postfix(Tile tile, float __result)
        {
            if (__result == -100f || __result == 0f)
            {
                return;
            }

            if (tile.rainfall < 1200f)
            {
                __result = 0f;
                return;
            }

            __result = (float) (15.0 + (tile.temperature - 7.0) + ((tile.rainfall - 600.0) / 180.0));
        }
    }
}