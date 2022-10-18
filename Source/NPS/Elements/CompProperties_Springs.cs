using Verse;

namespace TKKN_NPS;

// COMP PROPERTIES
public abstract class CompProperties_Springs : CompProperties
{
    public readonly int radius = 6;
    public readonly float temperature = 1f;
    public int AOE = 15;
    public int borderSize;
    public bool canReproduce;
    public string commonBiome;
    public TerrainDef deepTile;
    public TerrainDef dryTile;
    public int howOftenToChange;
    public ThingDef spawnProp;
    public int startingRadius;
    public int weight;
    public TerrainDef wetTile;
}

//THING COMPS