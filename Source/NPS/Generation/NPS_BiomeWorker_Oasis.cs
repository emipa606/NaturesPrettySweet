using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TKKN_NPS
{
    internal class NPS_BiomeWorker_Oasis : BiomeWorker_Desert
    {
        public override float GetScore(Tile tile, int id)
        {
            if (!(base.GetScore(tile, id) > 0))
            {
                return 0f;
            }

            if (Rand.Value > .006)
            {
                return 0f;
            }

            return tile.temperature + 20;
        }
    }
}