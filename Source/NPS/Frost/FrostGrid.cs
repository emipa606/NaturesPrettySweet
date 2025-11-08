using RimWorld;
using UnityEngine;
using Verse;

namespace TKKN_NPS;

public sealed class FrostGrid : MapComponent
{
    public const float MaxDepth = 1f;
    public const float currentFrost = 0f;

    private double totalDepth;

    public FrostGrid(Map map) : base(map)
    {
        DepthGridDirect_Unsafe = new float[map.cellIndices.NumGridCells];
    }

    internal float[] DepthGridDirect_Unsafe { get; }

    public float TotalDepth => (float)totalDepth;


    private bool CanHaveFrost(int ind)
    {
        var building = map.edificeGrid[ind];

        if (building != null && !CanCoexistWithFrost(building.def))
        {
            return false;
        }

        var terrainDef = map.terrainGrid.TerrainAt(ind);
        return terrainDef.HasModExtension<TerrainWeatherReactions>()
            ? terrainDef.GetModExtension<TerrainWeatherReactions>().holdFrost
            : terrainDef.holdSnowOrSand;

        //return terrainDef.affordances.Contains(TerrainAffordance.Light);
    }

    private bool CanCoexistWithFrost(ThingDef def)
    {
        return def.category != ThingCategory.Building; // || def.Fillage != FillCategory.Full;
    }

    public void AddDepth(IntVec3 c, float depthToAdd)
    {
        if (!c.InBounds(map))
        {
            return;
        }

        var num = map.cellIndices.CellToIndex(c);
        var num2 = DepthGridDirect_Unsafe[num];
        if (num2 <= 0f && depthToAdd < 0f)
        {
            return;
        }

        if (num2 >= 0.999f && depthToAdd > 1f)
        {
            return;
        }

        if (!CanHaveFrost(num))
        {
            DepthGridDirect_Unsafe[num] = 0f;
            return;
        }

        var num3 = num2 + depthToAdd;
        num3 = Mathf.Clamp(num3, 0f, 1f);
        var num4 = num3 - num2;
        totalDepth += num4;
        if (!(Mathf.Abs(num4) > 0.0001f))
        {
            return;
        }

        DepthGridDirect_Unsafe[num] = num3;
        CheckVisualOrPathCostChange(c, num2, num3);
    }

    public void SetDepth(IntVec3 c, float newDepth)
    {
        if (!c.InBounds(map))
        {
            return;
        }

        var num = map.cellIndices.CellToIndex(c);
        if (!CanHaveFrost(num))
        {
            DepthGridDirect_Unsafe[num] = 0f;
            return;
        }

        newDepth = Mathf.Clamp(newDepth, 0f, 1f);
        var num2 = DepthGridDirect_Unsafe[num];
        DepthGridDirect_Unsafe[num] = newDepth;
        var num3 = newDepth - num2;
        totalDepth += num3;
        CheckVisualOrPathCostChange(c, num2, newDepth);
    }

    private void CheckVisualOrPathCostChange(IntVec3 c, float oldDepth, float newDepth)
    {
        if (!map.GetComponent<Watcher>().cellWeatherAffects.TryGetValue(c, out var cell))
        {
            return;
        }

        cell.frostLevel = newDepth;
        if (Mathf.Approximately(oldDepth, newDepth))
        {
            return;
        }

        if (Mathf.Abs(oldDepth - newDepth) > 0.15f || Rand.Value < 0.0125f)
        {
            map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Snow, true, false);
            map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Things, true, false);
        }
        else if (newDepth == 0f)
        {
            map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Snow, true, false);
        }
    }

    public float GetDepth(IntVec3 c)
    {
        return !c.InBounds(map) ? 0f : DepthGridDirect_Unsafe[map.cellIndices.CellToIndex(c)];
    }

    public FrostCategory GetCategory(IntVec3 c)
    {
        return FrostUtility.GetFrostCategory(GetDepth(c));
    }
}