using RimWorld;
using Verse;

namespace TKKN_NPS;

[DefOf]
public static class TerrainDefOf
{
    public static TerrainDef TKKN_SaltField;
    public static TerrainDef TKKN_Salted_Earth;
    public static TerrainDef TKKN_HotSpringsWater;
    public static TerrainDef TKKN_ColdSpringsWater;
    public static TerrainDef TKKN_Lava;
    public static TerrainDef TKKN_LavaDeep;
    public static TerrainDef TKKN_LavaRock_RoughHewn;
    public static TerrainDef TKKN_DirtPath;
    public static TerrainDef TKKN_SandPath;

    //FOR WEATHER:

    //wet
    public static TerrainDef TKKN_SoilWet;
    public static TerrainDef TKKN_SoilWetRich;
    public static TerrainDef TKKN_SandWet;

    //cold
    public static TerrainDef TKKN_Ice;

    //tides
    public static TerrainDef TKKN_SandBeachWetSalt;

    //flooding
    public static TerrainDef TKKN_RiverDeposit;
    
    static TerrainDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(TerrainDefOf));
    }
}