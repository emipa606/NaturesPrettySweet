using HarmonyLib;
using RimWorld;
using Verse;

namespace TKKN_NPS;

//load the extra plant graphics
[HarmonyPatch(typeof(PlantProperties), "PostLoadSpecial")]
public static class PostLoadSpecialPatch
{
    [HarmonyPostfix]
    public static void Postfix(Plant __instance, ThingDef parentDef)
    {
        if (!__instance.def.HasModExtension<ThingWeatherReaction>())
        {
            return;
        }

        var mod = __instance.def.GetModExtension<ThingWeatherReaction>();

        if (!mod.frostGraphicPath.NullOrEmpty())
        {
            var id = $"{__instance.def.defName}frost";
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                __instance.Map.GetComponent<Watcher>().graphicHolder.Add(id,
                    GraphicDatabase.Get(__instance.def.graphicData.graphicClass, mod.frostGraphicPath,
                        __instance.def.graphic.Shader, __instance.def.graphicData.drawSize,
                        __instance.def.graphicData.color, __instance.def.graphicData.colorTwo));
            });
        }

        if (!mod.droughtGraphicPath.NullOrEmpty())
        {
            var id = $"{__instance.def.defName}drought";
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                __instance.Map.GetComponent<Watcher>().graphicHolder.Add(id,
                    GraphicDatabase.Get(__instance.def.graphicData.graphicClass, mod.droughtGraphicPath,
                        __instance.def.graphic.Shader, __instance.def.graphicData.drawSize,
                        __instance.def.graphicData.color, __instance.def.graphicData.colorTwo));
            });
        }

        if (mod.floweringGraphicPath.NullOrEmpty())
        {
            return;
        }

        {
            var id = $"{__instance.def.defName}flowering";
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                __instance.Map.GetComponent<Watcher>().graphicHolder.Add(id,
                    GraphicDatabase.Get(__instance.def.graphicData.graphicClass, mod.floweringGraphicPath,
                        __instance.def.graphic.Shader, __instance.def.graphicData.drawSize,
                        __instance.def.graphicData.color, __instance.def.graphicData.colorTwo));
            });
        }
    }
}