using System.Collections.Generic;
using Verse;

namespace TKKN_NPS;

internal class ThingWeatherReaction : DefModExtension
{
    public List<TerrainDef> allowedTerrains;
    public string droughtGraphicPath;
    public string floweringGraphicPath;
    public Graphic frostGraphicData;
    public string frostGraphicPath;
    public string frostLeaflessGraphicPath;
    public string snowGraphicPath;
    public string snowLeaflessGraphicPath;
}