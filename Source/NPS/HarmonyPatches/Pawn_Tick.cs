using HarmonyLib;
using RimWorld;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(Pawn), "Tick")]
internal class Pawn_Tick
{
    public static void Postfix(Pawn __instance)
    {
        if (__instance is not ({ Spawned: true } and { Dead: false }))
        {
            return;
        }

        var terrain = __instance.Position.GetTerrain(__instance.MapHeld);
        if (terrain == null)
            return;
        MakePaths(__instance);
        MakeBreath(__instance);
        MakeWet(__instance, terrain);
        DyingCheck(__instance, terrain);

        if (__instance.RaceProps.Humanlike && __instance.needs == null)
        {
            return;
        }

        
        if (terrain == TerrainDefOf.TKKN_HotSpringsWater)
        {
            if (__instance.needs.comfort != null)
            {
                __instance.needs.comfort.lastComfortUseTick--;
            }

            HediffDef hediffDef = HediffDefOf.TKKN_hotspring_chill_out;
            if (__instance.health.hediffSet.GetFirstHediffOfDef(hediffDef) == null)
            {
                var hediff = HediffMaker.MakeHediff(hediffDef, __instance);
                __instance.health.AddHediff(hediff);
            }
        }
        else if (terrain == TerrainDefOf.TKKN_ColdSpringsWater) {
            __instance.needs.rest?.TickResting(.05f);
            HediffDef hediffDef = HediffDefOf.TKKN_coldspring_chill_out;
            if (__instance.health.hediffSet.GetFirstHediffOfDef(hediffDef) != null) {
                return;
            }

            var hediff = HediffMaker.MakeHediff(hediffDef, __instance);
            __instance.health.AddHediff(hediff);
        }
    }

    private static void DyingCheck(Pawn pawn, TerrainDef terrain)
    {
        //drowning == immobile and in water
        if (!pawn.RaceProps.Humanlike)
        {
            return;
        }

        if (pawn.health.Downed && terrain.HasTag("TKKN_Wet"))
        {
            var damage = .0005f;
            //if they're awake, take less damage
            if (!pawn.health.capacities.CanBeAwake)
            {
                if (terrain.HasTag("TKKN_Swim"))
                {
                    damage = .0001f;
                }
                else
                {
                    return;
                }
            }

            //heavier clothing hurts them more
            var apparel = pawn.apparel.WornApparel;
            var weight = 0f;
            foreach (var apparel1 in apparel)
            {
                weight += (float)apparel1.HitPoints / 10000;
            }

            damage += weight / 5000;
            HealthUtility.AdjustSeverity(pawn, HediffDefOf.TKKN_Drowning, damage);

            var hediffDef = HediffDefOf.TKKN_Drowning;
            if (pawn.Faction is not { IsPlayer: true } ||
                pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) != null)
            {
                return;
            }

            string text = "TKKN_NPS_DrowningText".Translate();
            Messages.Message(text, MessageTypeDefOf.NeutralEvent);
            return;
        }

        var drowning = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.TKKN_Drowning);
        if (drowning != null)
        {
            pawn.health.RemoveHediff(drowning);
        }
    }

    private static void MakeWet(Pawn pawn, TerrainDef currentTerrain)
    {
        if (!Settings.allowPawnsToGetWet)
        {
            return;
        }

        var hediffDef = HediffDefOf.TKKN_Wetness;
        if (!pawn.RaceProps.Humanlike || pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) != null)
        {
            return;
        }

        var map = pawn.MapHeld;
        var c = pawn.Position;
        if (map == null || !c.IsValid)
        {
            return;
        }

        var isWet = false;
        if (map.weatherManager.curWeather.rainRate > .001f)
        {
            var roofed = map.roofGrid.Roofed(c);
            if (!roofed)
            {
                isWet = true;
            }
        }
        else
        {
            if (currentTerrain.HasTag("TKKN_Wet"))
            {
                isWet = true;
            }
        }

        if (!isWet)
        {
            return;
        }

        if (HarmonyMain.RimBrellasActive && (bool)HarmonyMain.HasUmbrella.Invoke(pawn, [pawn]))
        {
            return;
        }

        var hediff = HediffMaker.MakeHediff(hediffDef, pawn);
        hediff.Severity = 0;
        pawn.health.AddHediff(hediff);
    }

    

    private static Map cachedMap;
    private static Watcher watcher;
    private static void MakePaths(Pawn pawn)
    {
        if (cachedMap != pawn.Map)
        {
            cachedMap = pawn.Map;
            watcher = cachedMap.GetComponent<Watcher>();
        }

        if (watcher == null)
        {
            return;
        }

        if (!pawn.RaceProps.Humanlike || !pawn.Position.InBounds(cachedMap))
        {
            return;
        }

        //damage plants and remove snow/frost where they are. This will hopefully generate paths as pawns walk :)
        if (watcher.checkIfCold(pawn.Position))
        {
            watcher.frostGridComponent.AddDepth(pawn.Position, (float)-.05);
            cachedMap.snowGrid.AddDepth(pawn.Position, (float)-.05);
        }

        if (Settings.doDirtPath) {
            //pack down the soil only if the pawn is moving AND is in our colony
            if (pawn.pather.MovingNow && pawn.IsColonist &&
                watcher.cellWeatherAffects.TryGetValue(pawn.Position, out var cell)) {
                cell.doPack();
            }
        }

        /*
        if (Settings.allowPlantEffects)
        {
            //this will be handled by the terrain changing in doPack
            //		watcher.hurtPlants(pawn.Position, true, true);
        }*/
    }

    private static void MakeBreath(Pawn pawn)
    {
        if (Find.TickManager.TicksGame % 150 != 0)
        {
            return;
        }

        var isCold = watcher.checkIfCold(pawn.Position);
        if (!isCold)
        {
            return;
        }

        var head = pawn.Position;
        head.z += 1;
        if (!head.ShouldSpawnMotesAt(cachedMap) || cachedMap.moteCounter.SaturatedLowPriority)
        {
            return;
        }

        var moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.TKKN_Mote_ColdBreath);
        moteThrown.airTimeLeft = 99999f;
        moteThrown.Scale = Rand.Range(.5f, 1.5f);
        moteThrown.rotationRate = Rand.Range(-30f, 30f);
        moteThrown.exactPosition = head.ToVector3();
        moteThrown.SetVelocity(Rand.Range(20, 30), Rand.Range(0.5f, 0.7f));
        GenSpawn.Spawn(moteThrown, head, cachedMap);
    }
}