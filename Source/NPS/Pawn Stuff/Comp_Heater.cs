using RimWorld;
using Verse;

namespace TKKN_NPS;

public class Comp_Heater : ThingComp
{
    public int ticks;

    public CompProperties_Heater Props => (CompProperties_Heater)props;

    public override void CompTick()
    {
        ticks++;
        if (ticks % Props.howOften != 0)
        {
            return;
        }

        GenTemperature.PushHeat(parent, Props.temperature);
        FleckMaker.ThrowFireGlow(parent.DrawPos, parent.Map, 1);
        FleckMaker.ThrowSmoke(parent.Position.ToVector3(), parent.Map, 1);
    }
}