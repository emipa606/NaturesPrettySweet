using RimWorld;
using Verse;

namespace TKKN_NPS;

public class IncidentWorker_Dustdevil : IncidentWorker
{
    private const int MinDistanceFromMapEdge = 30;

    private const float MinWind = 1f;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return false;
/*
            var map = (Map) parms.target;
            return map.weatherManager.CurWindSpeedFactor >= 1f;
*/
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map)parms.target;
        var cellRect = CellRect.WholeMap(map).ContractedBy(30);
        if (cellRect.IsEmpty)
        {
            cellRect = CellRect.WholeMap(map);
        }

        var devils = new IntRange(1, 5);
        var randomInRange = devils.RandomInRange;
        for (var i = 0; i < randomInRange; i++)
        {
            if (!CellFinder.TryFindRandomCellInsideWith(cellRect, x => CanSpawnDustDevilAt(x, map), out var loc))
            {
                return false;
            }

            var t = (DustDevil)GenSpawn.Spawn(ThingDefOf.TKKN_DustDevil, loc, map);

            SendStandardLetter(parms, t);
        }

        return true;
    }

    private bool CanSpawnDustDevilAt(IntVec3 c, Map map)
    {
        if (c.Fogged(map))
        {
            return false;
        }

        var num = GenRadial.NumCellsInRadius(7f);
        for (var i = 0; i < num; i++)
        {
            var c2 = c + GenRadial.RadialPattern[i];
            if (!c2.InBounds(map))
            {
                continue;
            }

            if (AnyPawnOfPlayerFactionAt(c2, map))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AnyPawnOfPlayerFactionAt(IntVec3 c, Map map)
    {
        var thingList = c.GetThingList(map);
        foreach (var thing in thingList)
        {
            if (thing is Pawn pawn && pawn.Faction == Faction.OfPlayer)
            {
                return true;
            }
        }

        return false;
    }
}