﻿using RimWorld;
using Verse;

namespace TKKN_NPS;

public class Hediff_Wetness : HediffWithComps
{
    private int timeDrying;
    private float wetnessLevel;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref wetnessLevel, "wetnessLevel");
        Scribe_Values.Look(ref timeDrying, "timeDrying");
    }

    public override void Tick()
    {
        base.Tick();

        if (!pawn.Position.IsValid || pawn.MapHeld == null ||
            !pawn.Position.InBounds(pawn.MapHeld))
        {
            return;
        }

        var wetness = wetnessRate();
        if (wetness > 0)
        {
            Severity += wetness / 1000;
            wetnessLevel += wetness;
            if (wetnessLevel < 0)
            {
                wetnessLevel = 0;
            }

            if (!(Severity > .62) || ageTicks % 1000 != 0)
            {
                return;
            }

            FilthMaker.TryMakeFilth(pawn.Position, pawn.MapHeld, ThingDef.Named("TKKN_FilthPuddle"));
            Severity -= .3f;
        }
        else
        {
            Severity += wetness / 1000;
        }
    }

    private float wetnessRate()
    {
        var rate = 0f;
        //check if the pawn is in water
        var terrain = pawn.Position.GetTerrain(pawn.MapHeld);
        if (terrain != null && terrain.HasTag("TKKN_Wet"))
        {
            //deep water gets them soaked.
            if (terrain.HasTag("TKKN_Swim"))
            {
                if (Severity < .65f)
                {
                    Severity = .65f;
                }

                rate = .3f;
                return rate;
            }

            rate = .05f;
        }


        //check if the pawn is wet from the weather
        var roofed = pawn.MapHeld.roofGrid.Roofed(pawn.Position);
        if (!roofed && pawn.MapHeld.weatherManager.curWeather.rainRate > .001f)
        {
            rate = pawn.MapHeld.weatherManager.curWeather.rainRate / 10;
        }

        if (!roofed && pawn.MapHeld.weatherManager.curWeather.snowRate > .001f)
        {
            rate = pawn.MapHeld.weatherManager.curWeather.snowRate / 100;
        }

        if (rate == 0f)
        {
            timeDrying++;
        }
        else
        {
            timeDrying = 0;
            return rate;
        }

        //dry the pawn.
        if (pawn.AmbientTemperature > 0)
        {
            rate -= pawn.AmbientTemperature / 200;
        }

        //check if the pawn is near a heat source
        foreach (var c in GenAdj.CellsAdjacentCardinal(pawn))
        {
            if (!c.InBounds(pawn.MapHeld) || !c.IsValid)
            {
                continue;
            }

            var things = c.GetThingList(pawn.MapHeld);
            foreach (var thing in things)
            {
                var heater = thing.TryGetComp<CompHeatPusher>();
                if (heater != null)
                {
                    rate -= heater.Props.heatPerSecond / 5000;
                }
            }
        }

        rate -= (float)timeDrying / 250;
//			Log.Warning(rate.ToString());
        return rate;
    }
}