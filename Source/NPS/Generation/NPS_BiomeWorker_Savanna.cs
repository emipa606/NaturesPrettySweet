using RimWorld;
using RimWorld.Planet;

namespace TKKN_NPS;

internal class NPS_BiomeWorker_Savanna : BiomeWorker_TemperateForest
{
    public override float GetScore(Tile tile, int id)
    {
        //keep this the same, just make it fail more often. When it fails, shrubland will be rendered, instead.
        if (tile.WaterCovered)
        {
            return -100f;
        }

        if (tile.temperature is < 24f or > 30f)
        {
            return 0f;
        }

        if (tile.rainfall is < 1400f or >= 2000f)
        {
            return 0f;
        }

        return 22.5f + (tile.temperature - 7f) + 30 + ((tile.rainfall - 0f) / 180f);
    }
}