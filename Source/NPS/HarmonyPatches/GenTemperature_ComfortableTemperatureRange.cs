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
        _ = __result;
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

    private static int getOffSet(Hediff_Wetness wetness, Pawn pawn)
    {
        //soaked
        var setTo = 40;
        switch (wetness.CurStageIndex)
        {
            case 0:
                setTo = 0;
                break;
            case 1:
                //damp
                setTo = 5;
                break;
            case 2:
                //soggy
                setTo = 10;
                break;
            case 3:
                //wet
                setTo = 20;
                break;
        }

        if (pawn.InBed())
        {
            setTo -= 10;
        }

        //to stop hypothermia when it's hot outside
        if (pawn.AmbientTemperature > 0)
        {
            setTo -= (int)Math.Floor(pawn.AmbientTemperature / 3);
        }

        return setTo;
    }
}