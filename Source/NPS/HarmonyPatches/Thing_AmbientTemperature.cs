using HarmonyLib;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(Thing), nameof(Thing.AmbientTemperature), MethodType.Getter)]
internal class Thing_AmbientTemperature
{
    public static void Postfix(Thing __instance, ref float __result)
    {
        var temperature = __result;

        var c = __instance.Position;
        var map = __instance.Map;


        //check if we should have temperature affected by contact with terrain
        if (map != null && c.InBounds(map))
        {
            var terrain = c.GetTerrain(map);
            var weatherExtension = terrain.GetModExtension<TerrainWeatherReactions>();
            if (weatherExtension != null &&
                weatherExtension.temperatureAdjust != 0)
            {
                temperature += weatherExtension.temperatureAdjust;
            }
        }

        __result = temperature;
    }
}