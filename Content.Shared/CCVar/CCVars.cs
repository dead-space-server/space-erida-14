using Content.Shared.Administration;
using Content.Shared.CCVar.CVarAccess;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

/// <summary>
/// Contains all the CVars used by content.
/// </summary>
/// <remarks>
/// NOTICE FOR FORKS: Put your own CVars in a separate file with a different [CVarDefs] attribute. RT will automatically pick up on it.
/// </remarks>
[CVarDefs]
public sealed partial class CCVars : CVars
{
    // Only debug stuff lives here.

#if DEBUG
    [CVarControl(AdminFlags.Debug)]
    public static readonly CVarDef<string> DebugTestCVar =
        CVarDef.Create("debug.test_cvar", "default", CVar.SERVER);

    [CVarControl(AdminFlags.Debug)]
    public static readonly CVarDef<float> DebugTestCVar2 =
        CVarDef.Create("debug.test_cvar2", 123.42069f, CVar.SERVER);
#endif

    /// <summary>
    /// A simple toggle to test <c>OptionsVisualizerComponent</c>.
    /// </summary>
    public static readonly CVarDef<bool> DebugOptionVisualizerTest =
        CVarDef.Create("debug.option_visualizer_test", false, CVar.CLIENTONLY);

    /// <summary>
    /// Set to true to disable parallel processing in the pow3r solver.
    /// </summary>
    public static readonly CVarDef<bool> DebugPow3rDisableParallel =
        CVarDef.Create("debug.pow3r_disable_parallel", false, CVar.SERVERONLY);

    /*
    * Blob
    */
    public static readonly CVarDef<int> BlobMax =
        CVarDef.Create("blob.max", 3, CVar.SERVERONLY);

    public static readonly CVarDef<int> BlobPlayersPer =
        CVarDef.Create("blob.players_per", 20, CVar.SERVERONLY);

    public static readonly CVarDef<bool> BlobCanGrowInSpace =
         CVarDef.Create("blob.grow_space", false, CVar.SERVER);

    // <summary>
    // The speed multiplier threshold beyond which it will decrease
    // <summary>
    public static readonly CVarDef<float> SpeedModifierThreshold =
         CVarDef.Create("speed.modifier_threshold", 1.5f, CVar.SERVER);

    // <summary>
    // The range of numbers before the threshold at which the numbers grow too much
    // <summary>
    public static readonly CVarDef<float> SpeedModifierThresholdSaveZone =
         CVarDef.Create("speed.modifier_threshold_safe_zone", 0.3f, CVar.SERVER);

    // <summary>
    // The smaller the value, the faster the modifier will decrease after the threshold.
    // <summary>
    public static readonly CVarDef<int> SpeedModifierThresholdStrength =
         CVarDef.Create("speed.modifier_threshold_strength", -4, CVar.SERVER);
}
