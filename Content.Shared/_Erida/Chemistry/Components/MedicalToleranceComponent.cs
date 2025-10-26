using Content.Shared.Chemistry.Reagent;

namespace Content.Shared._Erida.Chemistry.Components;

[RegisterComponent]
public sealed partial class MedicalToleranceComponent : Component
{

    [ViewVariables]
    public Dictionary<ReagentId, float> Tolerances { get; set; } = new();
}