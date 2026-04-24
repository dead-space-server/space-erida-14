using Content.Shared.RCD.Systems;
using Content.Shared.Atmos.Components;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.RCD.Components;

/// <summary>
/// Main component for the RCD
/// Optionally uses LimitedChargesComponent.
/// Charges can be refilled with RCD ammo
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RCDSystem))]
public sealed partial class RCDComponent : Component
{
    /// <summary>
    /// List of RCD prototypes that the device comes loaded with
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<RCDPrototype>> AvailablePrototypes { get; set; } = new();

    /// <summary>
    /// Sound that plays when a RCD operation successfully completes
    /// </summary>
    [DataField]
    public SoundSpecifier SuccessSound { get; set; } = new SoundPathSpecifier("/Audio/Items/deconstruct.ogg");

    /// <summary>
    /// The ProtoId of the currently selected RCD prototype
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<RCDPrototype> ProtoId { get; set; } = "Invalid";

    // Erida start
    /// <summary>
    /// Indicates whether this device is configured as an atmospherics RPD.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsRpd { get; set; }

    /// <summary>
    /// Current layer selection mode for atmos placement.
    /// </summary>
    [DataField, AutoNetworkedField]
    public RpdMode CurrentMode { get; set; } = RpdMode.Free;

    /// <summary>
    /// Last layer selected by the client while in free placement mode.
    /// </summary>
    [DataField]
    public AtmosPipeLayer? LastSelectedLayer { get; set; }

    /// <summary>
    /// Sound played when switching RPD pipe mode.
    /// </summary>
    [DataField]
    public SoundSpecifier SoundSwitchMode { get; set; } = new SoundPathSpecifier("/Audio/Machines/quickbeep.ogg");
    // Erida end

    /// <summary>
    /// The direction constructed entities will face upon spawning
    /// </summary>
    [DataField, AutoNetworkedField]
    public Direction ConstructionDirection
    {
        get => _constructionDirection;
        set
        {
            _constructionDirection = value;
            ConstructionTransform = new Transform(new(), _constructionDirection.ToAngle());
        }
    }

    private Direction _constructionDirection = Direction.South;

    /// <summary>
    /// Returns a rotated transform based on the specified ConstructionDirection
    /// </summary>
    /// <remarks>
    /// Contains no position data
    /// </remarks>
    [ViewVariables(VVAccess.ReadOnly)]
    public Transform ConstructionTransform { get; private set; }
}

// Erida edit
[Serializable, NetSerializable]
public enum RpdMode : byte
{
    Primary,
    Secondary,
    Tertiary,
    Free,
}
