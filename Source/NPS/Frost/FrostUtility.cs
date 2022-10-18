using Verse;

namespace TKKN_NPS;

internal class FrostUtility
{
    public static FrostCategory GetFrostCategory(float FrostDepth)
    {
        switch (FrostDepth)
        {
            case < 0.1f:
                return FrostCategory.None;
            case < 0.3f:
                return FrostCategory.Frost;
            case < 0.5f:
                return FrostCategory.Thin;
            case < 0.7f:
                return FrostCategory.Medium;
            default:
                return FrostCategory.Thick;
        }
    }

    public static string GetDescription(FrostCategory category)
    {
        switch (category)
        {
            case FrostCategory.None:
                return "FrostNone".Translate();
            case FrostCategory.Dusting:
                return "FrostDusting".Translate();
            case FrostCategory.Thin:
                return "FrostThin".Translate();
            case FrostCategory.Medium:
                return "FrostMedium".Translate();
            case FrostCategory.Thick:
                return "FrostThick".Translate();
            default:
                return "Frost";
        }
    }

    public static int MovementTicksAddOn(FrostCategory category)
    {
        switch (category)
        {
            case FrostCategory.None:
                return 0;
            case FrostCategory.Dusting:
                return 0;
            case FrostCategory.Thin:
                return 0;
            case FrostCategory.Medium:
                return 1;
            case FrostCategory.Thick:
                return 2;
            default:
                return 0;
        }
    }
}