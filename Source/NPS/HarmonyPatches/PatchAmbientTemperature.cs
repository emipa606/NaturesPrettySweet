using HarmonyLib;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(Thing))]
[HarmonyPatch("AmbientTemperature", MethodType.Getter)]
internal class PatchAmbientTemperature
{
    [HarmonyPostfix]
    public static void Postfix(Thing __instance, ref float __result)
    {
        var temperature = __result;

        var c = __instance.Position;
        var map = __instance.Map;


        //check if we should have temperature affected by contact with terrain
        if (map != null && c.InBounds(map))
        {
            var terrain = c.GetTerrain(map);
            if (terrain.HasModExtension<TerrainWeatherReactions>() &&
                terrain.GetModExtension<TerrainWeatherReactions>().temperatureAdjust != 0)
            {
                temperature += terrain.GetModExtension<TerrainWeatherReactions>().temperatureAdjust;
            }
        }

        __result = temperature;
    }
}