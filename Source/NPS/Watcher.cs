using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TKKN_NPS;

public class Watcher(Map map) : MapComponent(map)
{
    public readonly Dictionary<string, Graphic> graphicHolder = new();
    private readonly int howManyFloodSteps = 5;

    private readonly int howManyTideSteps = 13;

    private readonly int MaxPuddles = 50;

    //used to save data about active springs.
    public Dictionary<int, springData> activeSprings = new();

    private BiomeSeasonalSettings biomeSettings;
    private bool bugFixFrostIsRemoved;
    public Dictionary<IntVec3, cellData> cellWeatherAffects = new();
    private int cycleIndex;
    private bool doCoast = true; //false if no coast
    private List<List<IntVec3>> floodCellsList = [];

    private int floodLevel; // 0 - 3
    private int floodThreat;
    public float[] frostGrid;

    private ModuleBase frostNoise;
    private float humidity;
    public HashSet<IntVec3> lavaCellsList = [];
    public Thing overlay;

    //used by weather
    private bool regenCellLists = true;
    public List<IntVec3> swimmingCellsList = [];

    private int ticks;

    //rebuild every save to keep file size down
    private List<List<IntVec3>> tideCellsList = [];
    private int tideLevel; // 0 - 13
    private int totalPuddles;
    private int totalSprings;
    
    public FrostGrid frostGridComponent;
    private float outdoorTemp;

//		public Map mapRef;

    /* STANDARD STUFF */

    public override void MapComponentTick()
    {
        ticks++;
        base.MapComponentTick();


        //run through saved terrain and check it
        checkThingsforLava();

        //environmental changes
        if (Settings.doWeather)
        {
            //set up humidity
            outdoorTemp=map.mapTemperature.OutdoorTemp;
            
            var baseHumidity = (map.TileInfo.rainfall + 1) * (map.TileInfo.temperature + 1) *
                               (map.TileInfo.swampiness + 1);
            var currentHumidity =
                (1 + map.weatherManager.curWeather.rainRate) * (1 + outdoorTemp);
            humidity = ((baseHumidity + currentHumidity) / 1000) + 18;


            // this.checkRandomTerrain(); triggering on atmosphere affects
            doTides();
            doFloods();
            var num = Mathf.RoundToInt(map.Area * Settings.weatherCellUpdateSpeed / 2);
            var area = map.Area;
            for (var i = 0; i < num; i++)
            {
                if (cycleIndex >= area)
                {
                    cycleIndex = 0;
                }

                var c = map.cellsInRandomOrder.Get(cycleIndex);
                doCellEnvironment(c);
                cycleIndex++;
            }
        }

        updateBiomeSettings();
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref regenCellLists, "regenCellLists", true, true);

        Scribe_Collections.Look(ref activeSprings, "TKKN_activeSprings", LookMode.Value, LookMode.Deep);
        Scribe_Collections.Look(ref cellWeatherAffects, "cellWeatherAffects", LookMode.Value, LookMode.Deep);

        Scribe_Collections.Look(ref lavaCellsList, "lavaCellsList", LookMode.Value);

        Scribe_Values.Look(ref doCoast, "doCoast", true, true);
        Scribe_Values.Look(ref floodThreat, "floodThreat", 0, true);
        Scribe_Values.Look(ref tideLevel, "tideLevel", 0, true);
        Scribe_Values.Look(ref ticks, "ticks", 0, true);
        Scribe_Values.Look(ref totalPuddles, "totalPuddles", totalPuddles, true);
        Scribe_Values.Look(ref bugFixFrostIsRemoved, "bugFixFrostIsRemoved", bugFixFrostIsRemoved, true);
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        biomeSettings = map.Biome.GetModExtension<BiomeSeasonalSettings>();
        frostGridComponent = map.GetComponent<FrostGrid>();
        updateBiomeSettings(true);
        
        frostNoise = new Perlin(0.039999999105930328, 2.0, 0.5, 5,
            Rand.Range(0, 651431), QualityMode.Medium);

