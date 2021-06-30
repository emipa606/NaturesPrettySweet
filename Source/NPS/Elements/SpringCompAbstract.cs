using Verse;

namespace TKKN_NPS
{
    public abstract class SpringCompAbstract : ThingComp
    {
        public bool specialFX;
        public abstract void specialCellAffects(IntVec3 c);
        public abstract void springTerrain(IntVec3 c);
        public abstract bool doBorder(IntVec3 c);

        public abstract void fillBorder();

        public virtual void specialFXAffect(IntVec3 c)
        {
            springTerrain(c);
            var FX = specialFX;
            specialFX = false;
            if (!FX)
            {
            }
        }
    }
}