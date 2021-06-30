using RimWorld;
using Verse;

namespace TKKN_NPS
{
    public class IncidentWorker_Drought : IncidentWorker_TKKN_Weather
    {
        public IncidentWorker_Drought()
        {
            label = "TKKN_NPS_DroughtLbl".Translate();
            text = "TKKN_NPS_DroughtTxt".Translate();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!settingsCheck())
            {
                return false;
            }

            var map = (Map) parms.target;

            if (map.weatherManager.RainRate > 0 || map.weatherManager.SnowRate > 0)
            {
                return false;
            }

            Find.LetterStack.ReceiveLetter(def.letterLabel.Translate(), def.letterText.Translate(), def.letterDef);


            return true;
        }
    }
}