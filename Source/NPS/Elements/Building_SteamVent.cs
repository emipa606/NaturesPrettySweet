using RimWorld;
using Verse;
using Verse.Sound;

namespace TKKN_NPS;

internal class Building_SteamVent : Building
{
    private readonly Building harvester = null;

    private Sustainer spraySustainer;

    private int spraySustainerStartTick = -999;
    private IntermittentSteamSprayer steamSprayer;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        steamSprayer = new IntermittentSteamSprayer(this)
        {
            startSprayCallback = StartSpray, endSprayCallback = EndSpray
        };
    }

    private void StartSpray()
    {
        SnowUtility.AddSnowRadial(this.OccupiedRect().RandomCell, Map, 4f, -0.06f);
        spraySustainer = SoundDefOf.GeyserSpray.TrySpawnSustainer(new TargetInfo(Position, Map));
        spraySustainerStartTick = Find.TickManager.TicksGame;
    }

    private void EndSpray()
    {
        if (spraySustainer == null)
        {
            return;
        }

        spraySustainer.End();
        spraySustainer = null;
    }

    public override void Tick()
    {
        if (harvester == null)
        {
            steamSprayer.SteamSprayerTick();
        }

        if (spraySustainer != null && Find.TickManager.TicksGame > spraySustainerStartTick + 1000)
        {
            Log.Message("Geyser spray sustainer still playing after 1000 ticks. Force-ending.");
            spraySustainer.End();
            spraySustainer = null;
        }


        if (Rand.Value < .000001f)
        {
            DeSpawn();
        }
    }
}