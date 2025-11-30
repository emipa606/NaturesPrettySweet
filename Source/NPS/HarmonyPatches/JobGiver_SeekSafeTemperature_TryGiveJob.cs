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
    public static void Postfix(ref Job __result, Pawn pawn) {
        if (__result != null || pawn?.RaceProps?.CanPassFences == false) {
            return;
        }

        if (pawn == null) {
            return;
        }

        if (Find.CurrentMap.GetComponent<Watcher>()?.activeSprings?.Count == 0) {
            return;
        }

        var heatstroke = pawn.health.hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.Heatstroke);

        if (heatstroke == null) {
            return;
        }

        //Setting this to lower stages will make the pawn repeatedly enter and leave hotspring
        if (heatstroke.CurStageIndex != (int)TemperatureInjuryStage.Serious) {
            return;
        }

        var terrain = pawn.Position.GetTerrain(Find.CurrentMap);
        if (terrain == TerrainDefOf.TKKN_ColdSpringsWater) {
            __result = new Job(RimWorld.JobDefOf.Wait_SafeTemperature, 500, true);
            return;
        }

        IntVec3 pawnLocation = pawn.GetLord()?.CurLordToil?.FlagLoc ?? pawn.Position;
        //send them to the closest spring to relax
        var thing = GenClosest.ClosestThingReachable(pawnLocation, pawn.Map,
            ThingRequest.ForDef(ThingDefOf.TKKN_ColdSpring), PathEndMode.Touch, TraverseParms.For(pawn), 50f,
            ignoreEntirelyForbiddenRegions: true);
        if (thing != null) {
            __result = new Job(RimWorld.JobDefOf.GotoSafeTemperature, thing.Position);
        }
    }
}