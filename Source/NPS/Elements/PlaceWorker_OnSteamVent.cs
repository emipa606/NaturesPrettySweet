using Verse;

namespace TKKN_NPS;

public class PlaceWorker_OnSteamVent : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        var possibleSteamVent = map.thingGrid.ThingAt(loc, ThingDefOf.TKKN_SteamVent);
        if (possibleSteamVent == null || possibleSteamVent.Position != loc)
        {
            return "TKKN_NPS_MustPlaceOnSteamVent".Translate();
        }

        return true;
    }

    public override bool ForceAllowPlaceOver(BuildableDef otherDef)
    {
        return otherDef == ThingDefOf.TKKN_SteamVent;
    }
}