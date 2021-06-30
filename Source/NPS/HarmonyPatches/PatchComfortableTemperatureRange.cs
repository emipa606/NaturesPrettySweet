using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace TKKN_NPS
{
    [HarmonyPatch(typeof(GenTemperature), "ComfortableTemperatureRange", typeof(Pawn))]
    internal class PatchComfortableTemperatureRange
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref FloatRange __result)
        {
            if (Find.TickManager.Paused)
            {
                return;
            }

            if (!p.RaceProps.Humanlike)
            {
                return;
            }

            var hediffDef = HediffDefOf.TKKN_Wetness;
            if (p.health.hediffSet.GetFirstHediffOfDef(hediffDef) is not Hediff_Wetness wetness)
            {
                return;
            }

            var setTo = getOffSet(wetness, p);
            if (setTo <= 0)
            {
                return;
            }

            //they are comfortable only at higher temp
            var old = __result;
            __result.min += setTo;
            __result.max += setTo;

            if (__result.min < 12)
            {
                __result.min = 12;
            }

            if (__result.max < 32)
            {
                __result.max = 32;
            }

            //	Log.Warning(p.Name.ToString() + " temp old: " + old.ToString() + " temp range: " + __result.ToString() + " temp: " + p.AmbientTemperature);
        }

        public static int getOffSet(Hediff_Wetness wetness, Pawn pawn)
        {
            //soaked
            var setTo = 40;
            if (wetness.CurStage.label == "dry")
            {
                setTo = 0;
            }

            if (wetness.CurStage.label == "damp")
            {
                //damp
                setTo = 5;
            }

            if (wetness.CurStage.label == "soggy")
            {
                //soggy
                setTo = 10;
            }

            if (wetness.CurStage.label == "wet")
            {
                //wet
                setTo = 20;
            }

            if (pawn.InBed())
            {
                setTo -= 10;
            }

            //to stop hypothermia when it's hot outside
            if (pawn.AmbientTemperature > 0)
            {
                setTo -= (int) Math.Floor(pawn.AmbientTemperature / 3);
            }

            return setTo;
        }
    }
}