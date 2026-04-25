using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Wieldable;
using Content.Shared.Power;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared.Weapons.Ranged;

public sealed class DualWeaponsContainerSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _sharedGunSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DualWeaponsBonusComponent, GotEquippedHandEvent>(OnEquipWeapon);
        SubscribeLocalEvent<DualWeaponsBonusComponent, GotUnequippedHandEvent>(OnUnequipWeapon);
        SubscribeLocalEvent<DualWeaponsBonusComponent, UpdateWeaponInListEvent>(OnUpdateWeaponInList);
        SubscribeLocalEvent<DualWeaponsContainerComponent, UpdateFullWeaponsListEvent>(OnUpdateFullWeaponsList);

        SubscribeLocalEvent<DualWeaponsBonusComponent, OnEmptyGunShotEvent>(OnUpdateWeaponInListOnEmptyGun);

        SubscribeLocalEvent<DualWeaponsBonusComponent, ItemWieldedEvent>(OnUpdateWeaponInListOnWielded);
        SubscribeLocalEvent<DualWeaponsBonusComponent, ItemUnwieldedEvent>(OnUpdateWeaponInListOnUnwielded);
    }

    private void OnEquipWeapon(EntityUid uid, DualWeaponsBonusComponent _, GotEquippedHandEvent args)
    {
        if (!TryComp<GunComponent>(args.Equipped, out var compGun)
            || !_sharedGunSystem.CheckCanShootAndAmmo(args.User, (args.Equipped, compGun)))
            return;

        UpdateWeaponInList(args.User, (args.Equipped, compGun));
    }

    private void OnUnequipWeapon(EntityUid uid, DualWeaponsBonusComponent _, GotUnequippedHandEvent args)
    {
        if (!TryComp<GunComponent>(args.Unequipped, out var compGun))
            return;

        UpdateWeaponInList(args.User, (args.Unequipped, compGun));
    }

    private void OnUpdateWeaponInList(EntityUid uid, DualWeaponsBonusComponent comp, UpdateWeaponInListEvent args)
    {
        UpdateWeaponInList(args.User, args.Gun);
    }

    private void OnUpdateFullWeaponsList(EntityUid uid, DualWeaponsContainerComponent comp, UpdateFullWeaponsListEvent args)
    {
        UpdateFullList(args.User);
    }

    private void OnUpdateWeaponInListOnEmptyGun(EntityUid uid, DualWeaponsBonusComponent comp, OnEmptyGunShotEvent args)
    {
        if (!TryComp<GunComponent>(uid, out var gunComp))
            return;
        UpdateWeaponInList(args.User, (uid, gunComp));
    }

    private void OnUpdateWeaponInListOnWielded(EntityUid uid, DualWeaponsBonusComponent comp, ItemWieldedEvent args)
    {
        if (!TryComp<GunComponent>(uid, out var gunComp)
            || gunComp == null)
            return;
        UpdateWeaponInList(args.User, (uid, gunComp));
    }

    private void OnUpdateWeaponInListOnUnwielded(EntityUid uid, DualWeaponsBonusComponent comp, ItemUnwieldedEvent args)
    {
        if (!TryComp<GunComponent>(uid, out var gunComp)
            || gunComp == null)
            return;
        UpdateWeaponInList(args.User, (uid, gunComp));
    }
    public void UpdateWeaponInList(EntityUid uid, Entity<GunComponent> gun)
    {
        if (!HasComp<DualWeaponsBonusComponent>(gun))
            return;

        var compWeaponsContainer = EnsureComp<DualWeaponsContainerComponent>(uid);
        var weaponsList = compWeaponsContainer.GunList;

        Logger.Debug($"item: {uid.Id}, {Transform(gun).ParentUid.Id}");

        if (_sharedGunSystem.CheckCanShootAndAmmo(uid, gun)
            && Transform(gun).ParentUid == uid)
            weaponsList.Add(gun);
        else
            weaponsList.Remove(gun);
    }
    public void UpdateFullList(EntityUid uid)
    {
        if (!TryComp<HandsComponent>(uid, out var handsComp))
            return;

        var compWeaponsContainer = EnsureComp<DualWeaponsContainerComponent>(uid);
        compWeaponsContainer.GunList.Clear();
        var weaponsList = compWeaponsContainer.GunList;

        foreach (var (handString, handClass) in handsComp.Hands)
        {
            if (!_hands.TryGetHeldItem(uid, handString, out var heldItem))
                continue;
            if (TryComp<GunComponent>(heldItem.Value, out var gunComp)
                && HasComp<DualWeaponsContainerComponent>(heldItem.Value)
                && _sharedGunSystem.CheckCanShootAndAmmo(uid, (heldItem.Value, gunComp)))
            {
                weaponsList.Add((heldItem.Value, gunComp));
            }
        }
    }

    public void CheckItemInHand()
    {

    }
}

[ByRefEvent]
public struct UpdateWeaponInListEvent
{
    public EntityUid User;
    public Entity<GunComponent> Gun;
}

[ByRefEvent]
public struct UpdateFullWeaponsListEvent
{
    public EntityUid User;
}
