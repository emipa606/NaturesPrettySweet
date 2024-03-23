using UnityEngine;
using Verse;

namespace TKKN_NPS;

[StaticConstructorOnStartup]
public static class MatBases
{
    public static readonly Texture frostTexture = ContentFinder<Texture2D>.Get("TKKN_NPS/Temperature/Frost");

    public static Material Frost
    {
        get
        {
            var frost = new Material(Verse.MatBases.Snow) { mainTexture = frostTexture };
            return frost;
        }
    }
}