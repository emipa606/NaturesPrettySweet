using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TKKN_NPS;

public class BiomeSeasonalSettings : DefModExtension
{
    //incident settings
    public List<ThingDef> bloomPlants;
    public bool diseaseCacheUpdated;
    public List<BiomeDiseaseRecord> fallDiseases;
    public List<TKKN_IncidentCommonalityRecord> fallEvents;
    public List<WeatherCommonalityRecord> fallWeathers;
    public Season lastChanged;
    public Quadrum lastChangedQ;

    //spring settings
    public int maxSprings;
    public bool plantCacheUpdated;

    public bool plantsAdded;
    public List<PawnKindDef> specialHerds;

    public List<BiomePlantRecord> specialPlants;

    //disease settings
    public List<BiomeDiseaseRecord> springDiseases;
    public List<TKKN_IncidentCommonalityRecord> springEvents;
    public float springSpawnChance;
    public bool springsSurviveDrought;
    public bool springsSurviveSummer;

    //weather settings
    public List<WeatherCommonalityRecord> springWeathers;
    public List<BiomeDiseaseRecord> summerDiseases;
    public List<TKKN_IncidentCommonalityRecord> summerEvents;
    public List<WeatherCommonalityRecord> summerWeathers;

    //misc settings
    public int wetPlantStart = 50;
    public List<BiomeDiseaseRecord> winterDiseases;
    public List<TKKN_IncidentCommonalityRecord> winterEvents;
    public List<WeatherCommonalityRecord> winterWeathers;


    public bool canPutOnTerrain(IntVec3 c, ThingDef thingDef, Map map)
    {
        var terrain = c.GetTerrain(map);

        //make sure plants are spawning on terrain that they're limited to:
        var weatherReaction = thingDef.GetModExtension<ThingWeatherReaction>();
        if (weatherReaction == null || terrain == null || weatherReaction.allowedTerrains == null)
        {
            return true;
        }

        //if they're only allowed to spawn in certain terrains, stop it from spawning.
        return weatherReaction.allowedTerrains.Contains(terrain);
    }

    public void setWeatherBySeason(Map map, Season season, Quadrum quadrum)
    {
        if (Season.Spring == season)
        {
            map.Biome.baseWeatherCommonalities = springWeathers;
        }
        else if (Season.Summer == season)
        {
            map.Biome.baseWeatherCommonalities = summerWeathers;
        }
        else if (Season.Fall == season)
        {
            map.Biome.baseWeatherCommonalities = fallWeathers;
        }
        else if (Season.Winter == season)
        {
            map.Biome.baseWeatherCommonalities = winterWeathers;
        }
        else
        {
            if (Quadrum.Aprimay == quadrum)
            {
                map.Biome.baseWeatherCommonalities = springWeathers;
            }
            else if (Quadrum.Decembary == quadrum)
            {
                map.Biome.baseWeatherCommonalities = winterWeathers;
            }
            else if (Quadrum.Jugust == quadrum)
            {
                map.Biome.baseWeatherCommonalities = fallWeathers;
            }
            else if (Quadrum.Septober == quadrum)
            {
                map.Biome.baseWeatherCommonalities = summerWeathers;
            }
        }
    }

    public void setDiseaseBySeason(Season season, Quadrum quadrum)
    {
        var seasonalDiseases = new List<BiomeDiseaseRecord>();
        if (Season.Spring == season && springDiseases != null)
        {
            seasonalDiseases = springDiseases;
        }
        else if (Season.Summer == season && summerDiseases != null)
        {
            seasonalDiseases = summerDiseases;
        }
        else if (Season.Fall == season && fallDiseases != null)
        {
            seasonalDiseases = fallDiseases;
        }
        else if (Season.Winter == season && winterDiseases != null)
        {
            seasonalDiseases = winterDiseases;
        }
        else
        {
            if (Quadrum.Aprimay == quadrum && springDiseases != null)
            {
                seasonalDiseases = springDiseases;
            }
            else if (Quadrum.Decembary == quadrum && winterDiseases != null)
            {
                seasonalDiseases = winterDiseases;
            }
            else if (Quadrum.Jugust == quadrum && summerDiseases != null)
            {
                seasonalDiseases = summerDiseases;
            }
            else if (Quadrum.Septober == quadrum && fallDiseases != null)
            {
                seasonalDiseases = fallDiseases;
            }
        }

        foreach (var diseaseRec in seasonalDiseases)
        {
            var disease = diseaseRec.diseaseInc;
            disease.baseChance = diseaseRec.commonality;
        }

        diseaseCacheUpdated = false;
    }

    public void setIncidentsBySeason(Season season, Quadrum quadrum)
    {
        var seasonalIncidents = new List<TKKN_IncidentCommonalityRecord>();
        if (Season.Spring == season && springEvents != null)
        {
            seasonalIncidents = springEvents;
        }
        else if (Season.Summer == season && summerEvents != null)
        {
            seasonalIncidents = summerEvents;
        }
        else if (Season.Fall == season && fallEvents != null)
        {
            seasonalIncidents = fallEvents;
        }
        else if (Season.Winter == season && winterEvents != null)
        {
            seasonalIncidents = winterEvents;
        }
        else
        {
            if (Quadrum.Aprimay == quadrum && springEvents != null)
            {
                seasonalIncidents = springEvents;
            }
            else if (Quadrum.Decembary == quadrum && winterEvents != null)
            {
                seasonalIncidents = winterEvents;
            }
            else if (Quadrum.Jugust == quadrum && summerEvents != null)
            {
                seasonalIncidents = summerEvents;
            }
            else if (Quadrum.Septober == quadrum && fallEvents != null)
            {
                seasonalIncidents = fallEvents;
            }
        }

        foreach (var incidentRate in seasonalIncidents)
        {
            var incident = incidentRate.incident;
            incident.baseChance = incidentRate.commonality;
        }
    }
}