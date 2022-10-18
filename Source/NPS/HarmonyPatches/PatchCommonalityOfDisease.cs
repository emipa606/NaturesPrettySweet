using HarmonyLib;
using RimWorld;

namespace TKKN_NPS;

[HarmonyPatch(typeof(BiomeDef))]
[HarmonyPatch("CommonalityOfDisease")]
public static class PatchCommonalityOfDisease
{
    [HarmonyPrefix]
    public static void Prefix(BiomeDef __instance)
    {
        var biomeSettings = __instance.GetModExtension<BiomeSeasonalSettings>();
        if (biomeSettings == null || biomeSettings.diseaseCacheUpdated)
        {
            return;
        }

        // Log.Warning("updating cachedDiseaseCommonalities");
        Traverse.Create(__instance).Field("cachedDiseaseCommonalities").SetValue(null);
        biomeSettings.diseaseCacheUpdated = true;
    }
}