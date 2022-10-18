using System;
using HarmonyLib;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(CellFinder))]
[HarmonyPatch("TryRandomClosewalkCellNear")]
public static class PatchTryRandomClosewalkCellNear
{
    [HarmonyPrefix]
    public static bool Prefix(IntVec3 root, Map map, int radius, out IntVec3 result,
        Predicate<IntVec3> extraValidator, ref bool __result)
    {
        // don't enter on deep water or Lava
        __result = CellFinder.TryFindRandomReachableCellNear(root, map, radius,
            TraverseParms.For(TraverseMode.NoPassClosedDoors),
            c => c.Standable(map) && !c.GetTerrain(map).HasTag("TKKN_Lava") &&
                 !c.GetTerrain(map).HasTag("TKKN_Swim") && (extraValidator == null || extraValidator(c)), null,
            out result);
        //			Log.Warning("result " + result.ToString());
        return false;
    }
}