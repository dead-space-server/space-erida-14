using Robust.Shared.GameStates;
namespace Content.Shared.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class DualWeaponsContainerComponent : Component
{
    public HashSet<Entity<GunComponent>> gunList = new();
}