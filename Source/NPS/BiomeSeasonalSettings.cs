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
        if (springWeathers == null || !springWeathers.Any() ||
            summerWeathers == null || !summerWeathers?.Any() == false ||
            fallWeathers == null || !fallWeathers?.Any() == false ||
            winterWeathers == null || !winterWeathers?.Any() == false)
        {
            return;
        }


        switch (season)
        {
            case Season.Spring:
                map.Biome.baseWeatherCommonalities = springWeathers;
                break;
            case Season.Summer:
                map.Biome.baseWeatherCommonalities = summerWeathers;
                break;
            case Season.Fall:
                map.Biome.baseWeatherCommonalities = fallWeathers;
                break;
            case Season.Winter:
                map.Biome.baseWeatherCommonalities = winterWeathers;
                break;
            default:
            {
                switch (quadrum)
                {
                    case Quadrum.Aprimay:
                        map.Biome.baseWeatherCommonalities = springWeathers;
                        break;
                    case Quadrum.Decembary:
                        map.Biome.baseWeatherCommonalities = winterWeathers;
                        break;
                    case Quadrum.Jugust:
                        map.Biome.baseWeatherCommonalities = fallWeathers;
                        break;
                    case Quadrum.Septober:
                        map.Biome.baseWeatherCommonalities = summerWeathers;
                        break;
                }

                break;
            }
        }

        if (map.Biome.baseWeatherCommonalities?.Any() == false)
        {
            map.Biome.baseWeatherCommonalities =
            [
                new WeatherCommonalityRecord { commonality = 1f, weather = WeatherDefOf.Clear }
            ];
        }
    }

    public void setDiseaseBySeason(Season season, Quadrum quadrum)
    {
        var seasonalDiseases = new List<BiomeDiseaseRecord>();
        switch (season)
        {
            case Season.Spring when springDiseases != null:
                seasonalDiseases = springDiseases;
                break;
            case Season.Summer when summerDiseases != null:
                seasonalDiseases = summerDiseases;
                break;
            case Season.Fall when fallDiseases != null:
                seasonalDiseases = fallDiseases;
                break;
            case Season.Winter when winterDiseases != null:
                seasonalDiseases = winterDiseases;
                break;
            default:
            {
                switch (quadrum)
                {
                    case Quadrum.Aprimay when springDiseases != null:
                        seasonalDiseases = springDiseases;
                        break;
                    case Quadrum.Decembary when winterDiseases != null:
                        seasonalDiseases = winterDiseases;
                        break;
                    case Quadrum.Jugust when summerDiseases != null:
                        seasonalDiseases = summerDiseases;
                        break;
                    case Quadrum.Septober when fallDiseases != null:
                        seasonalDiseases = fallDiseases;
                        break;
                }

                break;
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
        switch (season)
        {
            case Season.Spring when springEvents != null:
                seasonalIncidents = springEvents;
                break;
            case Season.Summer when summerEvents != null:
                seasonalIncidents = summerEvents;
                break;
            case Season.Fall when fallEvents != null:
                seasonalIncidents = fallEvents;
                break;
            case Season.Winter when winterEvents != null:
                seasonalIncidents = winterEvents;
                break;
            default:
            {
                switch (quadrum)
                {
                    case Quadrum.Aprimay when springEvents != null:
                        seasonalIncidents = springEvents;
                        break;
                    case Quadrum.Decembary when winterEvents != null:
                        seasonalIncidents = winterEvents;
                        break;
                    case Quadrum.Jugust when summerEvents != null:
                        seasonalIncidents = summerEvents;
                        break;
                    case Quadrum.Septober when fallEvents != null:
                        seasonalIncidents = fallEvents;
                        break;
                }

                break;
            }
        }

        foreach (var incidentRate in seasonalIncidents)
        {
            var incident = incidentRate.incident;
            incident.baseChance = incidentRate.commonality;
        }
    }
}