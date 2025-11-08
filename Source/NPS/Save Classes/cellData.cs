using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TKKN_NPS;

public class cellData : IExposable
{
    private readonly int packAt = 750;
    public TerrainDef baseTerrain;
    public HashSet<int> floodLevel = [];
    public float frostLevel;

    public bool gettingWet = false;
    public int howPacked;
    public int howWet;
    public float howWetPlants = 60;
    public bool isFlooded;
    public bool isFrozen;
    public bool isMelt;
    public bool isThawed = true;
    public bool isWet;
    public IntVec3 location;
    public Map map;
    public TerrainDef originalTerrain;

    public string overrideType = "";
    public float temperature = -9999;

    public int tideLevel = -1;


    public TerrainWeatherReactions weather => baseTerrain.HasModExtension<TerrainWeatherReactions>()
        ? baseTerrain.GetModExtension<TerrainWeatherReactions>()
        : null;

    public TerrainDef currentTerrain => location.GetTerrain(map);


    public void ExposeData()
    {
        Scribe_Values.Look(ref tideLevel, "tideLevel", tideLevel, true);
        Scribe_Collections.Look(ref floodLevel, "floodLevel", LookMode.Value);
        Scribe_Values.Look(ref howPacked, "howPacked", howPacked, true);
        Scribe_Values.Look(ref howWet, "howWet", howWet, true);
        Scribe_Values.Look(ref howWetPlants, "howWetPlants", howWetPlants, true);
        Scribe_Values.Look(ref frostLevel, "frostLevel", frostLevel, true);
        Scribe_Values.Look(ref isWet, "isWet", isWet, true);
        Scribe_Values.Look(ref isFlooded, "isFlooded", isFlooded, true);
        Scribe_Values.Look(ref isMelt, "isMelt", isMelt, true);
        Scribe_Values.Look(ref overrideType, "overrideType", overrideType, true);

        Scribe_Values.Look(ref isThawed, "isThawed", isThawed, true);


        Scribe_Values.Look(ref location, "location", location, true);
        Scribe_Values.Look(ref temperature, "temperature", -999, true);
        Scribe_Defs.Look(ref baseTerrain, "baseTerrain");

        Scribe_Defs.Look(ref originalTerrain, "originalTerrain");
    }

    public void setTerrain(TerrainType type) {
        var thisTerrain = currentTerrain;
        //Make sure it hasn't been made a floor or a floor hasn't been removed.
        if (!thisTerrain.HasModExtension<TerrainWeatherReactions>())
        {
            baseTerrain = thisTerrain;
        }
        else if (baseTerrain != thisTerrain && !baseTerrain.HasModExtension<TerrainWeatherReactions>())
        {
            baseTerrain = thisTerrain;
        }

        if (weather == null)
        {
            return;
        }

        switch (type)
        {
            //change the terrain
            case TerrainType.Frozen:
                setFrozenTerrain(true);
                break;
            case TerrainType.Dry:
            case TerrainType.Wet:
                setWetTerrain();
                break;
            case TerrainType.Thaw when isFrozen:
                howWet = 1;
                setWetTerrain();
                isFrozen = false;
                break;
            case TerrainType.Thaw:
                setFrozenTerrain(false);
                break;
            case TerrainType.Flooded:
                setFloodedTerrain();
                break;
            case TerrainType.Tide:
                setTidesTerrain();
                break;
        }

        overrideType = "";
    }

    public void DoCellSteadyEffects()
    {
        if (howWetPlants < 0)
        {
            howWetPlants = 0;
        }
    }

    private void setWetTerrain()
    {
        if (!Settings.showRain)
        {
            return;
        }
        var thisTerrain=currentTerrain;

        if (weather.wetTerrain != null && thisTerrain != weather.wetTerrain && howWet > weather.wetAt)
        {
            changeTerrain(weather.wetTerrain);
            /*
            if (baseTerrain == TerrainDefOf.TKKN_Lava)
            {
                map.GetComponent<Watcher>().lavaCellsList.Remove(location);
            }
            */
            isWet = true;
            rainSpawns();
        }
        else if (howWet == 0 && thisTerrain != baseTerrain && isWet && !isFlooded)
        {
            changeTerrain(baseTerrain);
            /*
            if (baseTerrain == TerrainDefOf.TKKN_Lava)
            {
                map.GetComponent<Watcher>().lavaCellsList.Add(location);
            }
            */

            isWet = false;
            howWet = -1;
        }
        else if (howWet == -1 && weather.dryTerrain != null && !isFlooded)
        {
            if (thisTerrain == weather.dryTerrain && baseTerrain == weather.dryTerrain)
            {
                return;
            }

            isWet = false;
            baseTerrain = weather.dryTerrain;
            changeTerrain(weather.dryTerrain);
        }

        //			*/
    }

