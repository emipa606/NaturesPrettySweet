using Verse;

namespace TKKN_NPS;

public class Comp_GraphicRotator : ThingComp
{
    public int curAngle;
    public int ticks;
    public int turnDegree;

    public CompProperties_GraphicRotator Props => (CompProperties_GraphicRotator)props;

    public override void CompTick()
    {
        ticks++;
        turnDegree = ticks % Props.howOften != 0 ? 0 : Props.howManyDegrees;
    }

    public float getCurrentAngle()
    {
        var pawn = parent as Pawn;
        if (Find.TickManager.Paused)
        {
            return curAngle;
        }

        if (pawn == null || !pawn.pather.Moving)
        {
            return -1f;
        }

        //get the direction it's moving
        if (pawn.pather.curPath == null || pawn.pather.curPath.NodesLeftCount < 1)
        {
            return -1f;
        }

        var c = pawn.pather.nextCell - pawn.Position;
        if (c.x > 0)
        {
            curAngle += turnDegree;
        }
        else if (c.x < 0)
        {
            curAngle += turnDegree;
        }
        else if (c.z > 0)
        {
            curAngle += turnDegree;
        }
        else
        {
            curAngle -= turnDegree;
        }

        if (curAngle > 360)
        {
            curAngle = 360 - curAngle;
        }
        else if (curAngle < 0)
        {
            curAngle = 360 + curAngle;
        }

        return curAngle;
    }
}