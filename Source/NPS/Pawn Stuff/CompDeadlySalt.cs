using RimWorld;
using Verse;

namespace TKKN_NPS;

public class CompDeadlySalt : ThingComp
{
    private bool Active {
        get
        {
            Pawn pawn = parent as Pawn;
            if (pawn == null) return false;
            if (!pawn.Spawned) return false;
            return true;
        }
    }

    private CompProperties_DeadlySalt Props => (CompProperties_DeadlySalt)props;

    public override void CompTick() 
    {
        if (parent.IsHashIntervalTick(120) && Active ) {
            if (parent is Pawn pawn) {
                var terrain = pawn.Position.GetTerrain(pawn.MapHeld);
                if (Props.deadlyTerrain.Contains(terrain)) {
                    BurnSnails(pawn);
                }
            }
        }
    }

    private void BurnSnails(Pawn pawn)
    {
        var battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire);
        Find.BattleLog.Add(battleLogEntry_DamageTaken);
        var dinfo = new DamageInfo(DamageDefOf.Flame, 100, -1f, 0, null, null, null,
            DamageInfo.SourceCategory.ThingOrUnknown, pawn);
        dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
        pawn.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);
    }
}