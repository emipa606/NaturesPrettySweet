using HarmonyLib;
using UnityEngine;
using Verse;

namespace TKKN_NPS;

[HarmonyPatch(typeof(MouseoverReadout))]
[HarmonyPatch("MouseoverReadoutOnGUI")]
internal class PatchMouseoverReadout
{
    private static void Postfix()
    {
        var c = UI.MouseCell();
        var map = Find.CurrentMap;
        if (!c.InBounds(map))
        {
            return;
        }

        Rect rect;
        var BotLeft = new Vector2(15f, 65f);
        var num = 38f;
        var zone = c.GetZone(map);
        if (zone != null)
        {
            num += 19f;
        }

        var depth = map.snowGrid.GetDepth(c);
        if (depth > 0.03f)
        {
            num += 19f;
        }

        var thingList = c.GetThingList(map);
        foreach (var thing in thingList)
        {
            if (thing.def.category != ThingCategory.Mote)
            {
                num += 19f;
            }
        }

        var roof = c.GetRoof(map);
        if (roof != null)
        {
            num += 19f;
        }

        if (Settings.showDevReadout)
        {
            rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
            var label3 = $"C: x-{c.x} y-{c.y} z-{c.z}";
            Widgets.Label(rect, label3);
            num += 19f;

            var watcher = map.GetComponent<Watcher>();

            if (watcher.cellWeatherAffects.TryGetValue(c, out var cell))
            {
                rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
                var label2 = $"Temperature: {cell.temperature}";
                Widgets.Label(rect, label2);
                num += 19f;

                rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
                var label4 =
                    $"Cell Info: Base Terrain {cell.baseTerrain.defName} Current Terrain {cell.currentTerrain.defName} | Wet {cell.isWet} | Melt {cell.isMelt} | Flooded {cell.isFlooded} | Frozen {cell.isFrozen} | Thawed {cell.isThawed} | Getting Wet? {cell.gettingWet}";
                Widgets.Label(rect, label4);
                num += 19f;

                rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
                var label6 =
                    $"TKKN_Wet {cell.currentTerrain.HasTag("TKKN_Wet")}TKKN_Swim {cell.currentTerrain.HasTag("TKKN_Swim")}";
                Widgets.Label(rect, label6);
                num += 19f;


                rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
                var label5 =
                    $"Cell Info: howWet {cell.howWet} | How Wet (Plants) {cell.howWetPlants} | How Packed {cell.howPacked}";
                if (cell.weather != null)
                {
                    if (cell.weather.wetTerrain != null)
                    {
                        label5 += $" | T Wet {cell.weather.wetTerrain.defName}";
                    }

                    if (cell.weather.dryTerrain != null)
                    {
                        label5 += $" | T Dry {cell.weather.dryTerrain.defName}";
                    }

                    if (cell.weather.freezeTerrain != null)
                    {
                        label5 += $" | T Freeze {cell.weather.freezeTerrain.defName}";
                    }
                }

                if (cell.originalTerrain != null)
                {
                    label5 += $" | Orig Terrain {cell.originalTerrain.defName}";
                }

                Widgets.Label(rect, label5);
            }

            num += 19f;
        }


        depth = map.GetComponent<FrostGrid>().GetDepth(c);
        if (!(depth > 0.01f))
        {
            return;
        }

        rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
        var frostCategory = FrostUtility.GetFrostCategory(depth);
        var label = FrostUtility.GetDescription(frostCategory);
        Widgets.Label(rect, label);
        //	Widgets.Label(rect, unused + " " + depth.ToString());
    }
}