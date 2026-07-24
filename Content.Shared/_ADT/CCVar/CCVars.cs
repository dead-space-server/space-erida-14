using Robust.Shared.Configuration;

namespace Content.Shared._ADT.CCVar;

[CVarDefs]
public sealed class SimpleStationCCVars
{
    #region Silicons
    public static readonly CVarDef<float> SiliconNpcUpdateTime =
        CVarDef.Create("silicon.npcupdatetime", 1.5f, CVar.SERVERONLY);
    #endregion

    #region Jetpack System
    public static readonly CVarDef<bool> JetpackEnableAnywhere =
        CVarDef.Create("jetpack.enable_anywhere", false, CVar.REPLICATED);

    public static readonly CVarDef<bool> JetpackEnableInNoGravity =
        CVarDef.Create("jetpack.enable_in_no_gravity", true, CVar.REPLICATED);
    #endregion

    public static readonly CVarDef<int> MaxTraitCount =
        CVarDef.Create("ic.traits.max_count", 10, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<int> MaxTraitPoints =
        CVarDef.Create("ic.traits.max_points", 0, CVar.SERVER | CVar.REPLICATED);
}
