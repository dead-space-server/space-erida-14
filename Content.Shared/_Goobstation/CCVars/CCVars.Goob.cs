using Robust.Shared.Configuration;

namespace Content.Shared._Goobstation.CCVar;

[CVarDefs]
public sealed partial class GoobCVars
{
    /// <summary>
    /// Controls how often GPS updates.
    /// </summary>
    public static readonly CVarDef<float> GpsUpdateRate =
        CVarDef.Create("gps.update_rate", 1f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// How long until the next EORG popup can be shown after previous one.
    /// </summary>
    public static readonly CVarDef<int> AskRoundEndNoEorgPopup =
        CVarDef.Create("game.ask_read_end_eorg_popup", 14, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Set the last shown of EORG popup to client current time.
    /// </summary>
    public static readonly CVarDef<string> LastReadRoundEndNoEorgPopup =
        CVarDef.Create("game.last_read_end_eorg_popup_time", "", CVar.CLIENTONLY | CVar.ARCHIVE);

}
