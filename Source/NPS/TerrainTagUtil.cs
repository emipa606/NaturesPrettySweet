using System.Collections.Generic;
using Verse;

namespace TKKN_NPS;

public static class TerrainTagUtil
{
    public static readonly HashSet<TerrainDef> TKKN_Wet = [];
    public static readonly HashSet<TerrainDef> TKKN_Swim = [];
    public static readonly HashSet<TerrainDef> Lava = [];
    public static readonly HashSet<TerrainDef> TKKN_SwimOrLava = [];

    public static void intializeTerrainTags()
    {
        var allTerrains = DefDatabase<TerrainDef>.AllDefsListForReading;

        foreach (var terrain in allTerrains)
        {
            if (terrain.HasTag("TKKN_Wet"))
            {
                TKKN_Wet.Add(terrain);
            }

            if (terrain.HasTag("TKKN_Swim"))
            {
                TKKN_Swim.Add(terrain);
                TKKN_SwimOrLava.Add(terrain);
            }

            if (!terrain.HasTag("Lava"))
            {
                continue;
            }

            Lava.Add(terrain);
            TKKN_SwimOrLava.Add(terrain);
        }
    }
}