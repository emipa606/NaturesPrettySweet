using HarmonyLib;
using RimWorld;
using Verse;

namespace TKKN_NPS;

//swap out plant graphics based on seasonal effects
[HarmonyPatch(typeof(Plant))]
[HarmonyPatch("Graphic", MethodType.Getter)]
public static class PatchGraphicPlant
{
    [HarmonyPostfix]
    public static void Postfix(Plant __instance, ref Graphic __result)
    {
        var id = __instance.def.defName;

        if (!__instance.def.HasModExtension<ThingWeatherReaction>())
        {
            return;
        }

        var mod = __instance.def.GetModExtension<ThingWeatherReaction>();
        var map = __instance.Map;

        var path = "";

        //get flowering or drought graphic if it's over 70
        if (__instance.AmbientTemperature > 21)
        {
            var watcher = map.GetComponent<Watcher>();
            if (watcher.cellWeatherAffects[__instance.Position] != null)
            {
                var cell = watcher.cellWeatherAffects[__instance.Position];
                var location = Find.WorldGrid.LongLatOf(__instance.MapHeld.Tile);
                var season = GenDate.Season(Find.TickManager.TicksAbs, location);

                if (!string.IsNullOrEmpty(mod.floweringGraphicPath) &&
                    (cell.howWetPlants > 60 && map.weatherManager.RainRate <= .001f || season == Season.Spring))
                {
                    id += "flowering";
                    path = mod.floweringGraphicPath;
                }

                if (!string.IsNullOrEmpty(mod.droughtGraphicPath) && cell.howWetPlants < 20)
                {
                    id += "drought";
                    path = mod.droughtGraphicPath;
                }
                else if (__instance.def.plant.leaflessGraphic != null && cell.howWetPlants < 20)
                {
                    id += "drought";
                    path = __instance.def.plant.leaflessGraphic.path;
                }
            }
        }

        if (path != "")
        {
            if (!map.GetComponent<Watcher>().graphicHolder.ContainsKey(id))
            {
                //only load the image once.
                map.GetComponent<Watcher>().graphicHolder.Add(id,
                    GraphicDatabase.Get(__instance.def.graphicData.graphicClass, path,
                        __instance.def.graphic.Shader, __instance.def.graphicData.drawSize,
                        __instance.def.graphicData.color, __instance.def.graphicData.colorTwo));
            }

            if (map.GetComponent<Watcher>().graphicHolder.TryGetValue(id, out var value))
            {
                __result = value;
            }

            return;
        }

        if (Settings.showCold)
        {
            //get snow graphic
            if (map.snowGrid.GetDepth(__instance.Position) >= 0.5f)
            {
                if (!string.IsNullOrEmpty(mod.snowGraphicPath))
                {
                    id += "snow";
                    path = mod.snowGraphicPath;
                }
            }
            else if (map.GetComponent<FrostGrid>().GetDepth(__instance.Position) >= 0.6f)
            {
                if (!string.IsNullOrEmpty(mod.frostGraphicPath))
                {
                    id += "frost";
                    path = mod.frostGraphicPath;
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            //if it's leafless
            if (__instance.def.plant.leaflessGraphic == __result)
            {
                id += "leafless";
                path = path.Replace("Frosted", "Frosted/Leafless");
                path = path.Replace("Snow", "Snow/Leafless");
                path += "_Leafless";
            }
            else if (__instance.def.blockWind)
            {
                //make it so snow doesn't fall under the tree until it's leafless.
                //	map.snowGrid.AddDepth(__instance.Position, -.05f);
            }
        }


        if (!map.GetComponent<Watcher>().graphicHolder.ContainsKey(id))
        {
            //only load the image once.
            map.GetComponent<Watcher>().graphicHolder.Add(id,
                GraphicDatabase.Get(__instance.def.graphicData.graphicClass, path, __instance.def.graphic.Shader,
                    __instance.def.graphicData.drawSize, __instance.def.graphicData.color,
                    __instance.def.graphicData.colorTwo));
        }

        if (map.GetComponent<Watcher>().graphicHolder.TryGetValue(id, out var value1))
        {
            __result = value1;
        }
    }
}