using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.Borgs.Components;

[RegisterComponent, NetworkedComponent] // Erida edit - Station AI borg control
public sealed partial class BorgControlComponent : Component
{
    /// <summary>
    /// The AI entity that is temporarily visiting this borg.
    /// </summary>
    public EntityUid? OriginalAi;

    /// <summary>
    /// Temporary action shown while a Station AI is controlling this borg.
    /// </summary>
    public EntityUid? ReturnToAiAction;

    /// <summary>
    /// Original access enabled state before the Station AI temporarily took control.
    /// </summary>
    public bool? OriginalAccessEnabled;
}
