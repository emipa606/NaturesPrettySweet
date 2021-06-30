using HarmonyLib;
using Verse;
using Verse.AI;

namespace TKKN_NPS
{
    [HarmonyPatch(typeof(Reachability), "CanReach", typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode),
        typeof(TraverseParms))]
    internal class PatchCanReach
    {
        [HarmonyPostfix]
        public static void Postfix(IntVec3 start, LocalTargetInfo dest, PathEndMode peMode,
            TraverseParms traverseParams, bool __result)
        {
            if (__result == false)
            {
                return;
            }

            if (traverseParams.pawn == null)
            {
                return;
            }

            if (!traverseParams.pawn.RaceProps.Animal)
            {
                return;
            }

            var c = dest.Cell;
            if (c.GetTerrain(traverseParams.pawn.Map).HasTag("TKKN_Swim") ||
                c.GetTerrain(traverseParams.pawn.Map).HasTag("TKKN_Lava"))
            {
                __result = false;
            }
        }
    }
}