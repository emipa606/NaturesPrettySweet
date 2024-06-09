using HarmonyLib;
using RimWorld;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Tick))]
internal class Pawn_Tick
{
    public static void Postfix(Pawn __instance)
    {
        if (__instance is not { Spawned: true })
        {
            return;
        }

        var terrain = __instance.Position.GetTerrain(__instance.MapHeld);
        if (terrain.defName is "TKKN_SaltField" or "TKKN_Salted_Earth" &&
            __instance.def.defName == "TKKN_giantsnail")
        {
            BurnSnails(__instance);
            return;
        }


        if (!__instance.Dead)
        {
            if (!Find.TickManager.Paused)
            {
                MakePaths(__instance);
                MakeBreath(__instance);
                MakeWet(__instance);
                DyingCheck(__instance, terrain);
            }
        }

        if (!__instance.Spawned || __instance.Dead ||
            __instance.RaceProps.Humanlike && __instance.needs == null)
        {
            return;
        }

        HediffDef hediffDef;
        if (terrain.defName == "TKKN_HotSpringsWater")
        {
            if (__instance.needs.comfort != null)
            {
                __instance.needs.comfort.lastComfortUseTick--;
            }

            hediffDef = HediffDefOf.TKKN_hotspring_chill_out;
            if (__instance.health.hediffSet.GetFirstHediffOfDef(hediffDef) == null)
            {
                var hediff = HediffMaker.MakeHediff(hediffDef, __instance);
                __instance.health.AddHediff(hediff);
            }
        }

        if (terrain.defName != "TKKN_ColdSpringsWater")
        {
            return;
        }

        {
            __instance.needs.rest.TickResting(.05f);
            hediffDef = HediffDefOf.TKKN_coldspring_chill_out;
            if (__instance.health.hediffSet.GetFirstHediffOfDef(hediffDef) != null)
            {
                return;
            }

            var hediff = HediffMaker.MakeHediff(hediffDef, __instance);
            __instance.health.AddHediff(hediff);
        }
    }

    public static void DyingCheck(Pawn pawn, TerrainDef terrain)
    {
        //drowning == immobile and in water
        if (pawn == null || terrain == null)
        {
            return;
        }

        if (pawn.RaceProps.Humanlike && pawn.health.Downed && terrain.HasTag("TKKN_Wet"))
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
            HealthUtility.AdjustSeverity(pawn, HediffDef.Named("TKKN_Drowning"), damage);

            var hediffDef = HediffDefOf.TKKN_Drowning;
            if (!pawn.Faction.IsPlayer || pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) != null ||
                !pawn.RaceProps.Humanlike)
            {
                return;
            }

            string text = "TKKN_NPS_DrowningText".Translate();
            Messages.Message(text, MessageTypeDefOf.NeutralEvent);
            return;
        }

        if (pawn.RaceProps.Humanlike &&
            pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.TKKN_Drowning) != null)
        {
            pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.TKKN_Drowning));
        }
    }

    public static void MakeWet(Pawn pawn)
    {
        if (!Settings.allowPawnsToGetWet)
        {
            return;
        }

        var hediffDef = HediffDefOf.TKKN_Wetness;
        if (pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) != null || !pawn.RaceProps.Humanlike)
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
            var room = c.GetRoom(map);
            var roofed = map.roofGrid.Roofed(c);
            _ = room is { UsesOutdoorTemperature: true };
            if (!roofed)
            {
                isWet = true;
            }
        }
        else
        {
            var currentTerrain = c.GetTerrain(map);
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

    public static void BurnSnails(Pawn pawn)
    {
        var battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire);
        Find.BattleLog.Add(battleLogEntry_DamageTaken);
        var dinfo = new DamageInfo(DamageDefOf.Flame, 100, -1f, 0, null, null, null,
            DamageInfo.SourceCategory.ThingOrUnknown, pawn);
        dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
        pawn.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);
    }

    public static void MakePaths(Pawn pawn)
    {
        var map = pawn.Map;
        var watcher = map.GetComponent<Watcher>();
        if (watcher == null)
        {
            return;
        }

        if (!pawn.Position.InBounds(map) || !pawn.RaceProps.Humanlike)
        {
            return;
        }

        //damage plants and remove snow/frost where they are. This will hopefully generate paths as pawns walk :)
        if (watcher.checkIfCold(pawn.Position))
        {
            map.GetComponent<FrostGrid>().AddDepth(pawn.Position, (float)-.05);
            map.snowGrid.AddDepth(pawn.Position, (float)-.05);
        }

        //pack down the soil only if the pawn is moving AND is in our colony
        if (pawn.pather.MovingNow && pawn.IsColonist &&
            watcher.cellWeatherAffects.TryGetValue(pawn.Position, out var cell))
        {
            cell.doPack();
        }

        if (Settings.allowPlantEffects)
        {
            //this will be handled by the terrain changing in doPack
            //		watcher.hurtPlants(pawn.Position, true, true);
        }
    }

    public static void MakeBreath(Pawn pawn)
    {
        if (Find.TickManager.TicksGame % 150 != 0)
        {
            return;
        }

        var map = pawn.Map;
        var watcher = map.GetComponent<Watcher>();

        var isCold = watcher.checkIfCold(pawn.Position);
        if (!isCold)
        {
            return;
        }

        var head = pawn.Position;
        head.z += 1;
        if (!head.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
        {
            return;
        }

        var moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("TKKN_Mote_ColdBreath"));
        moteThrown.airTimeLeft = 99999f;
        moteThrown.Scale = Rand.Range(.5f, 1.5f);
        moteThrown.rotationRate = Rand.Range(-30f, 30f);
        moteThrown.exactPosition = head.ToVector3();
        moteThrown.SetVelocity(Rand.Range(20, 30), Rand.Range(0.5f, 0.7f));
        GenSpawn.Spawn(moteThrown, head, map);
    }
}