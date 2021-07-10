using System.Linq;
using RimWorld;
using Verse;

namespace TKKN_NPS
{
    public class GameCondition_TKKNBlooms : GameCondition
    {
        public float howManyBlooms;

        public void DoCellSteadyEffects(IntVec3 c)
        {
            //must be outdoors.
            var unused = SingleMap;
            var biomeSettings = SingleMap.Biome.GetModExtension<BiomeSeasonalSettings>();
            var bloomPlants = biomeSettings.bloomPlants.ToList();
            if (bloomPlants.Count == 0)
            {
                return;
            }

            var room = c.GetRoom(SingleMap);
            if (room != null)
            {
                return;
            }


            var terrain = c.GetTerrain(SingleMap);
            if (terrain.fertility == 0)
            {
                return;
            }

            if (!c.Roofed(SingleMap) && c.GetEdifice(SingleMap) == null && c.GetCover(SingleMap) == null)
            {
                var thingList = c.GetThingList(SingleMap);
                var planted = false;
                foreach (var thing in thingList)
                {
                    if (thing is not Plant)
                    {
                        continue;
                    }

                    if (!(Rand.Value < 0.0065f))
                    {
                        continue;
                    }

                    var plant = (Plant) ThingMaker.MakeThing(thing.def);
                    if (plant.def.plant.LimitedLifespan &&
                        plant.def.statBases.GetStatOffsetFromList(StatDefOf.Beauty) > 3 &&
                        plant.def.ingestible.foodType != FoodTypeFlags.Tree)
                    {
                        plant.Growth = 1;
                    }

                    planted = true;
                }

                if (planted)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (!(Rand.Value < 0.65f * SingleMap.fertilityGrid.FertilityAt(c) * howManyBlooms))
            {
                return;
            }

            var source = from def in bloomPlants
                where def.CanEverPlantAt(c, SingleMap)
                select def;
            if (!source.Any())
            {
                return;
            }

            var thingDef = source.RandomElement();
            var makePlant = (Plant) ThingMaker.MakeThing(thingDef);
            makePlant.Growth = 1;
            GenSpawn.Spawn(makePlant, c, SingleMap);
        }
    }
}