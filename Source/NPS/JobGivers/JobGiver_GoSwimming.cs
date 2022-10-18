using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TKKN_NPS;

public class JobGiver_GoSwimming : JobGiver_Wander
{
    private static readonly List<IntVec3> swimmingSpots = new List<IntVec3>();

    public JobGiver_GoSwimming()
    {
        locomotionUrgency = LocomotionUrgency.Walk;
        maxDanger = Danger.None;
        ticksBetweenWandersRange = new IntRange(20, 100);
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!JoyUtility.EnjoyableOutsideNow(pawn))
        {
            return null;
        }

        var nextMoveOrderIsWait = pawn.mindState.nextMoveOrderIsWait;
        pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
        if (nextMoveOrderIsWait)
        {
            return new Job(RimWorld.JobDefOf.Wait_Wander)
            {
                expiryInterval = ticksBetweenWandersRange.RandomInRange
            };
        }


        var c = getSwimmingCell(pawn);
        if (!c.IsValid)
        {
            pawn.mindState.nextMoveOrderIsWait = false;
            return null;
        }

        var job = new Job(JobDefOf.TKKN_GoSwimming, c);
        pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, c);
        job.locomotionUrgency = locomotionUrgency;
        return job;
    }

    private IntVec3 getSwimmingCell(Pawn pawn)
    {
        var wanderRoot = GetWanderRoot(pawn);
        var c = RCellFinder.RandomWanderDestFor(pawn, wanderRoot, wanderRadius, wanderDestValidator,
            PawnUtility.ResolveMaxDanger(pawn, maxDanger));
        for (var i = 0; i < 20; i++)
        {
            var c2 = c + GenAdj.AdjacentCellsAndInside[i];
            if (!c2.InBounds(pawn.Map))
            {
                return IntVec3.Invalid;
            }

            if (!c2.Standable(pawn.Map))
            {
                return IntVec3.Invalid;
            }

            if (!c2.IsValid)
            {
                continue;
            }

            var terrain = c2.GetTerrain(pawn.Map);
            if (terrain.HasTag("TKKN_Swim"))
            {
                return c2;
            }
        }

        return IntVec3.Invalid;
    }

    protected override IntVec3 GetWanderRoot(Pawn pawn)
    {
        if (!pawn.RaceProps.Humanlike)
        {
            return IntVec3.Invalid;
        }

        var watcher = pawn.Map.GetComponent<Watcher>();
        swimmingSpots.Clear();

        foreach (var position in watcher.swimmingCellsList)
        {
            if (!position.IsForbidden(pawn) && pawn.CanReach(position, PathEndMode.Touch, Danger.None))
            {
                swimmingSpots.Add(position);
            }
        }

        return swimmingSpots.Count > 0 ? swimmingSpots.RandomElement() : IntVec3.Invalid;
    }
}