using Content.Shared._Erida.Circuits.Components;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Erida.Circuits;

#region Data types
[Serializable, NetSerializable]
public enum CircuitEventType
{
    Verb
};

[Serializable, NetSerializable]
public enum CircuitDataFormat
{
    Boolean,
    Number,
    ProtoId,
    String,
    Signal
}

[DataDefinition]
[Serializable, NetSerializable]
public partial struct CircuitData
{
    [DataField]
    public bool Signal = false;

    [DataField]
    public bool[] Boolean = [];

    [DataField]
    public int[] Number = [];

    [DataField]
    public string[] String = [];
}


/// <summary>
/// Stores information about connected port and itself data type.
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public partial struct CircuitPortData
{
    /// <summary>
    /// Data type, which port is supporting
    /// </summary>
    [DataField]
    public CircuitDataFormat DataType;

    [DataField]
    public bool IsChangeable = false;

    /// <summary>
    /// The component to which the port is connected
    /// </summary>
    [DataField]
    public NetEntity? ConnectedComponent;

    /// <summary>
    /// The index of the connected port in another component
    /// </summary>
    [DataField]
    public byte? ConnectedIndex;

    [DataField]
    public CircuitData? Data;

    [DataField]
    public bool ShouldDataDeleted = true;

    [DataField]
    public TimeSpan DataInvalidAt = TimeSpan.Zero;

    [DataField]
    public TimeSpan DataLifeTime = TimeSpan.FromSeconds(2);

    [DataField]
    public bool IsOutput = false;
}

[Serializable, NetSerializable]
public enum CircuitResponseType
{
    Button,
    Voice,
    Test,
}


[DataDefinition]
[Serializable, NetSerializable]
public partial struct CircuiNetLinkData
{
    public NetEntity OutputComponent;
    public byte OutputPortIndex;
    public NetEntity InputComponent;
    public byte InputPortIndex;
}

public partial struct CircuitLinkData
{
    public Entity<CircuitComponentComponent> OutputComponent;
    public byte OutputPortIndex;
    public Entity<CircuitComponentComponent> InputComponent;
    public byte InputPortIndex;
}
#endregion
#region Interface
[Serializable, NetSerializable]
public sealed class CircuitDeleteComponentMessage : BoundUserInterfaceMessage
{
    public NetEntity CompEntity;
}

[Serializable, NetSerializable]
public sealed class CircuitCreateLinkMessage : BoundUserInterfaceMessage
{
    public CircuiNetLinkData LinkData;
}

[Serializable, NetSerializable]
public sealed class CircuitDeleteLinkMessage : BoundUserInterfaceMessage
{
    public CircuitPortData PortData;
}


[Serializable, NetSerializable]
public sealed class CircuitSetupBoundUserInterfaceState(bool updateNodes = true, float? charge = null) : BoundUserInterfaceState
{
    public bool UpdateNodes = updateNodes;
    public float? Charge = charge;
}
#endregion


#region Events
public sealed class CircuitComponentActivated() : EntityEventArgs
{
    public Entity<CircuitComponentComponent>? Component = null;
}
#endregion