    private void setFrozenTerrain(bool frozen)
    {
        if (frozen)
        {
            if (!(temperature < 0) || !(temperature < weather.freezeAt) || weather.freezeTerrain == null)
            {
                return;
            }
            var thisTerrain = currentTerrain;

            if (isFlooded && weather.freezeTerrain != thisTerrain)
            {
                if (thisTerrain.HasModExtension<TerrainWeatherReactions>())
                {
                    var curWeather = thisTerrain.GetModExtension<TerrainWeatherReactions>();
                    changeTerrain(curWeather.freezeTerrain);
                }
            }
            else if (!isFrozen)
            {
                changeTerrain(weather.freezeTerrain);
                /*
                if (baseTerrain == TerrainDefOf.TKKN_Lava)
                {
                    map.GetComponent<Watcher>().lavaCellsList.Remove(location);
                }
                */
            }

            isFrozen = true;
            isThawed = false;
            return;
        }

        if (isThawed)
        {
            return;
        }
        /*
        if (baseTerrain == TerrainDefOf.TKKN_Lava)
        {
            map.GetComponent<Watcher>().lavaCellsList.Add(location);
        }
        */
        isFrozen = false;
        isThawed = true;
        changeTerrain(baseTerrain);
    }

    private void setFloodedTerrain()
    {
        if (!Settings.showRain || !Settings.doTides)
        {
            return;
        }
        var thisTerrain = currentTerrain;
        var floodTerrain = weather.floodTerrain;
        if (isFrozen)
        {
            var currWeather = thisTerrain.GetModExtension<TerrainWeatherReactions>();
            var frozenTerrain = currWeather.freezeTerrain;
            if (frozenTerrain != null)
            {
                changeTerrain(frozenTerrain);
            }
        }
        else if (overrideType == "dry")
        {
            howWetPlants = 100;
            floodTerrain = baseTerrain;
            changeTerrain(floodTerrain);
        }
        else if (floodTerrain != null && thisTerrain != floodTerrain)
        {
            changeTerrain(floodTerrain);

            isFlooded = true;
            if (!floodTerrain.IsWater)
            {
                isFlooded = false;
                howWetPlants = 100;
                leaveLoot();
            }
            else
            {
                clearLoot();
            }
        }
    }

    private void setTidesTerrain()
    {
        if (!Settings.doTides)
        {
            return;
        }
        var thisTerrain = currentTerrain;
        switch (overrideType)
        {
            case "dry":
                changeTerrain(baseTerrain);
                break;
            case "wet":
                changeTerrain(weather.tideTerrain);
                break;
            default:
            {
                changeTerrain(thisTerrain != baseTerrain ? baseTerrain : weather.tideTerrain);
                break;
            }
        }

        if (weather.tideTerrain == null)
        {
            return;
        }

        if (TerrainTagUtil.TKKN_Wet.Contains(thisTerrain))
        {
            clearLoot();
        }
        else
        {
            leaveLoot();
        }
    }

    public void doFrostOverlay(string action)
    {
        if (!location.InBounds(map))
        {
            return;
        }

        //KEEPING TO REMOVE OLD WAY OF DOING FROST
        var overlayIce = (from t in location.GetThingList(map)
            where t.def == ThingDefOf.TKKN_IceOverlay
            select t).FirstOrDefault();
        if (overlayIce == null)
        {
            return;
        }

        if (isFrozen)
        {
            isMelt = true;
        }

        overlayIce.Destroy();
    }


