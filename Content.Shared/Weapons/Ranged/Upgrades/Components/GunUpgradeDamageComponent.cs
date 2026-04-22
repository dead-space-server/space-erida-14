using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// A <see cref="GunUpgradeComponent"/> for increasing the damage of a gun's projectile.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GunUpgradeDamageComponent : Component
{
    // Goobstation start
    /// <summary>
    /// How much should we multiply the total projectile's damage.
    /// </summary>
    [DataField]
    public float Modifier = 1f;
    // Goobstation end

    /// <summary>
    /// Additional damage added onto the projectile's base damage.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new();
}
