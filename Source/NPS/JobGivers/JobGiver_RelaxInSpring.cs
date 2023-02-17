using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TKKN_NPS;

public class JobGiver_RelaxInSpring : ThinkNode_JobGiver
{
    private float radius = 30f;

    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!JoyUtility.EnjoyableOutsideNow(pawn))
        {
            return null;
        }

        bool Validator(Thing t)
        {
            if (t.def.defName == "TKKN_HotSpring" && t.AmbientTemperature is < 26 and > 15)
            {
                return true;
            }

            return t.def.defName == "TKKN_ColdSpring" && t.AmbientTemperature > 24;
        }

        var hotSpring = GenClosest.ClosestThingReachable(pawn.GetLord().CurLordToil.FlagLoc, pawn.Map,
            ThingRequest.ForDef(ThingDefOf.TKKN_HotSpring), PathEndMode.Touch, TraverseParms.For(pawn), -1f,
            Validator);
        if (hotSpring == null)
        {
            return null;
        }

        var spring = GenClosest.ClosestThingReachable(pawn.GetLord().CurLordToil.FlagLoc, pawn.Map,
            ThingRequest.ForDef(ThingDefOf.TKKN_ColdSpring), PathEndMode.Touch, TraverseParms.For(pawn), -1f,
            Validator);
        return spring != null ? new Job(RimWorld.JobDefOf.GotoSafeTemperature, getSpringCell(spring)) : null;
    }

    private IntVec3 getSpringCell(Thing spring)
    {
        bool Validator(IntVec3 pos)
        {
            switch (spring.def.defName)
            {
                case "TKKN_HotSpring":
                    return pos.GetTerrain(spring.Map).defName == "TKKN_HotSpringsWater";
                case "TKKN_ColdSpring":
                    return pos.GetTerrain(spring.Map).defName == "TKKN_ColdSpringsWater";
                default:
                    return false;
            }
        }

        spring.MapHeld.regionAndRoomUpdater.Enabled = true;
        CellFinder.TryFindRandomCellNear(spring.Position, spring.Map, 6, Validator, out var c);
        spring.MapHeld.regionAndRoomUpdater.Enabled = false;
        return c;
    }
}