        rebuildCellLists();
        if (TKKN_Holder.modsPatched.Count > 0)
        {
            Log.Message($"TKKN NPS: Loaded patches for: {string.Join(", ", TKKN_Holder.modsPatched.ToArray())}");
        }
    }


    private void rebuildCellLists()
    {
        if (Settings.regenCells)
        {
            regenCellLists = Settings.regenCells;
        }


        /*
        #region devonly
        this.regenCellLists = true;
        Log.Error("DEV STUFF IS ON");
        this.cellWeatherAffects = new Dictionary<IntVec3, cellData>();
        #endregion
        */

        if (regenCellLists)
        {
            //spawn oasis. Do before cell list building, so it's stored correctly.
            spawnOasis();
            fixLava();

            var rot = Find.World.CoastDirectionAt(map.Tile);

            var tmpTerrain = map.AllCells.InRandomOrder(); //random so we can spawn plants and stuff in this step.
            cellWeatherAffects = new Dictionary<IntVec3, cellData>();
            foreach (var c in tmpTerrain)
            {
                var terrain = c.GetTerrain(map);

                if (!c.InBounds(map))
                {
                    continue;
                }

                var cell = new cellData { location = c, baseTerrain = terrain, howWetPlants = 70 };

                if (cell.originalTerrain != null)
                {
                    cell.originalTerrain = terrain;
                }

                if (terrain == TerrainDefOf.TKKN_Lava)
                {
                    //fix for lava pathing. If lava is near lava, make it impassable.
                    var edgeLava = false;
                    var num = GenRadial.NumCellsInRadius(1);
                    for (var i = 0; i < num; i++)
                    {
                        var lavaCheck = c + GenRadial.RadialPattern[i];
                        if (!lavaCheck.InBounds(map))
                        {
                            continue;
                        }

                        var lavaCheckTerrain = lavaCheck.GetTerrain(map);
                        if (lavaCheckTerrain != TerrainDefOf.TKKN_Lava &&
                            lavaCheckTerrain != TerrainDefOf.TKKN_LavaDeep)
                        {
                            edgeLava = true;
                        }
                    }

                    if (!edgeLava)
                    {
                        map.terrainGrid.SetTerrain(c, TerrainDefOf.TKKN_LavaDeep);
                    }
                }
                else if (rot.IsValid && terrain == RimWorld.TerrainDefOf.Sand ||
                         terrain == TerrainDefOf.TKKN_SandBeachWetSalt)
                {
                    //get all the sand pieces that are touching water.
                    for (var j = 0; j < howManyTideSteps; j++)
                    {
                        var waterCheck = adjustForRotation(rot, c, j);
                        if (!waterCheck.InBounds(map) || waterCheck.GetTerrain(map) != RimWorld.TerrainDefOf.WaterOceanShallow)
                        {
                            continue;
                        }

                        map.terrainGrid.SetTerrain(c, TerrainDefOf.TKKN_SandBeachWetSalt);
                        cell.tideLevel = j;
                        break;
                    }
                }
                else if (terrain != RimWorld.TerrainDefOf.WaterOceanShallow && terrain != RimWorld.TerrainDefOf.WaterOceanDeep &&
                         terrain.HasTag("TKKN_Wet"))
                {
                    for (var j = 0; j < howManyFloodSteps; j++)
                    {
                        var num = GenRadial.NumCellsInRadius(j);
                        for (var i = 0; i < num; i++)
                        {
                            var bankCheck = c + GenRadial.RadialPattern[i];
                            if (!bankCheck.InBounds(map))
                            {
                                continue;
                            }

                            var bankCheckTerrain = bankCheck.GetTerrain(map);
                            if (terrain == TerrainDefOf.TKKN_SandBeachWetSalt || bankCheckTerrain.HasTag("TKKN_Wet"))
                            {
                                continue;
                            }

                            //see if this cell has already been done, because we can have each cell in multiple flood levels.
                            var bankCell = cellWeatherAffects.TryGetValue(bankCheck, out var affect)
                                ? affect
                                : new cellData { location = bankCheck, baseTerrain = bankCheckTerrain };

                            bankCell.floodLevel.Add(j);
                        }
                    }
                }

                //Spawn special elements:
                spawnSpecialElements(c);
                spawnSpecialPlants(c);

                cellWeatherAffects[c] = cell;
            }
        }


        //rebuild lookup lists.
        lavaCellsList = [];
        swimmingCellsList = [];
        tideCellsList = [];
        floodCellsList = [];

        for (var k = 0; k < howManyTideSteps; k++)
        {
            tideCellsList.Add([]);
        }

        for (var k = 0; k < howManyFloodSteps; k++)
        {
            floodCellsList.Add([]);
        }

        //var component = map.GetComponent<FrostGrid>();

        foreach (var thiscell in cellWeatherAffects)
        {
            cellWeatherAffects[thiscell.Key].map = map;
            if (!bugFixFrostIsRemoved)
            {
                thiscell.Value.doFrostOverlay("remove");
            }

            //temp fix until I can figure out why regenerate wasn't working
//				frostGrid.SetDepth(thiscell.Value.location, 0);
            frostGridComponent.SetDepth(thiscell.Value.location, thiscell.Value.frostLevel);
            /*
             Defs were removed in pre 1.0 version
            if (thiscell.Value.baseTerrain.defName == "TKKN_ColdSprings")
            {
                thiscell.Value.baseTerrain = TerrainDefOf.TKKN_ColdSpringsWater;
            }

            if (thiscell.Value.baseTerrain.defName == "TKKN_HotSprings")
            {
                thiscell.Value.baseTerrain = TerrainDefOf.TKKN_HotSpringsWater;
            }
            */
            if (thiscell.Value.tideLevel > -1)
            {
                tideCellsList[thiscell.Value.tideLevel].Add(thiscell.Key);
            }

            if (thiscell.Value.floodLevel.Count != 0)
            {
                foreach (var level in thiscell.Value.floodLevel)
                {
                    floodCellsList[level].Add(thiscell.Key);
                }
            }

            if (thiscell.Value.baseTerrain.HasTag("TKKN_Swim"))
            {
                swimmingCellsList.Add(thiscell.Key);
            }

            if (thiscell.Value.baseTerrain.HasTag("Lava"))
            {
                //future me: to do: split lava actions into ones that will affect pawns and ones that won't, since pawns can't walk on deep lava
                lavaCellsList.Add(thiscell.Key);
            }
        }

        bugFixFrostIsRemoved = true;

        if (!regenCellLists)
        {
            return;
        }

        setUpTidesBanks();
        regenCellLists = false;
    }

    private void spawnSpecialPlants(IntVec3 c)
    {
        //salt crystals:
        var terrain = c.GetTerrain(map);
        if (terrain == TerrainDefOf.TKKN_SaltField || terrain == TerrainDefOf.TKKN_SandBeachWetSalt)
        {
            if (c.GetEdifice(map) == null && c.GetCover(map) == null && Rand.Value < .003f)
            {
                var plant = (Plant)ThingMaker.MakeThing(ThingDefOf.TKKN_SaltCrystal);
                plant.Growth = Rand.Range(0.07f, 1f);
                if (plant.def.plant.LimitedLifespan)
                {
                    plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
                }

                GenSpawn.Spawn(plant, c, map);
            }
        }

        //barnacles and other ocean stuff
        if (terrain != TerrainDefOf.TKKN_SandBeachWetSalt)
        {
            return;
        }

        if (c.GetEdifice(map) != null || c.GetCover(map) != null || !(Rand.Value < .003f))
        {
            return;
        }

        var barnaclePlant = (Plant)ThingMaker.MakeThing(ThingDefOf.TKKN_PlantBarnacles);
        barnaclePlant.Growth = Rand.Range(0.07f, 1f);
        if (barnaclePlant.def.plant.LimitedLifespan)
        {
            barnaclePlant.Age = Rand.Range(0, Mathf.Max(barnaclePlant.def.plant.LifespanTicks - 50, 0));
        }

        GenSpawn.Spawn(barnaclePlant, c, map);
    }

    private void spawnSpecialElements(IntVec3 c)
    {
        var terrain = c.GetTerrain(map);


        //defaults
        var maxSprings = 3;
        var springSpawnChance = .8f;

        if (biomeSettings != null)
        {
            maxSprings = biomeSettings.maxSprings;
            springSpawnChance = biomeSettings.springSpawnChance;
        }

        if (!Settings.doSprings)
        {
            maxSprings = 0;
            springSpawnChance = 0;
        }

        foreach (var element in DefDatabase<ElementSpawnDef>.AllDefs)
        {
            var canSpawn = true;
            var isSpring = element.thingDef.defName.Contains("Spring");

            if (isSpring && maxSprings <= totalSprings)
            {
                canSpawn = false;
            }

            foreach (var biome in element.forbiddenBiomes)
            {
                if (map.Biome.defName != biome)
                {
                    continue;
                }

                canSpawn = false;
                break;
            }


            foreach (var biome in element.allowedBiomes)
            {
                if (map.Biome.defName == biome)
                {
                    continue;
                }

                canSpawn = false;
                break;
            }

            if (!canSpawn)
            {
                continue;
            }


            foreach (var allowed in element.terrainValidationAllowed)
            {
                if (terrain.defName == allowed)
                {
                    canSpawn = true;
                    break;
                }

                canSpawn = false;
            }

            foreach (var notAllowed in element.terrainValidationDisallowed)
            {
                if (!terrain.HasTag(notAllowed))
                {
                    continue;
                }

                canSpawn = false;
                break;
            }

            if (isSpring && canSpawn && Rand.Value < springSpawnChance)
            {
                var thing = ThingMaker.MakeThing(element.thingDef);
                GenSpawn.Spawn(thing, c, map);
                totalSprings++;
            }

            if (isSpring || !canSpawn || !(Rand.Value < .0001f))
            {
                continue;
            }

            var elementThing = ThingMaker.MakeThing(element.thingDef);
            GenSpawn.Spawn(elementThing, c, map);
        }
    }

    private void spawnOasis()
    {
        if (map.Biome == BiomeDefOf.TKKN_Oasis)
        {
            //spawn a big ol cold spring
            var springSpot = CellFinderLoose.TryFindCentralCell(map, 10, 15, x => !x.Roofed(map));
            var spring = (Spring)ThingMaker.MakeThing(ThingDefOf.TKKN_OasisSpring);
            GenSpawn.Spawn(spring, springSpot, map);
        }

        if (Rand.Value < .001f)
        {
            spawnOasis();
        }
    }

    private void fixLava()
    {
        //set so the area people land in will most likely not be lava.
        if (map.Biome != BiomeDefOf.TKKN_VolcanicFlow)
        {
            return;
        }

        var centerSpot = CellFinderLoose.TryFindCentralCell(map, 10, 15, x => !x.Roofed(map));
        var num = GenRadial.NumCellsInRadius(23);
        for (var i = 0; i < num; i++)
        {
            map.terrainGrid.SetTerrain(centerSpot + GenRadial.RadialPattern[i],
                TerrainDefOf.TKKN_LavaRock_RoughHewn);
        }
    }

    private static IntVec3 adjustForRotation(Rot4 rot, IntVec3 cell, int j)
    {
        var newDirection = new IntVec3(cell.x, cell.y, cell.z);
        if (rot == Rot4.North)
        {
            newDirection.z += j + 1;
        }
        else if (rot == Rot4.South)
        {
            newDirection.z -= j - 1;
        }
        else if (rot == Rot4.East)
        {
            newDirection.x += j + 1;
        }
        else if (rot == Rot4.West)
        {
            newDirection.x -= j - 1;
        }

        return newDirection;
    }

    private void setUpTidesBanks()
    {
        //set up tides and river banks for the first time:
        if (doCoast)
        {
            //set up for low tide
            tideLevel = 0;

            for (var i = 0; i < howManyTideSteps; i++)
            {
                var makeSand = tideCellsList[i];
                foreach (var c in makeSand)
                {
                    if (!cellWeatherAffects.TryGetValue(c, out var cell))
                    {
                        continue;
                    }

                    cell.baseTerrain = TerrainDefOf.TKKN_SandBeachWetSalt;
                    map.terrainGrid.SetTerrain(c, TerrainDefOf.TKKN_SandBeachWetSalt);
                }
            }

            //bring to current tide levels
            var level = getTideLevel();
            var max = 0;
            switch (level)
            {
                case FloodType.Normal:
                    max = (int)Math.Floor((howManyTideSteps - 1) / 2M);
                    break;
                case FloodType.High:
                    max = howManyTideSteps - 1;
                    break;
            }

            for (var i = 0; i < max; i++)
            {
                var makeSand = tideCellsList[i];
                foreach (var c in makeSand)
                {
                    if (!cellWeatherAffects.TryGetValue(c, out var cell))
                    {
                        continue;
                    }

                    cell.setTerrain(TerrainType.Tide);
                }
            }

            tideLevel = max;
        }

        var flood = getFloodType();

        for (var i = 0; i < howManyFloodSteps; i++)
        {
            var makeWater = floodCellsList[i];
            foreach (var c in makeWater)
            {
                if (!cellWeatherAffects.TryGetValue(c, out var cell))
                {
                    continue;
                }

                if (!cell.baseTerrain.HasTag("TKKN_Wet"))
                {
                    cell.baseTerrain = TerrainDefOf.TKKN_RiverDeposit;
                }

                switch (flood)
                {
                    case FloodType.High:
                        break;
                    case FloodType.Low:
                        cell.overrideType = "dry";
                        break;
                    default:
                    {
                        if (i >= howManyFloodSteps / 2)
                        {
                            cell.overrideType = "dry";
                        }

                        break;
                    }
                }

                cell.setTerrain(TerrainType.Flooded);
            }
        }
    }

    private void updateBiomeSettings(bool force = false)
    {
        if (biomeSettings == null)
        {
            return;
        }

        var location = Find.WorldGrid.LongLatOf(map.Tile);
        var season = GenDate.Season(Find.TickManager.TicksAbs, location);
        var quadrum = GenDate.Quadrum(Find.TickManager.TicksAbs, location.x);

        if (!force && (biomeSettings.lastChanged == season || biomeSettings.lastChangedQ == quadrum))
        {
            return;
        }

        //Log.Message("[NPS]: Updating seasonal settings");
        biomeSettings.setWeatherBySeason(map, season, quadrum);
        biomeSettings.setDiseaseBySeason(season, quadrum);
        biomeSettings.setIncidentsBySeason(season, quadrum);
        biomeSettings.lastChanged = season;
        biomeSettings.lastChangedQ = quadrum;
    }


    private void doCellEnvironment(IntVec3 c)
    {
        if (!cellWeatherAffects.TryGetValue(c, out var cell))
        {
            return;
        }

        cell.DoCellSteadyEffects();

        if (ticks % 2 == 0)
        {
            cell.unpack();
        }

        var currentTerrain = c.GetTerrain(map);
        var roofed = map.roofGrid.Roofed(c);

        var gettingWet = false;
        cell.gettingWet = false;

        //check if the terrain has been floored
        if (currentTerrain.designationCategory == DesignationCategoryDefOf.Floors)
        {
            cell.baseTerrain = currentTerrain;
        }

        //spawn special things
        if (Rand.Value < .0001f)
        {
            if (c.InBounds(map))
            {
                if (currentTerrain == TerrainDefOf.TKKN_Lava) {
                    var thing = ThingMaker.MakeThing(ThingDefOf.TKKN_LavaRock);
                    GenSpawn.Spawn(thing, c, map);
                }
                else if (currentTerrain == TerrainDefOf.TKKN_LavaRock_RoughHewn &&
                         map.Biome == BiomeDefOf.TKKN_VolcanicFlow &&
                         map.listerThings.ThingsOfDef(ThingDefOf.TKKN_SteamVent).Count < 10) 
                {
                    var thing = ThingMaker.MakeThing(ThingDefOf.TKKN_SteamVent);
                    GenSpawn.Spawn(thing, c, map);
                }
                
            }
        }


        if (Settings.showRain && !cell.currentTerrain.HasTag("TKKN_Wet"))
        {
            //if it's raining in this cell:
            if (!roofed && map.weatherManager.curWeather.rainRate > .0001f)
            {
                if (floodThreat < 1090000)
                {
                    floodThreat += 1 + (2 * (int)Math.Round(map.weatherManager.curWeather.rainRate));
                }

                gettingWet = true;
                cell.gettingWet = true;
                cell.setTerrain(TerrainType.Wet);
            }
            else if (Settings.showRain && !roofed && map.weatherManager.curWeather.snowRate > .001f)
            {
                gettingWet = true;
                cell.gettingWet = true;
                cell.setTerrain(TerrainType.Wet);
            }
            else
            {
                if (map.weatherManager.curWeather.rainRate == 0)
                {
                    floodThreat--;
                }

                //DRY GROUND
                cell.setTerrain(TerrainType.Dry);
            }
        }

        var isCold = checkIfCold(c);

        if (isCold)
        {
            if (Settings.doIce)
            {
                cell.setTerrain(TerrainType.Frozen);
            }

            //handle frost based on snowing
            if (!roofed && map.weatherManager.SnowRate > 0.001f)
            {
                frostGridComponent.AddDepth(c, map.weatherManager.SnowRate * -.01f);
            }
            else
            {
                CreepFrostAt(c, 0.46f * .3f, map);
            }
        }
        else
        {
            cell.setTerrain(TerrainType.Thaw);
            var frosty = cell.temperature * -.025f;
//				float frosty = this.map.mapTemperature.OutdoorTemp * -.03f;
            frostGridComponent.AddDepth(c, frosty);
            /*
            if (map.GetComponent<FrostGrid>().GetDepth(c) > .3f)
            {
                // cell.isMelt = true;
            }
            */
        }


        //HANDLE PLANT DAMAGES:
        if (gettingWet)
        {
            //note - removed ismelt because the dirt shouldn't dry out in winter, and snow wets the ground then.
            if (cell.howWetPlants < 100)
            {
                if (map.weatherManager.curWeather.rainRate > 0)
                {
                    cell.howWetPlants += map.weatherManager.curWeather.rainRate * 2;
                }
                else if (map.weatherManager.curWeather.snowRate > 0)
                {
                    cell.howWetPlants += map.weatherManager.curWeather.snowRate * 2;
                }
            }
        }
        else {
            if (outdoorTemp > 20)
            {
                cell.howWetPlants += -1 * (outdoorTemp / humidity / 10);
                if (cell.howWetPlants <= 0)
                {
                    if (cell.currentTerrain.HasModExtension<TerrainWeatherReactions>())
                    {
                        var weather = cell.currentTerrain.GetModExtension<TerrainWeatherReactions>();
                        if (weather.dryTerrain == null)
                        {
                            //only hurt plants on terrain that's not wet.
                            hurtPlants(c, false, true);
                        }
                    }
                    else
                    {
                        hurtPlants(c, false, true);
                    }
                }
            }
        }

        switch (cell.howWet)
        {
            case < 3 when Settings.showRain && (cell.isMelt || gettingWet):
                cell.howWet += 2;
                break;
            case > -1:
                cell.howWet--;
                break;
        }

        //PUDDLES
        var puddle = (from t in c.GetThingList(map)
            where t.def == ThingDefOf.TKKN_FilthPuddle
            select t).FirstOrDefault();

        switch (cell.howWet)
        {
            case 3 when !isCold && MaxPuddles > totalPuddles &&
                        cell.currentTerrain != TerrainDefOf.TKKN_SandBeachWetSalt:
            {
                if (puddle == null)
                {
                    FilthMaker.TryMakeFilth(c, map, ThingDefOf.TKKN_FilthPuddle);
                    totalPuddles++;
                }

                break;
            }
            case <= 0 when puddle != null:
                puddle.Destroy();
                totalPuddles--;
                break;
        }

        cell.isMelt = false;

        cellWeatherAffects[c] = cell;
    }

    public bool checkIfCold(IntVec3 c)
    {
        if (!Settings.showCold)
        {
            if (cellWeatherAffects.TryGetValue(c, out var affect) && affect.temperature < -998)
            {
                affect.temperature = c.GetTemperature(map);
            }

            return false;
        }

        if (!cellWeatherAffects.TryGetValue(c, out var cell))
        {
            return false;
        }

        var room = c.GetRoom(map);

        var isCold = false;
        if (room == null || room.UsesOutdoorTemperature) {
            cell.temperature = outdoorTemp;
            if (outdoorTemp < 0f)
            {
                isCold = true;
            }
        }

        if (room == null || room.UsesOutdoorTemperature)
        {
            return isCold;
        }

        var temperature = room.Temperature;
        cell.temperature = temperature;
        if (temperature < 0f)
        {
            isCold = true;
        }

        return isCold;
    }

    private bool checkIfHot(IntVec3 c)
    {
        if (!Settings.showHot)
        {
            return false;
        }

        if (!cellWeatherAffects.TryGetValue(c, out var cell))
        {
            return false;
        }

        var room = c.GetRoom(map);

        var isHot = false;
        if (room == null || room.UsesOutdoorTemperature)
        {
            cell.temperature = outdoorTemp;
            if (outdoorTemp > 37f)
            {
                isHot = true;
            }
        }

        if (room == null || room.UsesOutdoorTemperature)
        {
            return isHot;
        }

        var temperature = room.Temperature;
        cell.temperature = temperature;
        if (temperature > 37f)
        {
            isHot = true;
        }

        return isHot;
    }

    private void CreepFrostAt(IntVec3 c, float baseAmount, Map map)
    {


        var num = frostNoise.GetValue(c);
        num += 1f;
        num *= 0.5f;
        if (num < 0.5f)
        {
            num = 0.5f;
        }

        var depthToAdd = baseAmount * num;

        frostGridComponent.AddDepth(c, depthToAdd);
    }

    private FloodType getFloodType()
    {
        var flood = FloodType.Normal;
        var season = GenLocalDate.Season(map);
        if (floodThreat > 1000000 || season == Season.Spring)
        {
            flood = FloodType.High;
        }
        else if (season == Season.Fall)
        {
            flood = FloodType.Low;
        }

        var isDrought = map.gameConditionManager.GetActiveCondition<GameCondition_Drought>();
        if (isDrought != null)
        {
            flood = isDrought.floodOverride;
        }

        return flood;
    }

    private void doFloods()
    {
        if (!Settings.doFloods || ticks % 300 != 0)
        {
            return;
        }

        var half = (int)Math.Round((howManyFloodSteps - 1M) / 2);
        var max = howManyFloodSteps - 1;


        var flood = getFloodType();


        var overrideType = "";
        if (floodLevel < max && flood == FloodType.High)
        {
            overrideType = "wet";
        }
        else if (floodLevel > 0 && flood == FloodType.Low)
        {
            overrideType = "dry";
        }
        else if (floodLevel < half && flood == FloodType.Normal)
        {
            overrideType = "wet";
        }
        else if (floodLevel > half && flood == FloodType.Normal)
        {
            overrideType = "dry";
        }

        if (floodLevel == howManyFloodSteps && flood == FloodType.High)
        {
            return;
        }

        if (floodLevel == 0 && flood == FloodType.Low)
        {
            return;
        }

        if (floodLevel == half && flood == FloodType.Normal)
        {
            return;
        }

        var cellsToChange = floodCellsList[floodLevel];
        foreach (var c in cellsToChange)
        {
            if (!cellWeatherAffects.TryGetValue(c, out var cell))
            {
                continue;
            }

            if (overrideType != "")
            {
                cell.overrideType = overrideType;
            }

            cell.setTerrain(TerrainType.Flooded);
        }

        if (floodLevel < max && flood == FloodType.High)
        {
            floodLevel++;
        }
        else if (floodLevel > 0 && flood == FloodType.Low)
        {
            floodLevel--;
        }
        else if (floodLevel < half && flood == FloodType.Normal)
        {
            floodLevel++;
        }
        else if (floodLevel > half && flood == FloodType.Normal)
        {
            floodLevel--;
        }
    }

    private FloodType getTideLevel()
    {
        if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))
        {
            return FloodType.High;
        }

        if (GenLocalDate.HourOfDay(map) > 4 && GenLocalDate.HourOfDay(map) < 8)
        {
            return FloodType.Low;
        }

        if (GenLocalDate.HourOfDay(map) > 15 && GenLocalDate.HourOfDay(map) < 20)
        {
            return FloodType.High;
        }

        return FloodType.Normal;
    }

    private void doTides()
    {
        //notes to future me: use this.howManyTideSteps - 1, so we always have a little bit of wet sand, or else it looks stupid.
        if (!doCoast || !Settings.doTides || ticks % 100 != 0)
        {
            return;
        }

        var tideType = getTideLevel();
        var half = (int)Math.Round((howManyTideSteps - 1M) / 2);
        var max = howManyTideSteps - 1;

        switch (tideType)
        {
            case FloodType.Normal when tideLevel == half:
            case FloodType.High when tideLevel == max:
            case FloodType.Low when tideLevel == 0:
                return;
            case FloodType.Normal when tideLevel == max:
                tideLevel--;
                return;
        }

        var cellsToChange = tideCellsList[tideLevel];
        foreach (var c in cellsToChange)
        {
            if (!cellWeatherAffects.TryGetValue(c, out var cell))
            {
                continue;
            }

            switch (tideType)
            {
                case FloodType.High:
                    cell.overrideType = "wet";
                    break;
                case FloodType.Low:
                    cell.overrideType = "dry";
                    break;
            }

            cell.setTerrain(TerrainType.Tide);
        }

        switch (tideType)
        {
            case FloodType.High:
            {
                if (tideLevel < max)
                {
                    tideLevel++;
                }

                break;
            }
            case FloodType.Low:
            {
                if (tideLevel > 0)
                {
                    tideLevel--;
                }

                break;
            }
            case FloodType.Normal when tideLevel > half:
                tideLevel--;
                break;
            case FloodType.Normal:
            {
                if (tideLevel < half)
                {
                    tideLevel++;
                }

                break;
            }
        }
    }

    private void hurtPlants(IntVec3 c, bool onlyLow, bool saveHarvest)
    {
        if (!Settings.allowPlantEffects || ticks % 150 != 0)
        {
            return;
        }

        //don't hurt things in growing zone
        if (map.zoneManager.ZoneAt(c) is Zone_Growing)
        {
            return;
        }

        var things = c.GetThingList(map);
        foreach (var thing in things.ToList())
        {
            if (thing is not Plant)
            {
                continue;
            }

            var isLow = true;
            if (onlyLow)
            {
                isLow = thing.def.altitudeLayer == AltitudeLayer.LowPlant;
            }

            var isHarvestable = true;
            if (saveHarvest)
            {
                isHarvestable = thing.def.plant.harvestTag != "Standard";
            }

            if (thing.def.category != ThingCategory.Plant || !isLow || !isHarvestable)
            {
                continue;
            }

            var damage = -.001f;
            damage *= thing.def.plant.fertilityMin;
            thing.TakeDamage(new DamageInfo(DamageDefOf.Rotting, damage, 0, 0));
        }
    }
    private HashSet<IntVec3> removeFromLava = new HashSet<IntVec3>();
    private void checkThingsforLava()
    {
        removeFromLava.Clear();

        foreach (var c in lavaCellsList)
        {
            if (!cellWeatherAffects.TryGetValue(c, out var cell))
            {
                continue;
            }

            //check to see if it's still lava. Ignore roughhewn because lava can freeze/rain will cool it.
            if (c.GetTerrain(map).HasTag("Lava") || c.GetTerrain(map) == TerrainDefOf.TKKN_LavaRock_RoughHewn)
            {
                continue;
            }

            cell.baseTerrain = c.GetTerrain(map);
            removeFromLava.Add(c);
        }

        foreach (var c in removeFromLava)
        {
            lavaCellsList.Remove(c);
        }


        var n = 0;
        foreach (var c in lavaCellsList.InRandomOrder())
        {
            GenTemperature.PushHeat(c, map, 1);
            if (n > 50)
            {
                break;
            }

            if (map.weatherManager.curWeather.rainRate > .0001f)
            {
                if (Rand.Value < .0009f)
                {
                    FleckMaker.ThrowHeatGlow(c, map, 5f);
                    FleckMaker.ThrowSmoke(c.ToVector3(), map, 4f);
                }
            }
            else
            {
                if (Rand.Value < .0005f)
                {
                    FleckMaker.ThrowSmoke(c.ToVector3(), map, 4f);
                }
            }

            n++;
        }
    }
}