using UnityEngine;
using Verse;

namespace TKKN_NPS;

[StaticConstructorOnStartup]
public class WeatherOverlay_LavaSmoke : WeatherOverlayDualPanner
{
    private static readonly Material material = new(MatLoader.LoadMat("Weather/FogOverlayWorld"));

    public WeatherOverlay_LavaSmoke()
    {
        worldOverlayMat = material;
        ForcedOverlayColor = new Color(0.64f, 0.35f, 0.26f);

        worldOverlayPanSpeed1 = 0.0003f;
        worldOverlayPanSpeed2 = 0.0001f;
        worldPanDir1 = new Vector2(1f, 1f);
        worldPanDir2 = new Vector2(1f, -1f);
    }
}