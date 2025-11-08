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
        if (!TerrainTagUtil.TKKN_SwimOrLava.Contains(terrain))
        {
            return true;
        }

        return pawnCanOccupy(___pawn.Position, ___pawn) || tryRecoverFromUnwalkablePosition(___pawn);
    }

    private static bool tryRecoverFromUnwalkablePosition(Pawn pawn)
    {
        var tryRecoverFromUnwalkablePosition = false;
        foreach (var intVec3 in GenRadial.RadialPattern)
        {
            var intVec = pawn.Position + intVec3;
            if (!pawnCanOccupy(intVec, pawn))
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

    private static bool pawnCanOccupy(IntVec3 c, Pawn pawn)
    {
        if (!c.Walkable(pawn.Map))
        {
            return false;
        }

        var edifice = c.GetEdifice(pawn.Map);
        if (edifice is Building_Door buildingDoor && !buildingDoor.PawnCanOpen(pawn) && !buildingDoor.Open)
        {
            return false;
        }

        if (!pawn.RaceProps.Animal)
        {
            return true;
        }

        return !TerrainTagUtil.TKKN_SwimOrLava.Contains(c.GetTerrain(pawn.Map));
    }
}