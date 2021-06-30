using System.Collections.Generic;
using Verse;

namespace TKKN_NPS
{
    public class ElementSpawnDef : Def
    {
        public List<string> allowedBiomes;
        public bool allowOnWater;
        public List<string> forbiddenBiomes;
        public List<string> terrainValidationAllowed;
        public List<string> terrainValidationDisallowed;
        public ThingDef thingDef;
    }
}