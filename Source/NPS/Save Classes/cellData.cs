using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TKKN_NPS;

public class cellData : IExposable
{
    public TerrainDef baseTerrain;
    public HashSet<int> floodLevel = new HashSet<int>();
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

    public int packAt = 750;
    public float temperature = -9999;

    public int tideLevel = -1;


    public TerrainWeatherReactions weather
    {
        get
        {
            if (baseTerrain.HasModExtension<TerrainWeatherReactions>())
            {
                return baseTerrain.GetModExtension<TerrainWeatherReactions>();
            }

            return null;
        }
    }

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

    public void setTerrain(string type)
    {
        //Make sure it hasn't been made a floor or a floor hasn't been removed.
        if (!currentTerrain.HasModExtension<TerrainWeatherReactions>())
        {
            baseTerrain = currentTerrain;
        }
        else if (!baseTerrain.HasModExtension<TerrainWeatherReactions>() && baseTerrain != currentTerrain)
        {
            baseTerrain = currentTerrain;
        }

        if (weather == null)
        {
            return;
        }

        //change the terrain
        if (type == "frozen")
        {
            setFrozenTerrain();
        }
        else if (type == "dry")
        {
            setWetTerrain();
        }
        else if (type == "wet")
        {
            setWetTerrain();
        }
        else if (type == "thaw")
        {
            if (isFrozen)
            {
                howWet = 1;
                setWetTerrain();
                isFrozen = false;
            }
            else
            {
                setFrozenTerrain();
            }
        }
        else if (type == "flooded")
        {
            setFloodedTerrain();
        }
        else if (type == "tide")
        {
            setTidesTerrain();
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

    public void setWetTerrain()
    {
        if (!Settings.showRain)
        {
            return;
        }

        if (weather.wetTerrain != null && currentTerrain != weather.wetTerrain && howWet > weather.wetAt)
        {
            changeTerrain(weather.wetTerrain);
            if (baseTerrain.defName == "TKKN_Lava")
            {
                map.GetComponent<Watcher>().lavaCellsList.Remove(location);
            }

            isWet = true;
            rainSpawns();
        }
        else if (howWet == 0 && currentTerrain != baseTerrain && isWet && !isFlooded)
        {
            changeTerrain(baseTerrain);
            if (baseTerrain.defName == "TKKN_Lava")
            {
                map.GetComponent<Watcher>().lavaCellsList.Add(location);
            }

            isWet = false;
            howWet = -1;
        }
        else if (howWet == -1 && weather.dryTerrain != null && !isFlooded)
        {
            if (currentTerrain == weather.dryTerrain && baseTerrain == weather.dryTerrain)
            {
                return;
            }

            isWet = false;
            baseTerrain = weather.dryTerrain;
            changeTerrain(weather.dryTerrain);
        }

        //			*/
    }

    public void setFrozenTerrain()
    {
        if (!Settings.showCold)
        {
            return;
        }

        if (temperature < 0 && temperature < weather.freezeAt && weather.freezeTerrain != null)
        {
            if (isFlooded && weather.freezeTerrain != currentTerrain)
            {
                if (currentTerrain.HasModExtension<TerrainWeatherReactions>())
                {
                    var curWeather = currentTerrain.GetModExtension<TerrainWeatherReactions>();
                    changeTerrain(curWeather.freezeTerrain);
                }
            }
            else if (!isFrozen)
            {
                changeTerrain(weather.freezeTerrain);
                if (baseTerrain.defName == "TKKN_Lava")
                {
                    map.GetComponent<Watcher>().lavaCellsList.Remove(location);
                }
            }

            isFrozen = true;
            isThawed = false;
        }
        else if (temperature > 0)
        {
            if (isThawed)
            {
                return;
            }

            if (baseTerrain.defName == "TKKN_Lava")
            {
                map.GetComponent<Watcher>().lavaCellsList.Add(location);
            }

            isFrozen = false;
            isThawed = true;
            changeTerrain(baseTerrain);
        }
    }

    public void setFloodedTerrain()
    {
        if (!Settings.showRain)
        {
            return;
        }

        var floodTerrain = weather.floodTerrain;
        if (isFrozen)
        {
            var currWeather = currentTerrain.GetModExtension<TerrainWeatherReactions>();
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
        else if (floodTerrain != null && currentTerrain != floodTerrain)
        {
            changeTerrain(floodTerrain);

            isFlooded = true;
            if (!floodTerrain.HasTag("Water"))
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

    public void setTidesTerrain()
    {
        if (!Settings.doTides)
        {
            return;
        }

        if (overrideType == "dry")
        {
            changeTerrain(baseTerrain);
        }
        else if (overrideType == "wet")
        {
            changeTerrain(weather.tideTerrain);
        }
        else if (currentTerrain != baseTerrain)
        {
            changeTerrain(baseTerrain);
        }
        else
        {
            changeTerrain(weather.tideTerrain);
        }

        if (weather.tideTerrain == null)
        {
            return;
        }

        if (currentTerrain.HasTag("TKKN_Wet"))
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
            where t.def.defName == "TKKN_IceOverlay"
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
        if (!Settings.doDirtPath)
        {
            if (currentTerrain.defName == "TKKN_DirtPath")
            {
                changeTerrain(RimWorld.TerrainDefOf.Soil);
            }

            if (currentTerrain.defName == "TKKN_SandPath")
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
        else if (howPacked <= packAt / 2 && currentTerrain.defName == "TKKN_DirtPath")
        {
            changeTerrain(RimWorld.TerrainDefOf.Soil);
        }
        else if (howPacked <= packAt / 2 && currentTerrain.defName == "TKKN_SandPath")
        {
            changeTerrain(RimWorld.TerrainDefOf.Sand);
        }
    }

    public void doPack()
    {
        if (!Settings.doDirtPath)
        {
            return;
        }

        if (map.zoneManager.ZoneAt(location) is Zone_Growing &&
            currentTerrain.defName is not ("TKKN_DirtPath" and "TKKN_SandPath"))
        {
            return;
        }

        //don't pack if there's a growing zone.
        if (baseTerrain.defName is "Soil" or "Sand" ||
            baseTerrain.texturePath == "Terrain/Surfaces/RoughStone")
        {
            howPacked++;
        }

        if (howPacked > packAt)
        {
            //	this.howPacked = this.packAt;
            if (baseTerrain.defName == "Soil")
            {
                var packed = TerrainDef.Named("TKKN_DirtPath");
                changeTerrain(packed);
                baseTerrain = packed;
            }

            if (baseTerrain.defName == "Sand")
            {
                var packed = TerrainDef.Named("TKKN_SandPath");
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
    /*
    public void doFrostOverlay(string action) {
        if (action == "add")
        {
            if (!Settings.showCold) {
                return;
            }
            Thing overlayIce = (Thing)(from t in location.GetThingList(this.map)
                                       where t.def.defName == "TKKN_IceOverlay"
                                       select t).FirstOrDefault<Thing>();
            if ((weather.freezeTerrain == null || currentTerrain != weather.freezeTerrain || weather.isSalty) && !currentTerrain.HasTag("Water") && overlayIce == null)
            {
                Thing ice = ThingMaker.MakeThing(ThingDefOf.TKKN_IceOverlay, null);
                GenSpawn.Spawn(ice, location, map);
            }
        }
        else
        {
            Thing overlayIce = (Thing)(from t in location.GetThingList(this.map)
                                       where t.def.defName == "TKKN_IceOverlay"
                                       select t).FirstOrDefault<Thing>();
            if (overlayIce != null)
            {
                if (isFrozen)
                {
                    isMelt = true;
                }
                overlayIce.Destroy();
            }

        }
    }
    */

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
        if (Rand.Value < .009)
        {
            if (baseTerrain.defName == "TKKN_Lava")
            {
                GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TKKN_LavaRock), location, map);
            }
            else if (baseTerrain.defName == "TKKN_SandBeachWetSalt")
            {
                Log.Warning("Spawning crab");
                GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TKKN_crab), location, map);
            }
            else if (currentTerrain.HasTag("TKKN_Wet"))
            {
                FleckMaker.WaterSplash(location.ToVector3(), map, 1, 1);
            }
        }
        else if (Rand.Value < .04 && currentTerrain.HasTag("Lava"))
        {
            FleckMaker.ThrowSmoke(location.ToVector3(), map, 5);
        }
    }

    private void leaveLoot()
    {
        var leaveSomething = Rand.Value;
        if (leaveSomething < 0.001f)
        {
            var leaveWhat = Rand.Value;
            var allowed = new List<string>();
            if (leaveWhat > 0.1f)
            {
                //leave trash;
                allowed = new List<string>
                {
                    "Filth_Slime",
                    "TKKN_FilthShells",
                    "TKKN_FilthPuddle",
                    "TKKN_FilthSeaweed",
                    "TKKN_FilthDriftwood",
                    "TKKN_Sculpture_Shell",
                    "Kibble",
                    "EggRoeFertilized",
                    "EggRoeUnfertilized"
                };
            }
            else if (leaveWhat > 0.05f)
            {
                //leave resource;
                allowed = new List<string>
                {
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
                };
            }
            else if (leaveWhat > 0.03f)
            {
                // leave treasure.
                allowed = new List<string>
                {
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
                };

                string text = "TKKN_NPS_TreasureWashedUpText".Translate();
                Messages.Message(text, MessageTypeDefOf.NeutralEvent);
            }
            else if (leaveWhat > 0.02f)
            {
                //leave ultrarare
                allowed = new List<string>
                {
                    "AIPersonaCore",
                    "MechSerumHealer",
                    "MechSerumNeurotrainer",
                    "ComponentSpacer",
                    "MedicineUltratech",
                    "ThrumboHorn"
                };
                string text = "TKKN_NPS_UltraRareWashedUpText".Translate();
                Messages.Message(text, MessageTypeDefOf.NeutralEvent);
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

                var unused = currentTerrain;
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
                var unused = currentTerrain;
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