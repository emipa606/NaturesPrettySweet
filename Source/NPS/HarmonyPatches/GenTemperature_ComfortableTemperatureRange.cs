using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(GenTemperature), nameof(GenTemperature.ComfortableTemperatureRange), typeof(Pawn))]
internal class GenTemperature_ComfortableTemperatureRange
{
    public static void Postfix(Pawn p, ref FloatRange __result)
    {
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
        __result.min = Math.Max(12, __result.min + setTo);
        __result.max = Math.Min(32, __result.max + setTo);

        //	Log.Warning(p.Name.ToString() + " temp old: " + old.ToString() + " temp range: " + __result.ToString() + " temp: " + p.AmbientTemperature);
    }

    private static int getOffSet(Hediff_Wetness wetness, Pawn pawn)
    {
        //soaked
        var setTo = wetness.CurStageIndex switch {
            0 => 0, //dry
            1 => 5, //damp
            2 => 10, //soggy
            3 => 20, //wet
            _ => 40 //soaked
        };

        if (pawn.InBed())
        {
            setTo -= 10;
        }

        //to stop hypothermia when it's hot outside
        var ambientTemp = pawn.AmbientTemperature;
        if (ambientTemp > 0)
        {
            setTo -= (int)Math.Floor(ambientTemp / 3);
        }

        return setTo;
    }
}