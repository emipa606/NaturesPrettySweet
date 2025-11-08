using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace TKKN_NPS;

[StaticConstructorOnStartup]
public class DustDevil : ThingWithComps
{
    private const float Wind = 1f;

    private const int CloseDamageIntervalTicks = 5;

    private const float FarDamageMTBTicks = 5f;

    private const int CloseDamageRadius = 1;

    private const int FarDamageRadius = 8;

    private const float BaseDamage = 10f;

    private const int SpawnMoteEveryTicks = 4;

    private const float DownedPawnDamageFactor = 0.01f;

    private const float AnimalPawnDamageFactor = 0.3f;

    private const float BuildingDamageFactor = 0.2f;

    private const float PlantDamageFactor = .8f;

    private const float ItemDamageFactor = 0.1f;

    private const float CellsPerSecond = 2.7f;

    private const float DirectionChangeSpeed = 0.78f;

    private const float DirectionNoiseFrequency = 0.002f;

    private const float DustDevilAnimationSpeed = 30f;

    private const float ThreeDimensionalEffectStrength = 4f;

    private const int FadeInTicks = 120;

    private const int FadeOutTicks = 120;

    private const float MaxMidOffset = 2f;

    private static readonly MaterialPropertyBlock matPropertyBlock = new();

    private static ModuleBase directionNoise;

    private static readonly IntRange DurationTicks = new(2700, 10080);

    private static readonly Material DustDevilMaterial = MaterialPool.MatFrom("Things/Ethereal/Tornado",
        ShaderDatabase.Transparent, MapMaterialRenderQueues.Tornado);

    private static readonly FloatRange PartsDistanceFromCenter = new(1f, 5f);

    private static readonly float ZOffsetBias = -4f * PartsDistanceFromCenter.min;

    private static readonly List<Thing> tmpThings = [];

    private float direction;

    private int leftFadeOutTicks = -1;
    private Vector2 realPosition;

    private int spawnTick;

    private Sustainer sustainer;

    private int ticksLeftToDisappear = -1;

    private float FadeInOutFactor
    {
        get
        {
            var a = Mathf.Clamp01((Find.TickManager.TicksGame - spawnTick) / 120f);
            var b = leftFadeOutTicks >= 0 ? Mathf.Min(leftFadeOutTicks / 120f, 1f) : 1f;
            return Mathf.Min(a, b);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref realPosition, "realPosition");
        Scribe_Values.Look(ref direction, "direction");
        Scribe_Values.Look(ref spawnTick, "spawnTick");
        Scribe_Values.Look(ref leftFadeOutTicks, "leftFadeOutTicks");
        Scribe_Values.Look(ref ticksLeftToDisappear, "ticksLeftToDisappear");
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad)
        {
            var vector = Position.ToVector3Shifted();
            realPosition = new Vector2(vector.x, vector.z);
            direction = Rand.Range(0f, 360f);
            spawnTick = Find.TickManager.TicksGame;
            leftFadeOutTicks = -1;
            ticksLeftToDisappear = DurationTicks.RandomInRange;
        }

