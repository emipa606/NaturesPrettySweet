using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace TKKN_NPS;

[HarmonyPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.StartPath))]
public static class Pawn_PathFollower_StartPath
{
    public static bool Prefix(Pawn ___pawn)
    {
        if (!___pawn.RaceProps.Animal)
        {
            return true;
        }

        var terrain = ___pawn.Position.GetTerrain(___pawn.Map);
        if (!terrain.HasTag("TKKN_Swim") && !terrain.HasTag("TKKN_Lava"))
        {
            return true;
        }

        return PawnCanOccupy(___pawn.Position, ___pawn) || TryRecoverFromUnwalkablePosition(___pawn);
    }

    private static bool TryRecoverFromUnwalkablePosition(Pawn pawn)
    {
        var tryRecoverFromUnwalkablePosition = false;
        foreach (var intVec3 in GenRadial.RadialPattern)
        {
            var intVec = pawn.Position + intVec3;
            if (!PawnCanOccupy(intVec, pawn))
            {
                continue;
            }

            if (intVec == pawn.Position)
            {
                return true;
            }

            pawn.Position = intVec;
            pawn.Notify_Teleported(true, false);
            tryRecoverFromUnwalkablePosition = true;
            break;
        }

        if (!tryRecoverFromUnwalkablePosition)
        {
            pawn.Destroy();
        }

        return tryRecoverFromUnwalkablePosition;
    }

    private static bool PawnCanOccupy(IntVec3 c, Pawn pawn)
    {
        if (!c.Walkable(pawn.Map))
        {
            return false;
        }

        var edifice = c.GetEdifice(pawn.Map);
        if (edifice is Building_Door building_Door && !building_Door.PawnCanOpen(pawn) && !building_Door.Open)
        {
            return false;
        }

        if (!pawn.RaceProps.Animal)
        {
            return true;
        }

        return !c.GetTerrain(pawn.Map).HasTag("TKKN_Swim") && !c.GetTerrain(pawn.Map).HasTag("TKKN_Lava");
    }
}