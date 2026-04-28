namespace Content.Server._Erida.LightIntension.Components;

[RegisterComponent]
public sealed partial class LightIntensionComponent : Component
{
    [DataField]
    public float MaxLightCap = 1f;
}
