using Robust.Shared.Serialization;
using Content.Shared.Inventory;

namespace Content.Shared.VoiceMask;

[Serializable, NetSerializable]
public enum VoiceMaskUIKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class VoiceMaskBuiState : BoundUserInterfaceState
{
    public readonly string Name;
    public readonly string? Verb;
    public readonly string Voice; // Corvax-TTS
    public readonly bool Active;
    public readonly bool AccentHide;

    public VoiceMaskBuiState(string name, string voice, string? verb, bool active, bool accentHide)
    {
        Name = name;
        Verb = verb;
        Active = active;
        AccentHide = accentHide;
        Voice = voice;
    }
}

[Serializable, NetSerializable]
public sealed class VoiceMaskChangeNameMessage : BoundUserInterfaceMessage
{
    public readonly string Name;

    public VoiceMaskChangeNameMessage(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Change the speech verb prototype to override, or null to use the user's verb.
/// </summary>
[Serializable, NetSerializable]
public sealed class VoiceMaskChangeVerbMessage : BoundUserInterfaceMessage
{
    public readonly string? Verb;

    public VoiceMaskChangeVerbMessage(string? verb)
    {
        Verb = verb;
    }
}

/// <summary>
///     Toggle the effects of the voice mask.
/// </summary>
[Serializable, NetSerializable]
public sealed class VoiceMaskToggleMessage : BoundUserInterfaceMessage;

/// <summary>
///     Toggle the effects of accent negation.
/// </summary>
[Serializable, NetSerializable]
public sealed class VoiceMaskAccentToggleMessage : BoundUserInterfaceMessage;

/// <summary>
/// Fired when a voice mask is toggled.
/// </summary>
public sealed class VoiceMaskToggledEvent(EntityUid mask, EntityUid source, bool active) : IInventoryRelayEvent
{
    public EntityUid Mask = mask;
    public EntityUid Source = source;
    public bool Active = active;

    SlotFlags IInventoryRelayEvent.TargetSlots => SlotFlags.WITHOUT_POCKET;
}
