using RimWorld;

namespace TKKN_NPS;

[DefOf]
public class BiomeDefOf
{
    public static BiomeDef TKKN_VolcanicFlow;
    public static BiomeDef TKKN_Oasis;
    
    static BiomeDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(BiomeDefOf));
    }
}