using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace TKKN_NPS;

internal class PatchWeatherDecider
{
    [HarmonyPatch(typeof(WeatherDecider))]
    [HarmonyPatch("CurrentWeatherCommonality")]
    public static class PatchCurrentWeatherCommonality
    {
        [HarmonyPrefix]
        public static bool Prefix(WeatherDef weather, ref float __result, Map ___map, int ___ticksWhenRainAllowedAgain)
        {
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

                if (weather.rainRate > 0.1f)
                {
                    if (___map.gameConditionManager.ActiveConditions.Any(x => x.def.preventRain))
                    {
                        __result = 0f;
                        return false;
                    }
                }

                var biome = ___map.Biome;
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < biome.baseWeatherCommonalities.Count; i++)
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
                Log.Warning($"Failed to find the commonality of {weather}, skipping.\n{exception}");
                __result = 0f;
                return false;
            }

            __result = 0f;
            return false;
        }
    }
}