    public void unpack()
    {
        var thisTerrain = currentTerrain;
        if (!Settings.doDirtPath)
        {
            if (thisTerrain == TerrainDefOf.TKKN_DirtPath)
            {
                changeTerrain(RimWorld.TerrainDefOf.Soil);
            }

            if (thisTerrain == TerrainDefOf.TKKN_SandPath)
            {
                changeTerrain(RimWorld.TerrainDefOf.Sand);
            }

            return;
        }

        if (howPacked > packAt)
        {
            howPacked = packAt;
        }

        if (howPacked > 0)
        {
            howPacked--;
        }
        else if (howPacked <= packAt / 2 && thisTerrain == TerrainDefOf.TKKN_DirtPath)
        {
            changeTerrain(RimWorld.TerrainDefOf.Soil);
        }
        else if (howPacked <= packAt / 2 && thisTerrain == TerrainDefOf.TKKN_SandPath)
        {
            changeTerrain(RimWorld.TerrainDefOf.Sand);
        }
    }

    public void doPack()
    {
var thisTerrain = currentTerrain;
        if (map.zoneManager.ZoneAt(location) is Zone_Growing &&
            (thisTerrain != TerrainDefOf.TKKN_DirtPath &&
             thisTerrain != TerrainDefOf.TKKN_SandPath))
        {
            return;
        }

        //don't pack if there's a growing zone.
        if (baseTerrain == RimWorld.TerrainDefOf.Soil 
            || baseTerrain == RimWorld.TerrainDefOf.Sand
            || baseTerrain.texturePath == "Terrain/Surfaces/RoughStone")
        {
            howPacked++;
        }

        if (howPacked > packAt)
        {
            //	this.howPacked = this.packAt;
            if (baseTerrain == RimWorld.TerrainDefOf.Soil)
            {
                var packed = TerrainDefOf.TKKN_DirtPath;
                changeTerrain(packed);
                baseTerrain = packed;
            }

            if (baseTerrain == RimWorld.TerrainDefOf.Sand)
            {
                var packed = TerrainDefOf.TKKN_SandPath;
                changeTerrain(packed);
                baseTerrain = packed;
            }
        }

        if (baseTerrain.texturePath != "Terrain/Surfaces/RoughStone" || howPacked <= packAt * 10)
        {
            return;
        }

        var thisName = baseTerrain.defName;
        var replace = thisName.Replace("_Rough", "_Smooth").Replace("_SmoothHewn", "_Smooth");
        var terrain = TerrainDef.Named(replace);
        changeTerrain(terrain);
        baseTerrain = terrain;
    }

    private void changeTerrain(TerrainDef terrain)
    {
        if (terrain != null && terrain != currentTerrain)
        {
            map.terrainGrid.SetTerrain(location, terrain);
        }
    }

