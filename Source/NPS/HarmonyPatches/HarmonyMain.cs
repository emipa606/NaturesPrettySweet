using System.Reflection;
using HarmonyLib;
using Verse;

namespace TKKN_NPS
{
    [StaticConstructorOnStartup]
    internal class HarmonyMain
    {
        static HarmonyMain()
        {
            var harmony = new Harmony("com.github.tkkntkkn.Natures-Pretty-Sweet");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}