using System.Collections.Generic;
using Verse;

namespace TKKN_NPS;

public static class TerrainTagUtil
{
    public static HashSet<TerrainDef> TKKN_Wet=[];
    public static HashSet<TerrainDef> TKKN_Swim=[];
    public static HashSet<TerrainDef> Lava = [];

    public static void intializeTerrainTags() {
        var allTerrains=DefDatabase<TerrainDef>.AllDefsListForReading;

        foreach (var terrain in allTerrains) {
            if (terrain.HasTag("TKKN_Wet")) {
                TKKN_Wet.Add(terrain);
            }

            if (terrain.HasTag("TKKN_Swim")) {
                TKKN_Swim.Add(terrain);
            }

            if (terrain.HasTag("Lava")) {
                Lava.Add(terrain);
            }
        }
    }
    
}