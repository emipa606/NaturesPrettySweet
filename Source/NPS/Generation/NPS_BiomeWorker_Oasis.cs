using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TKKN_NPS;

internal class NPS_BiomeWorker_Oasis : BiomeWorker_Desert
{
    public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
    {
        if (!(base.GetScore(biome, tile, planetTile) > 0))
        {
            return 0f;
        }

        if (Rand.ValueSeeded(tile.tile.tileId * 3) > .006)
        {
            return 0f;
        }

        return tile.temperature + 20;
    }
}