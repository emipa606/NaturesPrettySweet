using Verse;

namespace TKKN_NPS;

public class Comp_GraphicRotator : ThingComp
{
    private int curAngle;
    private int ticks;
    private int turnDegree;

    private CompProperties_GraphicRotator Props => (CompProperties_GraphicRotator)props;

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
        if (c.x > 0 || c.x < 0 || c.z > 0)
        {
            curAngle += turnDegree;
        }
        else
        {
            curAngle -= turnDegree;
        }

        curAngle = curAngle switch
        {
            > 360 => 360 - curAngle, < 0 => 360 + curAngle, _ => curAngle
        };

        return curAngle;
    }
}