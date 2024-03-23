using HarmonyLib;
using RimWorld;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(GenSpawn), "Spawn", typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode),
    typeof(bool), typeof(bool))]
internal class PatchGenSpawnSpawn
{
    // Dont let it spawn  in lave
    [HarmonyPostfix]
    public static void Postfix(Thing newThing, IntVec3 loc, Map map)
    {
        var terrain = loc.GetTerrain(map);
        if (terrain == null || !terrain.HasTag("Lava"))
        {
            return;
        }

        if (newThing.GetStatValue(StatDefOf.Flammability) == 0f)
        {
            return;
        }

        if (newThing.def.altitudeLayer != AltitudeLayer.Item)
        {
            return;
        }

        if (!newThing.def.useHitPoints)
        {
            return;
        }

        if (Prefs.DevMode)
        {
            Log.Message($"{newThing} spawns in lava, setting on fire");
        }

        GenExplosion.DoExplosion(loc, map, 1, DamageDefOf.Flame, null, -1, -1f, null, null, null, null, null, 1f, 1,
            null, false, null, 0f, 1, 1f, false, null, null, null, true, 1f, 0f, true, null, 0);
    }
}