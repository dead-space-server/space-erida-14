namespace Content.Shared._ADT.Silicon;

[RegisterComponent]
public sealed partial class MobIpcComponent : Component
{
    [DataField]
    public bool DisablePointLightOnDeath = false;

    [DataField]
    public bool LightDisabledByDeath;
}
