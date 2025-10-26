
namespace Content.Shared._Erida.Chemistry.Components;

[RegisterComponent]
public sealed partial class MedicalToleranceComponent : Component
{

    [ViewVariables]

    public Dictionary<string, float> Tolerances { get; set; } = new();


    private const float ToleranceDecay = 0.01f;

}
