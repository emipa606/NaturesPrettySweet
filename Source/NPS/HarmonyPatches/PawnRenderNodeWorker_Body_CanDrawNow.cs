using HarmonyLib;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(PawnRenderNodeWorker_Body), nameof(PawnRenderNodeWorker_Body.CanDrawNow))]
internal class PawnRenderNodeWorker_Body_CanDrawNow
{
    public static void Postfix(PawnDrawParms parms, ref bool __result)
    {
        if (!__result)
        {
            return;
        }

        if (!Settings.allowPawnsSwim)
        {
            return;
        }

        var pawn = parms.pawn;
        if (pawn is not { Position.IsValid: true } || pawn.Dead)
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
            __result = false;
        }
    }
}