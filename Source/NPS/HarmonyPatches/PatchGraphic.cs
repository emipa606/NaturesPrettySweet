using HarmonyLib;
using UnityEngine;
using Verse;

namespace TKKN_NPS
{
    internal class PatchGraphic
    {
        [HarmonyPatch(typeof(Graphic_Shadow))]
        [HarmonyPatch("DrawWorker")]
        public static class PatchDrawWorker
        {
            [HarmonyPrefix]
            public static bool Prefix(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
            {
                var pawn = thing as Pawn;
                if (pawn is not Pawn)
                {
                    return true;
                }

                if (!pawn.RaceProps.Humanlike || !pawn.Position.IsValid)
                {
                    return true;
                }

                var terrain = pawn.Position.GetTerrain(pawn.MapHeld);
                if (terrain != null && terrain.HasTag("TKKN_Swim"))
                {
                    return false;
                }

                return true;
            }
        }
    }
}