    private void rainSpawns()
    {
        //spawn special things when it rains.
        if (Rand.Value < .009) {
            if (baseTerrain == TerrainDefOf.TKKN_Lava) {
                GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TKKN_LavaRock), location, map);
            }
            else if (baseTerrain == TerrainDefOf.TKKN_SandBeachWetSalt) {
                //Log.Warning("Spawning crab");
                GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TKKN_crab), location, map);
            }
            else {
                if (TerrainTagUtil.TKKN_Wet.Contains(currentTerrain)) {
                    FleckMaker.WaterSplash(location.ToVector3(), map, 1, 1);
                }
            }
        }
        else if (Rand.Value < .04 && TerrainTagUtil.Lava.Contains(currentTerrain))
        {
            FleckMaker.ThrowSmoke(location.ToVector3(), map, 5);
        }
    }

    private void leaveLoot()
    {
        if (!Settings.leaveStuff)
        {
            return;
        }

        var leaveSomething = Rand.Value;
        if (leaveSomething < 0.001f)
        {
            var leaveWhat = Rand.Value;
            var allowed = new List<string>();
            switch (leaveWhat)
            {
                case > 0.1f:
                    //leave trash;
                    allowed =
                    [
                        "Filth_Slime",
                        "TKKN_FilthShells",
                        "TKKN_FilthPuddle",
                        "TKKN_FilthSeaweed",
                        "TKKN_FilthDriftwood",
                        "TKKN_Sculpture_Shell",
                        "Kibble",
                        "EggRoeFertilized",
                        "EggRoeUnfertilized"
                    ];
                    break;
                case > 0.05f:
                    //leave resource;
                    allowed =
                    [
                        "Steel",
                        "Cloth",
                        "WoodLog",
                        "Synthread",
                        "Hyperweave",
                        "Kibble",
                        "SimpleProstheticLeg",
                        "MedicineIndustrial",
                        "ComponentIndustrial",
                        "Neutroamine",
                        "Chemfuel",
                        "MealSurvivalPack",
                        "Pemmican"
                    ];
                    break;
                case > 0.03f:
                {
                    // leave treasure.
                    allowed =
                    [
                        "Silver",
                        "Plasteel",
                        "Gold",
                        "Uranium",
                        "Jade",
                        "Heart",
                        "Lung",
                        "BionicEye",
                        "ScytherBlade",
                        "ElephantTusk"
                    ];

                    string text = "TKKN_NPS_TreasureWashedUpText".Translate();
                    Messages.Message(text, MessageTypeDefOf.NeutralEvent);
                    break;
                }
                case > 0.02f:
                {
                    //leave ultrarare
                    allowed =
                    [
                        "AIPersonaCore",
                        "MechSerumHealer",
                        "MechSerumNeurotrainer",
                        "ComponentSpacer",
                        "MedicineUltratech",
                        "ThrumboHorn"
                    ];
                    string text = "TKKN_NPS_UltraRareWashedUpText".Translate();
                    Messages.Message(text, MessageTypeDefOf.NeutralEvent);
                    break;
                }
            }

            if (allowed.Count <= 0)
            {
                return;
            }

            var leaveWhat2 = Rand.Range(1, allowed.Count) - 1;
            var loot = ThingMaker.MakeThing(ThingDef.Named(allowed[leaveWhat2]));
            if (loot != null)
            {
                GenSpawn.Spawn(loot, location, map);
            }
        }
        else

            //grow water and shore plants:
        if (leaveSomething < 0.002f && location.GetPlant(map) == null && location.GetCover(map) == null)
        {
            var plants = map.Biome.AllWildPlants;
            for (var i = plants.Count - 1; i >= 0; i--)
            {
                //spawn some water plants:
                var plantDef = plants[i];
                if (!plantDef.HasModExtension<ThingWeatherReaction>())
                {
                    continue;
                }

                //_ = currentTerrain;
                var thingWeather = plantDef.GetModExtension<ThingWeatherReaction>();
                var okTerrains = thingWeather.allowedTerrains;
                if (okTerrains == null || !okTerrains.Contains<TerrainDef>(currentTerrain))
                {
                    continue;
                }

                var plant = (Plant)ThingMaker.MakeThing(plantDef);
                plant.Growth = Rand.Range(0.07f, 1f);
                if (plant.def.plant.LimitedLifespan)
                {
                    plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
                }

                GenSpawn.Spawn(plant, location, map);
                break;
            }
        }
    }

    private void clearLoot()
    {
        if (!location.IsValid)
        {
            return;
        }

        var things = location.GetThingList(map);
        var remove = new List<string>
        {
            "FilthSlime",
            "TKKN_FilthShells",
            "TKKN_FilthPuddle",
            "TKKN_FilthSeaweed",
            "TKKN_FilthDriftwood",
            "TKKN_Sculpture_Shell",
            "Kibble",
            "Steel",
            "Cloth",
            "WoodLog",
            "Synthread",
            "Hyperweave",
            "Kibble",
            "SimpleProstheticLeg",
            "MedicineIndustrial",
            "ComponentIndustrial",
            "Neutroamine",
            "Chemfuel",
            "MealSurvivalPack",
            "Pemmican",
            "Silver",
            "Plasteel",
            "Gold",
            "Uranium",
            "Jade",
            "Heart",
            "Lung",
            "BionicEye",
            "ScytherBlade",
            "ElephantTusk",
            "AIPersonaCore",
            "MechSerumHealer",
            "MechSerumNeurotrainer",
            "ComponentSpacer",
            "MedicineUltratech",
            "ThrumboHorn"
        };

        for (var i = things.Count - 1; i >= 0; i--)
        {
            if (remove.Contains(things[i].def.defName))
            {
                things[i].Destroy();
                continue;
            }

            //remove any plants that might've grown:

            if (things[i] is not Plant plant)
            {
                continue;
            }

            if (plant.def.HasModExtension<ThingWeatherReaction>())
            {
                //_ = currentTerrain;
                var thingWeather = plant.def.GetModExtension<ThingWeatherReaction>();
                var okTerrains = thingWeather.allowedTerrains;
                if (okTerrains.Contains<TerrainDef>(currentTerrain))
                {
                    continue;
                }

                Log.Warning($"Destroying {plant.def.defName} at {location} on {currentTerrain.defName}");
                plant.Destroy();
            }
            else
            {
                plant.Destroy();
            }
        }
    }
}