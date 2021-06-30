using Verse;

namespace TKKN_NPS
{
    public class PlaceWorker_OnSteamVent : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
            Thing thingToIgnore = null, Thing thing = null)
        {
//			Thing thing = map.thingGrid.ThingAt(loc, TKKN_NPS.ThingDefOf.TKKN_SteamVent);
            if (thing == null || thing.Position != loc)
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
}