using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._Erida.Circuits.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class CircuitComponentComponent : Component
{
    /// <summary>
    /// Logic that will be executed upon activation
    /// </summary>
    [DataField(required: true), AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public CircuitResponseType AnswerType;

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public CircuitPortData[] Inputs = [];

    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public CircuitPortData[] Output = [];

    [DataField]
    public float PowerConsuming = 10f;

    [DataField, AutoNetworkedField]
    public CircuitEventType? EventType;

    [DataField]
    public byte NeedSignalsForActivate = 1;
    public Vector2 PositionInSetup = new(0, 0);
    public NetEntity? NetContainer = null;
}
