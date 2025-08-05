using Verse;

namespace TKKN_NPS;

public abstract class SpringCompAbstract : ThingComp
{
    protected bool specialFX;
    public abstract void specialCellAffects(IntVec3 c);
    protected abstract void springTerrain(IntVec3 c);
    public abstract bool doBorder(IntVec3 c);

    public abstract void fillBorder();

    protected virtual void specialFXAffect(IntVec3 c)
    {
        springTerrain(c);
        var FX = specialFX;
        specialFX = false;
        if (!FX)
        {
        }
    }
}