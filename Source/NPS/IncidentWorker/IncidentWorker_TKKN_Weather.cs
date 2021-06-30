using RimWorld;
using Verse;

namespace TKKN_NPS
{
    public class IncidentWorker_TKKN_Weather : IncidentWorker
    {
        private readonly bool relevantSetting = Settings.doWeather;
        public string label;
        public string text;
        public ThingDef thingDef;

        public bool settingsCheck()
        {
            if (!relevantSetting)
            {
                return false;
            }

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            return settingsCheck();
        }
    }
}