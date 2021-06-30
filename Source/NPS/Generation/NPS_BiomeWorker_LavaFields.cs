using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TKKN_NPS
{
    internal class NPS_BiomeWorker_LavaFields : BiomeWorker_TropicalRainforest
    {
        public override float GetScore(Tile tile, int id)
        {
            if (!(base.GetScore(tile, id) > 0))
            {
                return 0f;
            }

            if (Rand.Value > .009)
            {
                return 0f;
            }

            return (float) (32.0 + ((tile.temperature - 20.0) * 3.5) + ((tile.rainfall - 600.0) / 165.0));
        }
    }
}