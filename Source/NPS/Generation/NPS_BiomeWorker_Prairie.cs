using RimWorld;
using RimWorld.Planet;

namespace TKKN_NPS;

internal class NPS_BiomeWorker_Prairie : BiomeWorker_AridShrubland
{
    public override float GetScore(Tile tile, int id)
    {
        //keep this the same, just make it fail more often. When it fails, shrubland will be rendered, instead.
        /*
        if (tile.WaterCovered)
        {
            return -100f;
        }
        if (tile.temperature < -10f)
        {
            return 0f;
        }
        if ((tile.rainfall < 600f) || tile.rainfall >= 2000f)
        {
            return 0f;
        }

        return 22.5f + (tile.temperature - 22f) * 2.2f + (tile.rainfall - 600f) / 100f;
        */
        if (tile.WaterCovered)
        {
            return -100f;
        }

        if (tile.temperature is < -10f or > 22)
        {
            return 0f;
        }

        if (tile.rainfall is < 900f or >= 1300f)
        {
            return 0f;
        }

        if (tile.hilliness != Hilliness.Flat)
        {
            return 0f;
        }

        return 22.5f + ((tile.temperature - 20f) * 6.2f) + ((tile.rainfall - 0f) / 100f);
    }
}