using RimWorld;
using Verse;

namespace TKKN_NPS;

public class IncidentWorker_LavaFlow : IncidentWorker
{
    private const float FogClearRadius = 4.5f;
    public ThingDef thingDef;

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var map = (Map)parms.target;
        var intVec = CellFinder.RandomNotEdgeCell(15, map);

        if (!Settings.allowLavaEruption)
        {
            return false;
        }

        if (Settings.spawnLavaOnlyInBiome && map.Biome != BiomeDefOf.TKKN_VolcanicFlow)
        {
            return false;
        }

        _ = (ThingWithComps)GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TKKN_Lava_Spring), intVec, map);


        string label = "TKKN_NPS_LavaHasEruptedNearby".Translate();
        string text = "TKKN_NPS_LavaHasEruptedNearbyTxt".Translate();

        Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, new TargetInfo(intVec, map));


        return true;
    }
}