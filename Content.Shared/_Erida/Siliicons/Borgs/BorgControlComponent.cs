namespace Content.Shared.Silicons.Borgs.Components;

[RegisterComponent]
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
}
