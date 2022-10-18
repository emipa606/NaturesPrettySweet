using UnityEngine;
using Verse;

namespace TKKN_NPS;

[StaticConstructorOnStartup]
public class WeatherOverlay_ThickFog : SkyOverlay
{
    public static readonly Material FogOverlayWorld = new Material(MatLoader.LoadMat("Weather/FogOverlayWorld"));

    public WeatherOverlay_ThickFog()
    {
        worldOverlayMat = FogOverlayWorld;

        worldOverlayPanSpeed1 = 0.0003f;
        worldOverlayPanSpeed2 = 0.0001f;
        worldPanDir1 = new Vector2(1f, 1f);
        worldPanDir2 = new Vector2(1f, -1f);
    }
}