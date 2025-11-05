using System.Collections.Generic;
using Verse;

namespace TKKN_NPS;

public class CompProperties_DeadlySalt :  CompProperties
{
    public List<TerrainDef> deadlyTerrain = [];
    
    public CompProperties_DeadlySalt()
    {
        compClass = typeof(CompDeadlySalt);
    }
}