using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Tools;

namespace Content.Shared._Erida.Tools.WelderHeating;


[RegisterComponent, NetworkedComponent]
public sealed partial class WelderHeatingComponent : Component
{

    /// <summary>
    /// Defines how much heat can apply to solution once
    /// </summary>
    [DataField]
    public float HeatPerUse = 1000f;

    /// <summary>
    /// Threshold of heat to which welder can heat
    /// </summary>
    [DataField]
    public float HeatThreshold = 10000f;

    /// <summary>
    /// How long it takes to heat container
    /// </summary>
    [DataField]
    public float HeatDuration = 3f;
}
