using Verse;

namespace TKKN_NPS;

public class TerrainWeatherReactions : DefModExtension
{
    public TerrainDef baseOverride; //twmp fix for issue where wet soils weren't turning back to dry
    public TerrainDef dryTerrain; //perm fix for wet soils getting bugged
    public TerrainDef floodTerrain;
    public int freezeAt;
    public TerrainDef freezeTerrain;
    public bool holdFrost;
    public bool isSalty;
    public float temperatureAdjust;
    public TerrainDef tideTerrain;
    public int wetAt;
    public TerrainDef wetTerrain;
}