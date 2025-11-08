using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TKKN_NPS;

public class SpringComp : SpringCompAbstract
{
    private readonly List<IntVec3> affectableCells = [];
    private readonly List<IntVec3> affectableCellsAtmosphere = [];
    private readonly List<IntVec3> boundaryCells = [];
    private readonly List<IntVec3> boundaryCellsRough = [];
    private int age;

    private string biomeName;
    private int makeAnotherAt = 400;

    private bool spawnThings;
    private StatusType status = StatusType.spawning;
    protected ModuleBase terrainNoise;
    private TerrainType? terrainType = TerrainType.Wet;
    private float width;

    protected CompProperties_Springs Props => (CompProperties_Springs)props;

    private int getID()
    {
        var numOnly = parent.ThingID.Replace(parent.def.defName, "");
        return int.Parse(numOnly);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        makeAnotherAt = Props.howOftenToChange * 4000;
        if (respawningAfterLoad)
        {
            var savedData = parent.Map.GetComponent<Watcher>().activeSprings[getID()];
            if (savedData != null)
            {
                biomeName = savedData.biomeName;
                makeAnotherAt = savedData.makeAnotherAt;
                age = savedData.age;
                Enum.TryParse(savedData.status, out status);
                width = savedData.width;
            }
        }
        else
        {
            status = StatusType.spawning;
            biomeName = parent.Map.Biome.defName;
            width = Props.startingRadius;

            var savedData = new springData
            {
                springID = parent.ThingID,
                biomeName = biomeName,
                makeAnotherAt = makeAnotherAt,
                age = age,
                status = nameof(status),
                width = width
            };

            parent.Map.GetComponent<Watcher>().activeSprings.Add(getID(), savedData);
        }

        changeShape();
        CompTickRare();
        if (!respawningAfterLoad)
        {
            fillBorder();
        }
    }

    public override void CompTickRare()
    {
        base.CompTickRare();
        spawnThings = false;
        age += 250;

        if (Props.howOftenToChange > 0 && age > Props.howOftenToChange && age % Props.howOftenToChange == 0)
        {
            changeShape();
        }

        if (status == StatusType.spawning)
        {
            var radius = width;
            width += .5f;
            if (radius >= Props.radius)
            {
                status = StatusType.stable;
            }
        }

        var makeAnother = (float)age / 6000 / 1000;
        if (Props.canReproduce && Rand.Value + makeAnother > .01f)
        {
            //see if we're going to add another spring spawner.
            status = StatusType.expand;
            makeAnotherAt += Props.weight;
        }

        if (status != StatusType.despawn)
        {
            if (status != StatusType.stable)
            {
                setCellsToAffect();
                foreach (var cell in affectableCells)
                {
                    terrainType = TerrainType.Wet;
                    affectCell(cell);
                    specialFXAffect(cell);
                }

                foreach (var cell in boundaryCellsRough)
                {
                    affectCell(cell);
                    specialFXAffect(cell);
                }

                foreach (var cell in boundaryCells)
                {
                    if (!(Rand.Value > .1))
                    {
                        continue;
                    }

                    terrainType = TerrainType.Dry;
                    affectCell(cell);
                }
            }
            else
            {
                foreach (var cell in affectableCells)
                {
                    specialFXAffect(cell);
                }
            }

            foreach (var unused in affectableCellsAtmosphere)
            {
                atmosphereAffectCell();
            }
        }

        if (Props.canReproduce && status == StatusType.despawn)
        {
            parent.Map.GetComponent<Watcher>().activeSprings.Remove(getID());
            parent.Destroy();
            return;
        }

        checkIfDespawn();
        saveValues();
    }

    private void setCellsToAffect()
    {
        if (status == StatusType.stable)
        {
            return;
        }

        var pos = parent.Position;
        var map = parent.Map;
        affectableCells.Clear();
        boundaryCellsRough.Clear();
        boundaryCells.Clear();
        affectableCellsAtmosphere.Clear();
        if (!pos.InBounds(map))
        {
            return;
        }

        var maxArea = (int)Math.Round(width + Props.borderSize + 5);

        var region = pos.GetRegion(map);
        if (region == null)
        {
            return;
        }

        RegionTraverser.BreadthFirstTraverse(region, (_, r) => r.door == null, delegate(Region r)
        {
            foreach (var current in r.Cells)
            {
                if (current.InHorDistOf(pos, width))
                {
                    affectableCells.Add(current);
                }
                else if (current.InHorDistOf(pos, width + 2))
                {
                    boundaryCellsRough.Add(current);
                }
                else if (current.InHorDistOf(pos, width + Props.borderSize + 1))
                {
                    boundaryCells.Add(current);
                }
                else if (current.InHorDistOf(pos, width + Props.borderSize + 5))
                {
                    affectableCellsAtmosphere.Add(current);
                }
            }

            return false;
        }, maxArea);
    }

    private void saveValues()
    {
        var savedData = parent.Map.GetComponent<Watcher>().activeSprings[getID()];
        if (savedData == null)
        {
            return;
        }

        savedData.biomeName = biomeName;
        savedData.makeAnotherAt = makeAnotherAt;
        savedData.age = age;
        savedData.status = nameof(status);
        savedData.width = width;
    }

