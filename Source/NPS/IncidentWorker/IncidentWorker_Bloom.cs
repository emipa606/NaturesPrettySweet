using System.Linq;
using RimWorld;
using Verse;

namespace TKKN_NPS;

public class IncidentWorker_Bloom : IncidentWorker_TKKN_Weather
{
    private bool relevantSetting = Settings.allowPlantEffects;

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!settingsCheck())
        {
            return false;
        }

        var map = (Map)parms.target;
        var unused = CellFinder.RandomNotEdgeCell(15, map);

        //can the biome support it?
        var canBloomHere = true;
        var biomeSettings = map.Biome.GetModExtension<BiomeSeasonalSettings>();
        if (biomeSettings == null)
        {
            return false;
        }

        var bloomPlants = biomeSettings.bloomPlants.ToList();
        if (bloomPlants.Count == 0)
        {
            return false;
        }


        Find.LetterStack.ReceiveLetter(def.letterLabel.Translate(), def.letterText.Translate(), def.letterDef);


        return true;
    }
}