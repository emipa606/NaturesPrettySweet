using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace TKKN_NPS;

[HarmonyPatch(typeof(BiomeDef), nameof(BiomeDef.CommonalityOfDisease))]
public static class BiomeDef_CommonalityOfDisease
{
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