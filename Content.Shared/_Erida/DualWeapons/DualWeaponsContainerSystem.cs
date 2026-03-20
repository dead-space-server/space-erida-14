using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Hands;

namespace Content.Shared.Weapons.Ranged;

public sealed class DualWeaponsContainerSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _sharedGunSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DualWeaponsBonusComponent, GotEquippedHandEvent>(OnEquipWeapon);
        SubscribeLocalEvent<DualWeaponsBonusComponent, GotUnequippedHandEvent>(OnUnequipWeapon);
    }

    private void OnEquipWeapon(EntityUid uid, DualWeaponsBonusComponent _, GotEquippedHandEvent args)
    {
        if (!TryComp<GunComponent>(args.Equipped, out var compGun)
            || !_sharedGunSystem.CheckCanShoot(args.User, (args.Equipped, compGun)))
            return;

        var compWeaponsContainer = EnsureComp<DualWeaponsContainerComponent>(args.User);

        compWeaponsContainer.gunList.Add((args.Equipped, compGun));
    }

    private void OnUnequipWeapon(EntityUid uid, DualWeaponsBonusComponent _, GotUnequippedHandEvent args)
    {
        if (!TryComp<GunComponent>(args.Unequipped, out var compGun))
            return;

        var compWeaponsContainer = EnsureComp<DualWeaponsContainerComponent>(args.User);

        compWeaponsContainer.gunList.Remove((args.Unequipped, compGun));
    }
}