    protected virtual void changeShape()
    {
        if (Props.howOftenToChange == 0)
        {
            return;
        }

        ModuleBase moduleBase = new Perlin(1.1, 1, 5, 3, Props.radius, QualityMode.Medium);
        moduleBase = new ScaleBias(0.2, 0.2, moduleBase);

        ModuleBase moduleBase2 = new DistFromAxis(2);
        moduleBase2 = new ScaleBias(.2, .2, moduleBase2);
        moduleBase2 = new Clamp(0, 1, moduleBase2);

        terrainNoise = new Add(moduleBase, moduleBase2);
    }

    protected override void springTerrain(IntVec3 loc)
    {
        if (terrainNoise == null)
        {
            terrainType = TerrainType.Dry;
            return;
        }

        var value = terrainNoise.GetValue(loc);
        value /= Props.radius;
        var dif = (int)Math.Floor(value);
        value -= dif;


        if (value < .1)
        {
            specialFX = true;
        }

        if (value < .8f)
        {
            terrainType = TerrainType.Wet;
            return;
        }

        if (value < .85f)
        {
            spawnThings = true;
        }

        terrainType = TerrainType.Dry;
    }

    private void checkIfDespawn()
    {
        if (biomeName == Props.commonBiome)
        {
            if (Rand.Value < .0001f)
            {
                status = StatusType.despawn;
            }
        }
        else
        {
            if (Rand.Value < .001f)
            {
                status = StatusType.despawn;
            }
        }
    }

    public override void fillBorder()
    {
        var map = parent.Map;
        var list = map.Biome.AllWildPlants.ToList();
        list.Add(Props.spawnProp);
        foreach (var c in boundaryCellsRough.InRandomOrder())
        {
            genPlants(c, map, list);
        }

        foreach (var c in boundaryCells.InRandomOrder())
        {
            genPlants(c, map, list);
        }
    }

    private static void genPlants(IntVec3 c, Map map, List<ThingDef> list)
    {
        if (c.GetEdifice(map) != null || c.GetCover(map) != null)
        {
            return;
        }

        var source = from def in list
            where def.CanEverPlantAt(c, map)
            select def;

        if (!source.Any())
        {
            return;
        }

        if (!source.TryRandomElementByWeight(def => plantChoiceWeight(def, map), out var thingDef))
        {
            return;
        }

        var plant = (Plant)ThingMaker.MakeThing(thingDef);
        plant.Growth = Rand.Range(0.07f, 1f);
        if (plant.def.plant.LimitedLifespan)
        {
            plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
        }

        GenSpawn.Spawn(plant, c, map);
    }

    private static float plantChoiceWeight(ThingDef def, Map map)
    {
        return map.Biome.CommonalityOfPlant(def) * def.plant.wildClusterWeight;
    }

    private void affectCell(IntVec3 c)
    {
        var isSpawnCell = false;
        if (!c.InBounds(parent.Map))
        {
            return;
        }

        if (terrainType == null)
        {
            springTerrain(c);
        }

        if (status != StatusType.despawn)
        {
            if (c == parent.Position)
            {
                isSpawnCell = true;
                terrainType = TerrainType.Wet;
            }

            //double check we're not adding a border into another instance.
            if (terrainType == TerrainType.Dry)
            {
                if (!doBorder(c))
                {
                    terrainType = TerrainType.Wet;
                }
            }
        }

        if (terrainType == TerrainType.Dry)
        {
            var num = c.GetThingList(parent.Map).Count;
            //spawn whatever special items surround this thing.
            parent.Map.terrainGrid.SetTerrain(c, Props.dryTile);

            if (num == 0 && spawnThings && Props.spawnProp != null)
            {
                var def = Props.spawnProp;
                GenSpawn.Spawn(def, c, parent.Map);
            }

            spawnThings = false;
        }
        else
        {
            specialCellAffects(c);
        }

        if (status == StatusType.expand)
        {
            if (!isSpawnCell)
            {
                GenSpawn.Spawn(ThingMaker.MakeThing(parent.def), c, parent.Map);
            }

            status = StatusType.stable;
        }

        if (parent.Map.GetComponent<Watcher>().cellWeatherAffects.TryGetValue(c, out var affect))
        {
            affect.baseTerrain = c.GetTerrain(parent.Map);
        }

        terrainType = null;
    }

    public override bool doBorder(IntVec3 c)
    {
        var currentTerrain = c.GetTerrain(parent.Map);
        if (currentTerrain == Props.wetTile)
        {
            return false;
        }

        return !TerrainTagUtil.TKKN_Wet.Contains(currentTerrain);
    }

    private void atmosphereAffectCell()
    {
        GenTemperature.PushHeat(parent, Props.temperature);
    }

    public override void specialCellAffects(IntVec3 c)
    {
        if (terrainType == TerrainType.Wet)
        {
            parent.Map.terrainGrid.SetTerrain(c, Props.wetTile);
        }

        FilthMaker.RemoveAllFilth(c, parent.Map);
    }

    protected override void specialFXAffect(IntVec3 c)
    {
        base.specialFXAffect(c);
    }

    private enum StatusType : byte
    {
        spawning,
        stable,
        expand,
        despawn
    }
}