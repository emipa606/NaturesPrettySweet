using RimWorld;

namespace TKKN_NPS;

public class GameCondition_Drought : GameCondition
{
    public readonly FloodType floodOverride = FloodType.Low;
    public int tempAdjust = 10;
}