using HarmonyLib;
using Verse;
using Verse.AI;

namespace TKKN_NPS;

[HarmonyPatch(typeof(Reachability), nameof(Reachability.CanReach), typeof(IntVec3), typeof(LocalTargetInfo),
    typeof(PathEndMode),
    typeof(TraverseParms))]
internal class Reachability_CanReach
{
    public static void Postfix(LocalTargetInfo dest, TraverseParms traverseParams, ref bool __result)
    {
        if (!__result)
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