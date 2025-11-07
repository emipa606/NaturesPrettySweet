using System;
using HarmonyLib;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(CellFinder), nameof(CellFinder.TryRandomClosewalkCellNear))]
public static class CellFinder_TryRandomClosewalkCellNear
{
    public static bool Prefix(IntVec3 root, Map map, int radius, out IntVec3 result,
        Predicate<IntVec3> extraValidator, ref bool __result)
    {
        // don't enter on deep water or Lava
        __result = CellFinder.TryFindRandomReachableCellNearPosition(root, root, map, radius,
            TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblocked(true),
            c => c.Standable(map) && !TerrainTagUtil.TKKN_Swim.Contains(c.GetTerrain(map)) &&
                 !TerrainTagUtil.Lava.Contains(c.GetTerrain(map)) &&
                 (extraValidator == null || extraValidator(c)), null, out result);
        //			Log.Warning("result " + result.ToString());
        return false;
    }
}