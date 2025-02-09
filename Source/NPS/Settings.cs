using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TKKN_NPS;

public class Settings : ModSettings
{
    public static bool leaveStuff = true;
    public static Dictionary<string, IntVec3> wetCells;

    public static Dictionary<string, int> totalThings = new Dictionary<string, int>
    {
        { "TKKN_Lava_Spring", 0 },
        { "TKKN_ColdSpring", 0 },
        { "TKKN_HotSpring", 0 }
    };

    public static Dictionary<string, int> totalSpecialThings = new Dictionary<string, int>
    {
        { "TKKN_Total_Special_Elements", 0 },
        { "TKKN_Total_Discovered_Elements", 0 },
        { "TKKN_Total_Removed_Elements", 0 }
    };

    public static readonly Dictionary<string, Texture2D> customWeathers;

    public static bool spawnLavaOnlyInBiome = true;
    public static bool allowLavaEruption = true;
    public static bool allowPlantEffects = true;
    public static bool showCold = true;
    public static bool showHot = true;
    public static bool allowPawnsToGetWet = true;
    public static bool allowPawnsSwim = true;
    public static bool showRain = true;
    public static bool doWeather = true;
    public static bool doDirtPath = true;
    public static bool regenCells;
    public static bool doTides = true;
    public static bool showDevReadout;

    public static bool showUpdateNotes = true;
    public static bool doFloods = true;
    public static float weatherCellUpdateSpeed = 0.0006f;
    public static bool doIce = showCold;
    public static bool doSprings = true;

    static Settings()
    {
    }


    public static bool showTempOverlay => showCold && showHot;

    public void DoWindowContents(Rect inRect)
    {
        var list = new Listing_Standard(GameFont.Small) { ColumnWidth = inRect.width / 2 };
        list.Begin(inRect);

        //Performance Settings
        list.CheckboxLabeled(
            "TKKN_doWeather_title".Translate(),
            ref doWeather,
            "TKKN_doWeather_text".Translate());
        if (doWeather)
        {
            weatherCellUpdateSpeed = list.SliderLabeled(
                "TKKN_weatherCellUpdateSpeed_title".Translate(weatherCellUpdateSpeed * 10000),
                weatherCellUpdateSpeed, 0.0001f, 0.002f, 0.5f, "TKKN_weatherCellUpdateSpeed_text".Translate());
        }

        list.CheckboxLabeled(
            "TKKN_showHot_title".Translate(),
            ref showHot,
            "TKKN_showHot_text".Translate());
        list.CheckboxLabeled(
            "TKKN_showCold_title".Translate(),
            ref showCold,
            "TKKN_showCold_text".Translate());
        if (showCold)
        {
            list.CheckboxLabeled(
                "TKKN_doIce_title".Translate(),
                ref doIce,
                "TKKN_doIce_text".Translate());
        }

        list.CheckboxLabeled(
            "TKKN_showRain_title".Translate(),
            ref showRain,
            "TKKN_showRain_text".Translate());
        list.CheckboxLabeled(
            "TKKN_doTides_title".Translate(),
            ref doTides,
            "TKKN_doTides_text".Translate());
        list.CheckboxLabeled(
            "TKKN_doFloods_title".Translate(),
            ref doFloods,
            "TKKN_doFloods_text".Translate());
        list.CheckboxLabeled(
            "TKKN_leaveStuff_title".Translate(),
            ref leaveStuff,
            "TKKN_leaveStuff_text".Translate());

        list.CheckboxLabeled(
            "TKKN_doSprings_title".Translate(),
            ref doSprings,
            "TKKN_doSprings_text".Translate());
        list.Gap();
        list.CheckboxLabeled(
            "TKKN_doDirtPath_title".Translate(),
            ref doDirtPath,
            "TKKN_doDirtPath_text".Translate());
        list.Gap();


        //Game Play Settings


        list.CheckboxLabeled(
            "TKKN_allowLavaEruption_title".Translate(),
            ref allowLavaEruption,
            "TKKN_allowLavaEruption_text".Translate());
        list.CheckboxLabeled(
            "TKKN_spawnLavaOnlyInBiome_title".Translate(),
            ref spawnLavaOnlyInBiome,
            "TKKN_spawnLavaOnlyInBiome_text".Translate());
        list.CheckboxLabeled(
            "TKKN_allowPlantEffects_title".Translate(),
            ref allowPlantEffects,
            "TKKN_allowPlantEffects_text".Translate());
        list.CheckboxLabeled(
            "TKKN_allowPawnsToGetWet_title".Translate(),
            ref allowPawnsToGetWet,
            "TKKN_allowPawnsToGetWet_text".Translate());
        list.CheckboxLabeled(
            "TKKN_allowPawnsSwim_title".Translate(),
            ref allowPawnsSwim,
            "TKKN_allowPawnsToSwim_text".Translate());


        //Development stuff
        list.Gap(30f);

        list.CheckboxLabeled(
            "Show Update Notes?",
            ref showUpdateNotes,
            "");
        list.Gap(30f);
        list.CheckboxLabeled(
            "TKKN_regen_title".Translate(),
            ref regenCells,
            "TKKN_regen_text".Translate());

        list.CheckboxLabeled(
            "TKKN_showTempReadout_title".Translate(),
            ref showDevReadout,
            "TKKN_showTempReadout_text".Translate());

        if (Controller.currentVersion != null)
        {
            list.Gap();
            GUI.contentColor = Color.gray;
            list.Label("TKKN_CurrentModVersion_text".Translate(Controller.currentVersion));
            GUI.contentColor = Color.white;
        }

        list.End();
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref doDirtPath, "doDirtPath", true, true);
        Scribe_Values.Look(ref showHot, "showHot", true, true);
        Scribe_Values.Look(ref showCold, "showCold", true, true);
        Scribe_Values.Look(ref showHot, "allowPlantEffects", true, true);
        Scribe_Values.Look(ref showRain, "showRain", true, true);
        Scribe_Values.Look(ref doTides, "doTides", true, true);
        Scribe_Values.Look(ref doFloods, "doFloods", true, true);
        Scribe_Values.Look(ref leaveStuff, "leaveStuff", true, true);
        Scribe_Values.Look(ref doSprings, "doSprings", true, true);
        Scribe_Values.Look(ref doIce, "doIce", showCold, true);
        Scribe_Values.Look(ref allowPawnsToGetWet, "allowPawnsToGetWet", true, true);
        Scribe_Values.Look(ref allowPawnsSwim, "allowPawnsSwim", true, true);
        Scribe_Values.Look(ref showDevReadout, "showDevReadout", false, true);
        Scribe_Values.Look(ref spawnLavaOnlyInBiome, "spawnLavaOnlyInBiome", false, true);
        Scribe_Values.Look(ref allowLavaEruption, "allowLavaEruption", true, true);
        Scribe_Values.Look(ref regenCells, "regenCells", false, true);
    }
}