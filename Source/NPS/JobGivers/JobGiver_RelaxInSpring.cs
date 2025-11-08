using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TKKN_NPS;

public class JobGiver_RelaxInSpring : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!JoyUtility.EnjoyableOutsideNow(pawn))
        {
            return null;
        }

        if (!pawn.RaceProps.Humanlike)
        {
            return null;
        }

        var hotSpring = GenClosest.ClosestThingReachable(pawn.GetLord().CurLordToil.FlagLoc, pawn.Map,
            ThingRequest.ForDef(ThingDefOf.TKKN_HotSpring), PathEndMode.Touch, TraverseParms.For(pawn), -1f,
            validator);
        if (hotSpring == null)
        {
            return null;
        }

        var spring = GenClosest.ClosestThingReachable(pawn.GetLord().CurLordToil.FlagLoc, pawn.Map,
            ThingRequest.ForDef(ThingDefOf.TKKN_ColdSpring), PathEndMode.Touch, TraverseParms.For(pawn), -1f,
            validator);
        return spring != null ? new Job(RimWorld.JobDefOf.GotoSafeTemperature, getSpringCell(spring)) : null;

        static bool validator(Thing t)
        {
            if (t.def == ThingDefOf.TKKN_HotSpring && t.AmbientTemperature is < 26 and > 15)
            {
                return true;
            }

            return t.def == ThingDefOf.TKKN_ColdSpring && t.AmbientTemperature > 24;
        }
    }

    private static IntVec3 getSpringCell(Thing spring)
    {
        spring.MapHeld.regionAndRoomUpdater.Enabled = true;
        CellFinder.TryFindRandomCellNear(spring.Position, spring.Map, 6, validator, out var c);
        spring.MapHeld.regionAndRoomUpdater.Enabled = false;
        return c;

        bool validator(IntVec3 pos)
        {
            if (spring.def == ThingDefOf.TKKN_HotSpring)
            {
                return pos.GetTerrain(spring.Map) == TerrainDefOf.TKKN_HotSpringsWater;
            }

            if (spring.def == ThingDefOf.TKKN_ColdSpring)
            {
                return pos.GetTerrain(spring.Map) == TerrainDefOf.TKKN_ColdSpringsWater;
            }

            return false;
        }
    }
}