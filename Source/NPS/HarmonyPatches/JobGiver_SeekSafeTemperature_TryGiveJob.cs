using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TKKN_NPS;

//pawns will go sit in cold springs to cool off if there is no better option
[HarmonyPatch(typeof(JobGiver_SeekSafeTemperature), "TryGiveJob")]
internal class JobGiver_SeekSafeTemperature_TryGiveJob
{
    public static void Postfix(ref Job __result, Pawn pawn)
    {
        if (__result != null || pawn?.RaceProps?.CanPassFences == false)
        {
            return;
        }

        if (Find.CurrentMap.GetComponent<Watcher>().activeSprings.Count != 0)
        {
            __result = null;
            return;
        }

        var isHot = false;
        if (pawn == null)
        {
            return;
        }

        foreach (var hediff in pawn.health.hediffSet.hediffs)
        {
            if (hediff.def != RimWorld.HediffDefOf.Heatstroke ||
                hediff.CurStageIndex < (int)TemperatureInjuryStage.Serious)
            {
                continue;
            }

            isHot = true;
            break;
        }

        if (!isHot)
        {
            __result = null;
            return;
        }

        var terrain = pawn.Position.GetTerrain(Find.CurrentMap);
        if (terrain.defName == "TKKN_ColdSpringsWater")
        {
            __result = new Job(RimWorld.JobDefOf.Wait_SafeTemperature, 500, true);
            return;
        }

        //send them to the closest spring to relax

        var thing = GenClosest.ClosestThingReachable(pawn.GetLord().CurLordToil.FlagLoc, pawn.Map,
            ThingRequest.ForDef(ThingDefOf.TKKN_ColdSpring), PathEndMode.Touch, TraverseParms.For(pawn), -1f);
        if (thing != null)
        {
            __result = new Job(RimWorld.JobDefOf.GotoSafeTemperature, thing.Position);
        }
    }
}