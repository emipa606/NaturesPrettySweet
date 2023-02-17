using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace TKKN_NPS;

[HarmonyPatch(typeof(BiomeDef))]
[HarmonyPatch("CommonalityOfDisease")]
public static class PatchCommonalityOfDisease
{
    [HarmonyPrefix]
    public static void Prefix(BiomeDef __instance, ref Dictionary<IncidentDef, float> ___cachedDiseaseCommonalities)
    {
        var biomeSettings = __instance.GetModExtension<BiomeSeasonalSettings>();
        if (biomeSettings == null || biomeSettings.diseaseCacheUpdated)
        {
            return;
        }

        ___cachedDiseaseCommonalities = null;
        biomeSettings.diseaseCacheUpdated = true;
    }
}