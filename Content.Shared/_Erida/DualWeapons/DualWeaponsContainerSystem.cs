using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Wieldable;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Interaction.Events;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Shared.Weapons.Ranged;

public sealed class DualWeaponsContainerSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _sharedGunSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    public override void Initialize()
    {
        base.Initialize();

        //SubscribeLocalEvent<DualWeaponsBonusComponent, GotEquippedHandEvent>(OnEquipWeapon);
        SubscribeLocalEvent<DualWeaponsBonusComponent, GotUnequippedHandEvent>(OnUnequipWeapon);

        //SubscribeLocalEvent<DualWeaponsBonusComponent, ItemWieldedEvent>(OnUpdateWeaponInListOnWielded);
        SubscribeLocalEvent<DualWeaponsBonusComponent, ItemUnwieldedEvent>(OnUpdateWeaponInListOnUnwielded);

        SubscribeLocalEvent<DualWeaponsBonusComponent, OnEmptyGunShotEvent>(OnEmptyGunShot);
        SubscribeLocalEvent<DualWeaponsBonusComponent, ContactInteractionEvent>(OnContactInteractionEvent);
    }

    private void OnEquipWeapon(EntityUid uid, DualWeaponsBonusComponent dwbComp, GotEquippedHandEvent args)
    {
        if (!TryComp<GunComponent>(args.Equipped, out var compGun)
            || !_sharedGunSystem.CheckCanShootAndAmmo(args.User, (args.Equipped, compGun)))
            return;

        AddOrRemoveWeaponList(args.User, (args.Equipped, compGun, dwbComp), true);
    }

    private void OnUnequipWeapon(EntityUid uid, DualWeaponsBonusComponent dwbComp, GotUnequippedHandEvent args)
    {
        if (!TryComp<GunComponent>(args.Unequipped, out var compGun))
            return;

        AddOrRemoveWeaponList(args.User, (args.Unequipped, compGun, dwbComp), false);
    }

    private void OnUpdateWeaponInListOnWielded(EntityUid uid, DualWeaponsBonusComponent dwbComp, ItemWieldedEvent args)
    {
        if (!TryComp<GunComponent>(uid, out var gunComp))
            return;

        if (!HasComp<GunRequiresWieldComponent>(uid))
            return;

        AddOrRemoveWeaponList(args.User, (uid, gunComp, dwbComp), true);
    }

    private void OnUpdateWeaponInListOnUnwielded(EntityUid uid, DualWeaponsBonusComponent dwbComp, ItemUnwieldedEvent args)
    {
        if (!TryComp<GunComponent>(uid, out var gunComp))
            return;

        if (!HasComp<GunRequiresWieldComponent>(uid))
            return;

        AddOrRemoveWeaponList(args.User, (uid, gunComp, dwbComp), false);
    }
    private void OnEmptyGunShot(Entity<DualWeaponsBonusComponent> ent, ref OnEmptyGunShotEvent args)
    {
        if (!TryComp<GunComponent>(ent, out var gunComp))
            return;

        AddOrRemoveWeaponList(args.User, (ent, gunComp, ent.Comp), false);
    }

    private void OnContactInteractionEvent(Entity<DualWeaponsBonusComponent> ent, ref ContactInteractionEvent args)
    {
        if (!TryComp<GunComponent>(ent, out var gunComp))
            return;

        if (TryComp<HandsComponent>(args.Other, out var hComp))
            if (!_hands.IsHolding((args.Other, hComp), ent))
                return;

        EnsureComp<DualWeaponsContainerComponent>(args.Other);
        Logger.Debug(args.Other.ToString());

        if (HasComp<DualWeaponsContainerComponent>(args.Other))
            Logger.Debug("AAAAA");

        AddOrRemoveWeaponList(args.Other, (ent, gunComp, ent.Comp), true);
    }

    public void AddOrRemoveWeaponList(Entity<DualWeaponsContainerComponent?> ent, Entity<GunComponent?, DualWeaponsBonusComponent?> gun, bool add)
    {
        if (!Resolve(gun.Owner, ref gun.Comp1, ref gun.Comp2))
            return;

        if (!Resolve(ent.Owner, ref ent.Comp))
            ent.Comp = EnsureComp<DualWeaponsContainerComponent>(ent);

        if (add)
            ent.Comp.GunList.Add(gun.Owner);
        else
            ent.Comp.GunList.Remove(gun.Owner);

        Dirty(ent, ent.Comp);
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
                weaponsList.Add(heldItem.Value);
            }
        }
    }
}
