using HarmonyLib;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("SpawnSetup")]
internal class PatchSpawnSetupPawn
{
    [HarmonyPostfix]
    public static void Postfix(Pawn __instance)
    {
        if (__instance is not { Spawned: true } || !__instance.RaceProps.Humanlike)
        {
        }
    }
}