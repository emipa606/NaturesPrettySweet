using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(WeatherDecider), "CurrentWeatherCommonality")]
public static class WeatherDecider_CurrentWeatherCommonality
{
    public static bool Prefix(WeatherDef weather, ref float __result, Map ___map, int ___ticksWhenRainAllowedAgain)
    {
        if (___map == null || ___map.weatherManager == null || ___map.gameConditionManager == null ||
            ___map.Biome == null || ___map.mapTemperature == null || weather == null)
        {
            __result = 0f;
            return false;
        }

        try
        {
            if (___map.weatherManager.curWeather is { repeatable: false } &&
                weather == ___map.weatherManager.curWeather)
            {
                __result = 0f;
                return false;
            }

            if (!weather.temperatureRange.Includes(___map.mapTemperature.OutdoorTemp))
            {
                __result = 0f;
                return false;
            }

            if (weather.favorability < Favorability.Neutral && GenDate.DaysPassedSinceSettle < 8)
            {
                __result = 0f;
                return false;
            }

            if (weather.rainRate > 0.1f && Find.TickManager.TicksGame < ___ticksWhenRainAllowedAgain)
            {
                __result = 0f;
                return false;
            }

            if (weather.rainRate > 0.1f &&
                ___map.gameConditionManager.ActiveConditions?.Any(x => x.def.preventRain) == true)
            {
                __result = 0f;
                return false;
            }

            if (ModsConfig.AnomalyActive && weather.minMonolithLevel > Find.Anomaly.HighestLevelReached)
            {
                __result = 0f;
                return false;
            }

            var biome = ___map.Biome;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < biome.baseWeatherCommonalities?.Count; i++)
            {
                var weatherCommonalityRecord = biome.baseWeatherCommonalities[i];
                if (weatherCommonalityRecord.weather != weather)
                {
                    continue;
                }

                var num = weatherCommonalityRecord.commonality;
                if (___map.fireWatcher.LargeFireDangerPresent && weather.rainRate > 0.1f)
                {
                    num *= 15f;
                }

                if (weatherCommonalityRecord.weather.commonalityRainfallFactor != null)
                {
                    num *= weatherCommonalityRecord.weather.commonalityRainfallFactor.Evaluate(___map.TileInfo
                        .rainfall);
                }

                __result = num;
                return false;
            }
        }
        catch (Exception exception)
        {
            Log.WarningOnce($"Failed to find the commonality of {weather}, skipping.\n{exception}",
                weather.GetHashCode());
            __result = 0f;
            return false;
        }

        __result = 0f;
        return false;
    }
}