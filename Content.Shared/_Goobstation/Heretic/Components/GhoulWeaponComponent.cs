using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class GhoulWeaponComponent : Component
{
    [DataField]
    public LocId ExamineMessage = "ghoul-weapon-comp-examine";
}
