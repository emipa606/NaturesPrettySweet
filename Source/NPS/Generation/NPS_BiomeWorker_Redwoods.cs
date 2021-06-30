using RimWorld;
using RimWorld.Planet;

namespace TKKN_NPS
{
    internal class NPS_BiomeWorker_Redwoods : BiomeWorker_BorealForest
    {
        public override float GetScore(Tile tile, int id)
        {
            if (tile.WaterCovered)
            {
                return -100f;
            }

            if (tile.temperature < -10f || tile.temperature > 10f)
            {
                return 0f;
            }

            if (tile.rainfall < 1100f)
            {
                return 0f;
            }

            return 40f;
        }
    }
}