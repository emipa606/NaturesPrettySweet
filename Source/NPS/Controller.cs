using Mlie;
using UnityEngine;
using Verse;

namespace TKKN_NPS;

public class Controller : Mod
{
    public static Settings settings;
    public static string currentVersion;

    public Controller(ModContentPack content)
        : base(content)
    {
        settings = GetSettings<Settings>();
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        settings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Nature's Pretty Sweet";
    }
}