        createSustainer();
    }

    protected override void Tick()
    {
        if (!Spawned)
        {
            return;
        }

        if (sustainer == null)
        {
            Log.Error("DustDevil sustainer is null.");
            createSustainer();
        }

        sustainer?.Maintain();
        updateSustainerVolume();
        GetComp<CompWindSource>().wind = 5f * FadeInOutFactor;
        if (leftFadeOutTicks > 0)
        {
            leftFadeOutTicks--;
            if (leftFadeOutTicks == 0)
            {
                Destroy();
            }
        }
        else
        {
            directionNoise ??= new Perlin(0.0020000000949949026, 2.0, 0.5, 4, 1948573612, QualityMode.Medium);

            direction +=
                (float)directionNoise.GetValue(Find.TickManager.TicksAbs, thingIDNumber % 500 * 1000f, 0.0) *
                0.78f;
            realPosition = realPosition.Moved(direction, 0.0283333343f);
            var intVec = new Vector3(realPosition.x, 0f, realPosition.y).ToIntVec3();
            if (intVec.InBounds(Map))
            {
                Position = intVec;
                if (this.IsHashIntervalTick(15))
                {
                    damageCloseThings();
                }

                if (Rand.MTBEventOccurs(15f, 1f, 1f))
                {
                    damageFarThings();
                }

                if (ticksLeftToDisappear > 0)
                {
                    ticksLeftToDisappear--;
                    if (ticksLeftToDisappear == 0)
                    {
                        leftFadeOutTicks = 120;
                        Messages.Message("MessageDustDevilDissipated".Translate(),
                            new TargetInfo(Position, Map), MessageTypeDefOf.PositiveEvent);
                    }
                }

                if (!this.IsHashIntervalTick(4) || cellImmuneToDamage(Position))
                {
                    return;
                }

                var num = Rand.Range(0.6f, 1f);
                FleckMaker.ThrowTornadoDustPuff(new Vector3(realPosition.x, 0f, realPosition.y)
                    {
                        y = AltitudeLayer.MoteOverhead.AltitudeFor()
                    } + Vector3Utility.RandomHorizontalOffset(1.5f), Map, Rand.Range(1.5f, 3f),
                    new Color(num, num, num));
            }
            else
            {
                leftFadeOutTicks = 120;
                Messages.Message("MessageDustDevilLeftMap".Translate(), new TargetInfo(Position, Map),
                    MessageTypeDefOf.PositiveEvent);
            }
        }
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        Rand.PushState();
        Rand.Seed = thingIDNumber;
        for (var i = 0; i < 180; i++)
        {
            drawDustDevilPart(PartsDistanceFromCenter.RandomInRange, Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f),
                Rand.Range(0.52f, 0.88f));
        }

        Rand.PopState();
    }

    private void drawDustDevilPart(float distanceFromCenter, float initialAngle, float speedMultiplier,
        float colorMultiplier)
    {
        var ticksGame = Find.TickManager.TicksGame;
        var num = 1f / distanceFromCenter;
        var num2 = 25f * speedMultiplier * num;
        var num3 = (initialAngle + (ticksGame * num2)) % 360f;
        var vector = realPosition.Moved(num3, adjustedDistanceFromCenter(distanceFromCenter));
        vector.y += distanceFromCenter * 4f;
        vector.y += ZOffsetBias;
        var vector2 = new Vector3(vector.x, AltitudeLayer.Weather.AltitudeFor() + (0.046875f * Rand.Range(0f, 1f)),
            vector.y);
        var num4 = distanceFromCenter * 3f;
        var num5 = 1f;
        if (num3 > 270f)
        {
            num5 = GenMath.LerpDouble(270f, 360f, 0f, 1f, num3);
        }
        else if (num3 > 180f)
        {
            num5 = GenMath.LerpDouble(180f, 270f, 1f, 0f, num3);
        }

        var num6 = Mathf.Min(distanceFromCenter / (PartsDistanceFromCenter.max + 2f), 1f);
        var d = Mathf.InverseLerp(0.18f, 0.4f, num6);
        var a = new Vector3(Mathf.Sin((ticksGame / 1000f) + (thingIDNumber * 10)) * 2f, 0f, 0f);
        vector2 += a * d;
        var a2 = Mathf.Max(1f - num6, 0f) * num5 * FadeInOutFactor;
        var value = new Color(colorMultiplier, colorMultiplier, colorMultiplier, a2);
        matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
        var matrix = Matrix4x4.TRS(vector2, Quaternion.Euler(0f, num3, 0f), new Vector3(num4, 1f, num4));
        Graphics.DrawMesh(MeshPool.plane10, matrix, DustDevilMaterial, 0, null, 0, matPropertyBlock);
    }

    private static float adjustedDistanceFromCenter(float distanceFromCenter)
    {
        var num = Mathf.Min(distanceFromCenter / 8f, 1f);
        num *= num;
        return distanceFromCenter * num;
    }

    private void updateSustainerVolume()
    {
        sustainer.info.volumeFactor = FadeInOutFactor;
    }

    private void createSustainer()
    {
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            var soundDef = SoundDef.Named("Tornado");
            sustainer = soundDef.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            updateSustainerVolume();
        });
    }

    private void damageCloseThings()
    {
        var num = GenRadial.NumCellsInRadius(3f);
        for (var i = 0; i < num; i++)
        {
            var intVec = Position + GenRadial.RadialPattern[i];
            if (!intVec.InBounds(Map) || cellImmuneToDamage(intVec))
            {
                continue;
            }

            var firstPawn = intVec.GetFirstPawn(Map);
            if (firstPawn is { Downed: true } && Rand.Bool)
            {
                continue;
            }

            var damageFactor = GenMath.LerpDouble(0f, 3f, 1f, 0.2f, intVec.DistanceTo(Position));
            doDamage(intVec, damageFactor);
        }
    }

    private void damageFarThings()
    {
        var c = (from x in GenRadial.RadialCellsAround(Position, 8f, true)
            where x.InBounds(Map)
            select x).RandomElement();
        if (cellImmuneToDamage(c))
        {
            return;
        }

        doDamage(c, 0.5f);
    }

    private bool cellImmuneToDamage(IntVec3 c)
    {
        if (c.Roofed(Map) && c.GetRoof(Map).isThickRoof)
        {
            return true;
        }

        var edifice = c.GetEdifice(Map);
        return edifice != null && edifice.def.category == ThingCategory.Building &&
               (edifice.def.building.isNaturalRock ||
                edifice.def == RimWorld.ThingDefOf.Wall && edifice.Faction == null);
    }

    private void doDamage(IntVec3 c, float damageFactor)
    {
        tmpThings.Clear();
        tmpThings.AddRange(c.GetThingList(Map));
        var vector = c.ToVector3Shifted();
        var b = new Vector2(vector.x, vector.z);
        var angle = -realPosition.AngleTo(b) + 180f;
        foreach (var thing in tmpThings)
        {
            BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;
            switch (thing.def.category)
            {
                case ThingCategory.Pawn:
                {
                    var pawn = (Pawn)thing;
                    battleLogEntry_DamageTaken =
                        new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Tornado);
                    Find.BattleLog.Add(battleLogEntry_DamageTaken);
                    if (pawn.RaceProps.baseHealthScale < 1f)
                    {
                        damageFactor *= pawn.RaceProps.baseHealthScale;
                    }

                    if (pawn.RaceProps.Animal)
                    {
                        damageFactor *= AnimalPawnDamageFactor;
                    }

                    if (pawn.Downed)
                    {
                        damageFactor *= DownedPawnDamageFactor;
                    }

                    break;
                }
                case ThingCategory.Item:
                    damageFactor *= ItemDamageFactor;
                    break;
                case ThingCategory.Plant:
                    damageFactor *= PlantDamageFactor;
                    break;
                case ThingCategory.Building:
                    damageFactor *= BuildingDamageFactor;
                    break;
            }

            var amount = Mathf.Max(GenMath.RoundRandom(DustDevilAnimationSpeed * damageFactor), 1);
            thing.TakeDamage(new DamageInfo(DamageDefOf.TornadoScratch, amount, 0, angle, this))
                .AssociateWithLog(battleLogEntry_DamageTaken);
        }

        tmpThings.Clear();
    }
}