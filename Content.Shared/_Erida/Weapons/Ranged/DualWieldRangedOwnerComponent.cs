using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Erida.Weapons.Ranged.MakedonShooting.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true, true)]
public sealed partial class DualWieldRangedOwnerComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<Entity<GunComponent>> WeaponList = [];
}
