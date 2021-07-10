using HarmonyLib;
using Verse;

namespace TKKN_NPS
{
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
    internal class PatchRenderPawnInternal
    {
        [HarmonyPrefix]
        public static void Prefix(ref bool renderBody, ref float angle, PawnRenderer __instance)
        {
            if (!Settings.allowPawnsSwim)
            {
                return;
            }

            var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn == null || !pawn.Position.IsValid || pawn.Dead)
            {
                return;
            }

            if (!pawn.RaceProps.Humanlike || pawn.MapHeld == null)
            {
                return;
            }

            var terrain = pawn.Position.GetTerrain(pawn.MapHeld);
            if (terrain == null)
            {
                return;
            }

            if (terrain.HasTag("TKKN_Swim"))
            {
                renderBody = false;
            }
        }
    }
}