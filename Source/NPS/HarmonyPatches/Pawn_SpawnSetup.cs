using HarmonyLib;
using RimWorld;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
internal class Pawn_SpawnSetup
{
    public static void Postfix(Pawn __instance)
    {
        if (__instance is not { Spawned: true } || !__instance.RaceProps.Humanlike)
        {
            return;
        }

        var terrain = __instance.Position.GetTerrain(__instance.Map);
        if (terrain == null || !terrain.HasTag("Lava"))
        {
            return;
        }

        if (Prefs.DevMode)
        {
            Log.Message($"{__instance} spawns in lava, setting on fire");
        }

        __instance.TryAttachFire(10f, __instance);
    }
}