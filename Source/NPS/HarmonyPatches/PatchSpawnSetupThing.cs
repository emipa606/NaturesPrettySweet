using HarmonyLib;
using RimWorld;
using Verse;

namespace TKKN_NPS
{
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("SpawnSetup")]
    internal class PatchSpawnSetupThing
    {
        //if it spawns in lava, destroy it
        [HarmonyPostfix]
        public static void Postfix(Thing __instance)
        {
            var c = __instance.Position;
            var map = __instance.Map;
            var terrain = c.GetTerrain(map);
            if (terrain == null || !terrain.HasTag("Lava"))
            {
                return;
            }

            FireUtility.TryStartFireIn(c, map, 5f);

            var statValue = __instance.GetStatValue(StatDefOf.Flammability);
            var alt = __instance.def.altitudeLayer == AltitudeLayer.Item;
            if (statValue != 0f || !alt)
            {
                return;
            }

            if (!__instance.Destroyed && __instance.def.destroyable)
            {
                __instance.Destroy();
            }
        }
    }
}