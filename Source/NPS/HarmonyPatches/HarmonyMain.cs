using System.Reflection;
using HarmonyLib;
using Verse;

namespace TKKN_NPS;

[StaticConstructorOnStartup]
internal class HarmonyMain
{
    public static readonly bool RimBrellasActive;
    public static readonly MethodInfo HasUmbrella;

    static HarmonyMain()
    {
        var harmony = new Harmony("com.github.tkkntkkn.Natures-Pretty-Sweet");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        RimBrellasActive = ModLister.GetActiveModWithIdentifier("battlemage64.Rimbrellas") != null;

        if (!RimBrellasActive)
        {
            return;
        }

        HasUmbrella = AccessTools.Method("Umbrellas.UmbrellaDefMethods:HasUmbrella");
        if (HasUmbrella != null)
        {
            return;
        }

        Log.Warning(
            "[Natures Pretty Sweet]: Rimbrella loaded but could not find the correct method to check for umbrellas");
        RimBrellasActive = false;
    }
}