using RimWorld;
using Verse;

namespace TKKN_NPS;

public class CompDeadlySalt : ThingComp
{
    private bool Active => parent is Pawn { Spawned: true };

    private CompProperties_DeadlySalt Props => (CompProperties_DeadlySalt)props;

    public override void CompTick()
    {
        if (!parent.IsHashIntervalTick(120) || !Active)
        {
            return;
        }

        if (parent is not Pawn pawn)
        {
            return;
        }

        var terrain = pawn.Position.GetTerrain(pawn.MapHeld);
        if (Props.deadlyTerrain.Contains(terrain))
        {
            burnSnails(pawn);
        }
    }

    private static void burnSnails(Pawn pawn)
    {
        var battleLogEntryDamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire);
        Find.BattleLog.Add(battleLogEntryDamageTaken);
        var damageInfo = new DamageInfo(DamageDefOf.Flame, 100, -1f, 0, null, null, null,
            DamageInfo.SourceCategory.ThingOrUnknown, pawn);
        damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
        pawn.TakeDamage(damageInfo).AssociateWithLog(battleLogEntryDamageTaken);
    }
}