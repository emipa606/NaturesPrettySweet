using HarmonyLib;
using Verse;

namespace TKKN_NPS;

internal class PatchGraphic
{
    [HarmonyPatch(typeof(Graphic_Shadow))]
    [HarmonyPatch("DrawWorker")]
    public static class PatchDrawWorker
    {
        [HarmonyPrefix]
        public static bool Prefix(Thing thing)
        {
            if (thing is not Pawn pawn)
            {
                return true;
            }

            if (!pawn.RaceProps.Humanlike || !pawn.Position.IsValid)
            {
                return true;
            }

            var terrain = pawn.Position.GetTerrain(pawn.MapHeld);
            return terrain == null || !terrain.HasTag("TKKN_Swim");
        }
    }
}