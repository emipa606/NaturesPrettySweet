using Verse;
using Verse.Noise;

namespace TKKN_NPS;

public class LavaComp : SpringComp
{
    public override void fillBorder()
    {
    }

    protected override void changeShape()
    {
        ModuleBase moduleBase = new Perlin(1.1, 2, 0.5, 2, Rand.Range(0, Props.radius), QualityMode.Medium);
        moduleBase = new ScaleBias(0.2, 0.2, moduleBase);

        ModuleBase moduleBase2 = new DistFromAxis(new FloatRange(0, Props.radius).RandomInRange);
        moduleBase2 = new ScaleBias(.2, .2, moduleBase2);
        moduleBase2 = new Clamp(0, 1, moduleBase2);

        terrainNoise = new Add(moduleBase, moduleBase2);
    }

    public override bool doBorder(IntVec3 c)
    {
        var currentTerrain = c.GetTerrain(parent.Map);
        return currentTerrain != Props.wetTile;
    }
}