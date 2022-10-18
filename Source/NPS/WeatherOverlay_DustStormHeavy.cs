using UnityEngine;
using Verse;

namespace TKKN_NPS;

[StaticConstructorOnStartup]
public class WeatherOverlay_DustStormHeavy : SkyOverlay
{
    private static readonly Material material;

    static WeatherOverlay_DustStormHeavy()
    {
        material = new Material(MatLoader.LoadMat("Weather/SnowOverlayWorld"));
    }

    public WeatherOverlay_DustStormHeavy()
    {
        worldOverlayMat = material;

        OverlayColor = new Color(0.57f, 0.34f, 0.10f);

        worldOverlayPanSpeed1 = 0.018f;
        worldPanDir1 = new Vector2(-1f, -0.26f);
        worldPanDir1.Normalize();

        worldOverlayPanSpeed2 = 0.022f;
        worldPanDir2 = new Vector2(-1f, -0.24f);
        worldPanDir2.Normalize();
    }
}