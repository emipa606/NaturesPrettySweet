using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TKKN_NPS;

public class TKKN_SpecialHerdMigration : IncidentWorker
{
    private static readonly IntRange AnimalsCount = new(50, 70);
    public Map map;
    private BiomeSeasonalSettings mod;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        var target = parms.target;
        var map1 = (Map)target;
        if (!map1.Biome.HasModExtension<BiomeSeasonalSettings>())
        {
            return false;
        }

        mod = map1.Biome.GetModExtension<BiomeSeasonalSettings>();
        if (mod.specialHerds == null)
        {
            return false;
        }

        return TryFindAnimalKind(map1.Tile, out _) &&
               TryFindStartAndEndCells(map1, out _, out _);
    }

    private bool TryFindAnimalKind(int tile, out PawnKindDef animalKind)
    {
        var specialHerds = mod.specialHerds;
        var possibleAnimals = from k in DefDatabase<PawnKindDef>.AllDefs
            where specialHerds.Contains(k) && k.RaceProps.CanDoHerdMigration &&
                  Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)
            select k;
        if (possibleAnimals.Any())
        {
            return possibleAnimals.TryRandomElementByWeight(x => x.race.GetStatValueAbstract(StatDefOf.Wildness),
                out animalKind);
        }

        animalKind = null;
        return false;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var parmsTarget = (Map)parms.target;
        if (!TryFindAnimalKind(parmsTarget.Tile, out var pawnKindDef))
        {
            return false;
        }

        if (!TryFindStartAndEndCells(parmsTarget, out var intVec, out var near))
        {
            return false;
        }

        var rot = Rot4.FromAngleFlat((parmsTarget.Center - intVec).AngleFlat);
        var list = GenerateAnimals(pawnKindDef, parmsTarget.Tile);
        foreach (var newThing in list)
        {
            var loc = CellFinder.RandomClosewalkCellNear(intVec, parmsTarget, 10);
            GenSpawn.Spawn(newThing, loc, parmsTarget, rot);
        }

        LordMaker.MakeNewLord(null, new LordJob_ExitMapNear(near, LocomotionUrgency.Jog, 0f), parmsTarget, list);
        var text = string.Format(def.letterText, pawnKindDef.GetLabelPlural()).CapitalizeFirst();
        var label = string.Format(def.letterLabel, pawnKindDef.GetLabelPlural().CapitalizeFirst());
        Find.LetterStack.ReceiveLetter(label, text, def.letterDef, list[0]);
        return true;
    }

    private static bool TryFindStartAndEndCells(Map localMap, out IntVec3 start, out IntVec3 end)
    {
        if (!RCellFinder.TryFindRandomPawnEntryCell(out start, localMap, CellFinder.EdgeRoadChance_Animal))
        {
            end = IntVec3.Invalid;
            return false;
        }

        end = IntVec3.Invalid;
        for (var i = 0; i < 8; i++)
        {
            var startLocal = start;
            if (!CellFinder.TryFindRandomEdgeCellWith(
                    x => localMap.reachability.CanReach(startLocal, x, PathEndMode.OnCell,
                        TraverseMode.NoPassClosedDoors,
                        Danger.Deadly), localMap, CellFinder.EdgeRoadChance_Ignore, out var intVec))
            {
                break;
            }

            if (!end.IsValid || intVec.DistanceToSquared(start) > end.DistanceToSquared(start))
            {
                end = intVec;
            }
        }

        return end.IsValid;
    }

    private static List<Pawn> GenerateAnimals(PawnKindDef animalKind, int tile)
    {
        var randomInRange = AnimalsCount.RandomInRange;
        var list = new List<Pawn>();
        for (var i = 0; i < randomInRange; i++)
        {
            var request = new PawnGenerationRequest(animalKind, null, PawnGenerationContext.NonPlayer, tile, false,
                false, false, false, true, 1f, false, true, true, false);
            var item = PawnGenerator.GeneratePawn(request);
            list.Add(item);
        }

        return list;
    }
}