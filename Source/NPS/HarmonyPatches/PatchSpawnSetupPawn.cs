using HarmonyLib;
using Verse;

namespace TKKN_NPS
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("SpawnSetup")]
    internal class PatchSpawnSetupPawn
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance)
        {
            if (__instance == null || !__instance.Spawned || !__instance.RaceProps.Humanlike)
            {
            }
        }
    }
}