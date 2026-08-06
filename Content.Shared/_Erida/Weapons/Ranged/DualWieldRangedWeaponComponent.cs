using Robust.Shared.GameStates;

namespace Content.Shared._Erida.Weapons.Ranged.MakedonShooting.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true, true)]
public sealed partial class DualWieldRangedWeaponComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("minAngle"), AutoNetworkedField]
    public Angle MinAngle = Angle.FromDegrees(45);

    [ViewVariables(VVAccess.ReadWrite), DataField("maxAngle"), AutoNetworkedField]
    public Angle MaxAngle = Angle.FromDegrees(45);

    [DataField, AutoNetworkedField]
    public Angle AngleDecay = Angle.FromDegrees(0);

    [DataField, AutoNetworkedField]
    public Angle AngleIncrease = Angle.FromDegrees(5);

    [AutoNetworkedField, DataField]
    public bool DualCurrent = false;
}
