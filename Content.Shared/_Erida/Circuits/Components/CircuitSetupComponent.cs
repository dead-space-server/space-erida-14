using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Erida.Circuits.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class CircuitSetupComponent : Component, ISerializationHooks
{
    public Container PartsContainer;

    /// <summary>
    /// Maximum number of parts in setup
    /// </summary>
    [DataField]
    public byte MaxParts = 10;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<CircuitResponseType, byte> MaxTypesOfPart = new()
    {
        {CircuitResponseType.Button, 3},
        {CircuitResponseType.Voice, 1},
    };

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public byte FallbackMaxTypesOfPart = 10;


    [DataField, ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public Dictionary<CircuitResponseType, List<NetEntity>> InsertedParts = [];

    public Dictionary<CircuitEventType, List<CircuitResponseType>> EventParts = [];

    public readonly string BaseContainerId = "circuits";

    public NetEntity? BatteryNetEnt;
}

[Serializable, NetSerializable]
public enum CircuitSetupUiKey : byte
{
    Key
}
