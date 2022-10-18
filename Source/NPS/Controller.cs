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
            VersionFromManifest.GetVersionFromModMetaData(
                ModLister.GetActiveModWithIdentifier("Mlie.NaturesPrettySweet"));
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        settings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Nature's Pretty Sweet";
    }

    // ReSharper disable once MissingXmlDoc
    public override void WriteSettings()
    {
        settings?.Write();

        if (Current.ProgramState != ProgramState.Playing)
        {
        }
    }
}