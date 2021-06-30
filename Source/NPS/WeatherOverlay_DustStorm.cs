using UnityEngine;
using Verse;

namespace TKKN_NPS
{
    [StaticConstructorOnStartup]
    public class WeatherOverlay_DustStorm : SkyOverlay
    {
        public static readonly Material material = new Material(MatLoader.LoadMat("Weather/FogOverlayWorld"));

        public WeatherOverlay_DustStorm()
        {
            worldOverlayMat = material;
            OverlayColor = new Color(0.57f, 0.34f, 0.10f);

            worldOverlayPanSpeed1 = 0.0003f;
            worldOverlayPanSpeed2 = 0.0001f;
            worldPanDir1 = new Vector2(1f, 1f);
            worldPanDir2 = new Vector2(1f, -1f);
        }
    }
}