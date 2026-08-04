using System.Runtime.CompilerServices;
using Content.Shared._Erida.Weapons.Ranged.MakedonShooting.Components;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Shared._Erida.Weapons.Ranged.MakedonShooting;

public sealed partial class DualWieldRangedSystem : EntitySystem
{
    [Dependency] private SharedGunSystem _gunSystem = default!;
    [Dependency] private SharedHandsSystem _handsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<RequestShootEvent>(OnShootRequest);
        SubscribeAllEvent<RequestStopShootEvent>(OnStopShootRequest);

        SubscribeLocalEvent<DualWieldRangedWeaponComponent, EquippedHandEvent>(OnEquippedHand);
        SubscribeLocalEvent<DualWieldRangedWeaponComponent, UnequippedHandEvent>(OnUnequippedHand);

        SubscribeLocalEvent<DualWieldRangedWeaponComponent, ItemUnwieldedEvent>(OnItemUnwieldedHand);
        SubscribeLocalEvent<DualWieldRangedWeaponComponent, ItemWieldedEvent>(OnItemWieldedHand);

        SubscribeLocalEvent<DualWieldRangedWeaponComponent, GunRefreshModifiersEvent>(OnGunRefreshModifiers);
    }

    private void OnShootRequest(RequestShootEvent msg, EntitySessionEventArgs args)
    {
        var user = args.SenderSession.AttachedEntity;

        if (user == null)
            return;

        var gunUid = GetEntity(msg.Gun);

        if (!HasComp<DualWieldRangedWeaponComponent>(gunUid))
            return;

        if (!TryGetWeaponsList(user.Value, out var weaponList))
            return;

        foreach (var weapon in weaponList)
        {
            _gunSystem.AttemptShoot(user.Value, weapon, GetCoordinates(msg.Coordinates), GetEntity(msg.Target));
        }
    }

    private void OnStopShootRequest(RequestStopShootEvent msg, EntitySessionEventArgs args)
    {
        var user = args.SenderSession.AttachedEntity;

        if (user == null)
            return;

        if (!TryGetWeaponsList(user.Value, out var weaponList))
            return;

        foreach (var weapon in weaponList)
        {
            _gunSystem.StopShooting(weapon);
        }
    }

    private void OnEquippedHand(Entity<DualWieldRangedWeaponComponent> entity, ref EquippedHandEvent args)
    {
        if (!TryComp<GunComponent>(entity, out var gComp))
            return;

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(args.User);

        entity.Comp.DualCurrent = true;
        dwroComp.WeaponList.Add((entity.Owner, gComp));
        _gunSystem.RefreshModifiers((entity.Owner, gComp));
    }

    private void OnUnequippedHand(Entity<DualWieldRangedWeaponComponent> entity, ref UnequippedHandEvent args)
    {
        if (!TryComp<GunComponent>(entity, out var gComp))
            return;

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(args.User);

        entity.Comp.DualCurrent = false;
        dwroComp.WeaponList.Remove((entity.Owner, gComp));
        _gunSystem.RefreshModifiers((entity.Owner, gComp));
    }

    private void OnItemUnwieldedHand(Entity<DualWieldRangedWeaponComponent> entity, ref ItemUnwieldedEvent args)
    {
        if (!HasComp<GunRequiresWieldComponent>(entity))
            return;

        RemoveWeaponFromList(entity, args.User);
    }

    private void OnItemWieldedHand(Entity<DualWieldRangedWeaponComponent> entity, ref ItemWieldedEvent args)
    {
        if (!HasComp<GunRequiresWieldComponent>(entity))
            return;

        AddWeaponToList(entity, args.User);
    }

    public bool UpdateWeaponList(EntityUid uid, out HashSet<Entity<GunComponent>>? weaponList)
    {
        weaponList = null;

        if (!TryComp<HandsComponent>(uid, out var hComp))
            return false;

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(uid);

        dwroComp.WeaponList = [];

        var itemsInHands = _handsSystem.EnumerateHeld((uid, hComp));

        var activeItem = _handsSystem.GetActiveItem(uid);

        foreach (var item in itemsInHands)
        {
            if (!TryComp<DualWieldRangedWeaponComponent>(item, out var dwrwComp)
                || !TryComp<GunComponent>(item, out var gComp))
                continue;

            dwrwComp.DualCurrent = true;
            _gunSystem.RefreshModifiers((item, gComp));
            dwroComp.WeaponList.Add((item, gComp));
        }

        weaponList = dwroComp.WeaponList;
        return true;
    }

    public bool AddWeaponToList(Entity<DualWieldRangedWeaponComponent> weapon, EntityUid user, GunComponent? gComp = null)
    {
        if (!Resolve(weapon, ref gComp))
            return false;

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(user);

        weapon.Comp.DualCurrent = true;
        dwroComp.WeaponList.Add((user, gComp));
        _gunSystem.RefreshModifiers((weapon, gComp));
        return true;
    }

    public bool RemoveWeaponFromList(Entity<DualWieldRangedWeaponComponent> weapon, EntityUid user, GunComponent? gComp = null)
    {
        if (!Resolve(weapon, ref gComp))
            return false;

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(user);

        weapon.Comp.DualCurrent = false;
        dwroComp.WeaponList.Remove((user, gComp));
        _gunSystem.RefreshModifiers((weapon, gComp));
        return true;
    }

    public bool TryGetWeaponsList(EntityUid uid, out HashSet<Entity<GunComponent>> weaponList)
    {
        if (TryComp<DualWieldRangedOwnerComponent>(uid, out var msoComp)
            && msoComp.WeaponList != null)
        {
            weaponList = msoComp.WeaponList;
            return true;
        }

        weaponList = default!;
        return false;
    }

    private void OnGunRefreshModifiers(Entity<DualWieldRangedWeaponComponent> bonus, ref GunRefreshModifiersEvent args)
    {
        if (bonus.Comp.DualCurrent)
        {
            args.MinAngle += bonus.Comp.MinAngle;
            args.MaxAngle += bonus.Comp.MaxAngle;
            args.AngleDecay += bonus.Comp.AngleDecay;
            args.AngleIncrease += bonus.Comp.AngleIncrease;
        }
    }
}
