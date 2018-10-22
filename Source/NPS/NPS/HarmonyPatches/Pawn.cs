﻿using Verse;
using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;

namespace TKKN_NPS
{
	
	[HarmonyPatch(typeof(Pawn))]
	[HarmonyPatch("SpawnSetup")]
	class PatchSpawnSetupPawn
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn __instance)
		{
			if (__instance == null || !__instance.Spawned || !__instance.RaceProps.Humanlike)
			{
				return;
			}
			
		}
	}
	[HarmonyPatch(typeof(Pawn))]
	[HarmonyPatch("Tick")]
	class PatchTickPawn
	{
		[HarmonyPostfix]
		public static void Postfix(Pawn __instance)
		{
			if (__instance == null || !__instance.Spawned)
			{
				return;
			}

			TerrainDef terrain = __instance.Position.GetTerrain(__instance.MapHeld);
			if ((terrain.defName == "TKKN_SaltField" || terrain.defName == "TKKN_Salted_Earth") && __instance.def.defName == "TKKN_giantsnail")
			{
				PatchTickPawn.BurnSnails(__instance);
				return;
			}

			if (!__instance.Dead)
			{
				if (!Find.TickManager.Paused)
				{
					PatchTickPawn.MakePaths(__instance);
					PatchTickPawn.MakeBreath(__instance);
					PatchTickPawn.MakeWet(__instance);
					PatchTickPawn.DyingCheck(__instance, terrain);
				}
			}
		}
		public static void DyingCheck(Pawn pawn, TerrainDef terrain)
		{
			//drowning == immobile and in water
			if (pawn.RaceProps.Humanlike && pawn.health.Downed && terrain.HasTag("TKKN_Wet"))
			{
				float damage = .05f;
				//if they're awake, take less damage
				if (!pawn.health.capacities.CanBeAwake)
				{
					damage = .01f;
				}

				//heavier clothing hurts them more
				List<Apparel> apparel = pawn.apparel.WornApparel;
				float weight = 0f;
				for (int i = 0; i < apparel.Count; i++)
				{
					weight += apparel[i].HitPoints / 100;
				}
				damage += weight / 50;
				HealthUtility.AdjustSeverity(pawn, HediffDef.Named("TKKN_Drowning"), damage);

				if (pawn.Faction.IsPlayer)
				{
					string text = "TKKN_NPS_DrowningText".Translate();
					Messages.Message(text, MessageTypeDefOf.NeutralEvent);

				}

			}
		}
		public static void MakeWet(Pawn pawn)
		{
			HediffDef hediffDef = HediffDefOf.TKKN_Wetness;
			if (pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef) == null && pawn.RaceProps.Humanlike)
			{
				Map map = pawn.MapHeld;
				IntVec3 c = pawn.Position;
				if (map == null || !c.IsValid)
				{
					return;
				}
				bool isWet = false;
				if (map.weatherManager.curWeather.rainRate > .001f)
				{
					Room room = c.GetRoom(map, RegionType.Set_All);
					bool roofed = map.roofGrid.Roofed(c);
					bool flag2 = room != null && room.UsesOutdoorTemperature;
					if (!roofed)
					{
						isWet = true;
					}

				}
				else
				{
					TerrainDef currentTerrain = c.GetTerrain(map);
					if (currentTerrain.HasTag("TKKN_Wet")){
						isWet = true;
					}
				}

				if (isWet)
				{
					Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, null);
					hediff.Severity = 0;
					pawn.health.AddHediff(hediff, null, null);
				}
			}
		}

		public static void BurnSnails(Pawn pawn)
		{
			BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire, null);
			Find.BattleLog.Add(battleLogEntry_DamageTaken);
			DamageInfo dinfo = new DamageInfo(DamageDefOf.Flame, 100, -1f, 0, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, pawn);
			dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
			pawn.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);
		}
		public static void MakePaths(Pawn pawn)
		{
			Map map = pawn.Map;
			Watcher watcher = map.GetComponent<Watcher>();
			if (watcher == null)
			{
				return;
			}
			#region paths
			if (pawn.Position.InBounds(map) && pawn.RaceProps.Humanlike)
			{
				//damage plants and remove snow/frost where they are. This will hopefully generate paths as pawns walk :)
				if (watcher.checkIfCold(pawn.Position))
				{
					map.GetComponent<FrostGrid>().AddDepth(pawn.Position, (float)-.05);
					map.snowGrid.AddDepth(pawn.Position, (float)-.05);
				}

				//pack down the soil.
				if (pawn.pather.MovingNow)
				{
					cellData cell = watcher.cellWeatherAffects[pawn.Position];
					cell.doPack();
				}
				if (Settings.allowPlantEffects)
				{
					//this will be handled by the terrain changing in doPack
			//		watcher.hurtPlants(pawn.Position, true, true);
				}
			}
			#endregion
		}

		public static void MakeBreath(Pawn pawn)
		{
			if (Find.TickManager.TicksGame % 150 == 0)
			{
				Map map = pawn.Map;
				Watcher watcher = map.GetComponent<Watcher>();

				bool isCold = watcher.checkIfCold(pawn.Position);
				if (isCold)
				{
					IntVec3 head = pawn.Position;
					head.z += 1;
					if (!head.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
					{
						return;
					}
					MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("TKKN_Mote_ColdBreath"), null);
					moteThrown.airTimeLeft = 99999f;
					moteThrown.Scale = Rand.Range(.5f, 1.5f);
					moteThrown.rotationRate = Rand.Range(-30f, 30f);
					moteThrown.exactPosition = head.ToVector3();
					moteThrown.SetVelocity((float)Rand.Range(20, 30), Rand.Range(0.5f, 0.7f));
					GenSpawn.Spawn(moteThrown, head, map);
				}
			}
		}
	}
	[HarmonyPatch(typeof(Pawn))]
	[HarmonyPatch("TickRare")]
	class PatchTickRarePawn
	{

		[HarmonyPostfix]
		public static void Postfix(Pawn __instance)
		{
			if (__instance == null || !__instance.Spawned || __instance.Dead ||(__instance.RaceProps.Humanlike && __instance.needs == null))
			{
				return;
			}
			HediffDef hediffDef = new HediffDef();
			TerrainDef terrain = __instance.Position.GetTerrain(__instance.Map);
			if (terrain.defName == "TKKN_HotSpringsWater")
			{
				if (__instance.needs.comfort != null)
				{
					__instance.needs.comfort.lastComfortUseTick--;
				}
				hediffDef = HediffDefOf.TKKN_hotspring_chill_out;
				if (__instance.health.hediffSet.GetFirstHediffOfDef(hediffDef) == null)
				{
					Hediff hediff = HediffMaker.MakeHediff(hediffDef, __instance, null);
					__instance.health.AddHediff(hediff, null, null);
				}
			}
			if (terrain.defName == "TKKN_ColdSpringsWater")
			{
				__instance.needs.rest.CurLevel++;
				hediffDef = HediffDefOf.TKKN_coldspring_chill_out;
				if (__instance.health.hediffSet.GetFirstHediffOfDef(hediffDef, false) == null)
				{
					Hediff hediff = HediffMaker.MakeHediff(hediffDef, __instance, null);
					__instance.health.AddHediff(hediff, null, null);
				}
			}
		}
	}
}