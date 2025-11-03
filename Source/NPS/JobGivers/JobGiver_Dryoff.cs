using Verse;
using Verse.AI;

// NOTE: The job that puts pawns in springs when they're too hot is in harmonypatches/jobgiverspringspatch.cs


namespace TKKN_NPS;

public class JobGiver_Dryoff : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!pawn.RaceProps.Humanlike)
        {
            return null;
        }

        var hediffDef = HediffDefOf.TKKN_Wetness;

        if (pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) is Hediff_Wetness wetness &&
            wetness.CurStageIndex == 4)
        {
            return null;
        }

        var c = getDryCell(pawn);

        var job = new Job(JobDefOf.TKKN_DryOff, c);
        pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, c);
        return job;
    }

    private static IntVec3 getDryCell(Pawn pawn)
    {
        pawn.MapHeld.regionAndRoomUpdater.Enabled = true;
        CellFinder.TryFindRandomCellNear(pawn.Position, pawn.MapHeld, 6, Validator, out var c);
        pawn.MapHeld.regionAndRoomUpdater.Enabled = false;
        return c;

        bool Validator(IntVec3 pos)
        {
            if (pos.GetTerrain(pawn.MapHeld).HasTag("TKKN_Wet"))
            {
                return false;
            }

            if (!(pawn.MapHeld.weatherManager.RainRate > 0) && !(pawn.MapHeld.weatherManager.SnowRate > 0))
            {
                return true;
            }

            return pawn.MapHeld.roofGrid.Roofed(pos);
        }
    }
}