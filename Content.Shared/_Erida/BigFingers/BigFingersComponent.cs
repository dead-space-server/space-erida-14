namespace Content.Shared._Erida.BigFingers.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class BigFingersComponent : Component
{
    public bool ByClothes = false;

    [DataField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(3.0);

    [DataField]
    [AutoPausedField]
    public TimeSpan? NextPopupTime = null;
}
