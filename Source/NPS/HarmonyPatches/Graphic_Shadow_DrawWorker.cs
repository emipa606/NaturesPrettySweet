using HarmonyLib;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(Graphic_Shadow), nameof(Graphic_Shadow.DrawWorker))]
public static class Graphic_Shadow_DrawWorker
{
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
        return !TerrainTagUtil.TKKN_Swim.Contains(terrain);
    }
}