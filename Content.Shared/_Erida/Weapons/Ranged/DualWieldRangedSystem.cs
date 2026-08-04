using System.Runtime.CompilerServices;
using Content.Shared._Erida.Weapons.Ranged.MakedonShooting.Components;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;
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

        SubscribeLocalEvent<DualWieldRangedWeaponComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<DualWieldRangedWeaponComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);

        SubscribeLocalEvent<DualWieldRangedWeaponComponent, ItemWieldedEvent>(OnItemWieldedHand);
        SubscribeLocalEvent<DualWieldRangedWeaponComponent, ItemUnwieldedEvent>(OnItemUnwieldedHand);


        SubscribeLocalEvent<DualWieldRangedWeaponComponent, GunRefreshModifiersEvent>(OnGunRefreshModifiers);
    }

    private void OnShootRequest(RequestShootEvent msg, EntitySessionEventArgs args)
    {
        var gunUid = GetEntity(msg.Gun);

        if (!HasComp<DualWieldRangedWeaponComponent>(gunUid))
            return;

        var user = args.SenderSession.AttachedEntity;

        if (user == null)
            return;

        if (!TryGetWeaponsList(user.Value, out var weaponList))
            return;

        foreach (var weapon in weaponList)
        {
            if (!TryComp<GunComponent>(weapon, out var gComp))
                continue;

            _gunSystem.AttemptShoot(user.Value, (weapon, gComp), GetCoordinates(msg.Coordinates), GetEntity(msg.Target));
        }
    }

    private void OnGotEquippedHand(Entity<DualWieldRangedWeaponComponent> entity, ref GotEquippedHandEvent args)
    {
        if (!TryComp<GunComponent>(entity, out var gComp))
            return;

        AddWeaponToList(entity, args.User, gComp);
    }

    private void OnGotUnequippedHand(Entity<DualWieldRangedWeaponComponent> entity, ref GotUnequippedHandEvent args)
    {
        if (!TryComp<GunComponent>(entity, out var gComp))
            return;

        RemoveWeaponFromList(entity, args.User, gComp);
    }

    private void OnItemWieldedHand(Entity<DualWieldRangedWeaponComponent> entity, ref ItemWieldedEvent args)
    {
        AddWeaponToList(entity, args.User);
    }

    private void OnItemUnwieldedHand(Entity<DualWieldRangedWeaponComponent> entity, ref ItemUnwieldedEvent args)
    {
        if (!HasComp<GunRequiresWieldComponent>(entity))
            return;

        RemoveWeaponFromList(entity, args.User);
    }

    public bool AddWeaponToList(Entity<DualWieldRangedWeaponComponent> weapon, EntityUid user, GunComponent? gComp = null)
    {
        if (TryComp<WieldableComponent>(weapon, out var wComp)
            && HasComp<GunRequiresWieldComponent>(weapon)
            && !wComp.Wielded)
            return false;

        if (!Resolve(weapon, ref gComp))
            return false;

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(user);

        dwroComp.WeaponList.Add(weapon);
        DirtyField(user, dwroComp, nameof(DualWieldRangedOwnerComponent.WeaponList));

        if (dwroComp.NeedToUpdateOnUp)
            UpdateState(user, dwroComp);
        else if (dwroComp.DualWield)
            weapon.Comp.DualCurrent = true;

        if (dwroComp.DualWield)
            _gunSystem.RefreshModifiers((weapon, gComp));

        return true;
    }

    public bool RemoveWeaponFromList(Entity<DualWieldRangedWeaponComponent> weapon, EntityUid user, GunComponent? gComp = null)
    {
        if (!Resolve(weapon, ref gComp))
            return false;

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(user);

        weapon.Comp.DualCurrent = false;
        dwroComp.WeaponList.Remove(weapon);
        DirtyField(user, dwroComp, nameof(DualWieldRangedOwnerComponent.WeaponList));

        if (dwroComp.NeedToUpdateOnDown)
            UpdateState(user, dwroComp);

        _gunSystem.RefreshModifiers((weapon, gComp));

        return true;
    }

    public bool UpdateState(EntityUid user, DualWieldRangedOwnerComponent? dwroComp = null, HashSet<EntityUid>? weapons = null)
    {
        if (!Resolve(user, ref dwroComp))
            return false;

        weapons ??= dwroComp.WeaponList;

        UpdateDualCurrentList(weapons, dwroComp.DualWield);

        return true;
    }

    private void UpdateDualCurrentList(HashSet<EntityUid> weaponList, bool state)
    {
        foreach (var weapon in weaponList)
        {
            if (!TryComp<DualWieldRangedWeaponComponent>(weapon, out var drwComp))
                continue;

            drwComp.DualCurrent = state;
        }
    }

    public bool UpdateWeaponList(EntityUid uid, out HashSet<EntityUid>? weaponList)
    {
        weaponList = null;

        if (!TryComp<HandsComponent>(uid, out var hComp))
            return false;

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(uid);

        dwroComp.WeaponList = [];

        var itemsInHands = _handsSystem.EnumerateHeld((uid, hComp));

        foreach (var item in itemsInHands)
        {
            if (!TryComp<DualWieldRangedWeaponComponent>(item, out var dwrwComp)
                || !TryComp<GunComponent>(item, out var gComp))
                continue;

            dwrwComp.DualCurrent = true;
            _gunSystem.RefreshModifiers((item, gComp));
            dwroComp.WeaponList.Add(item);
        }

        weaponList = dwroComp.WeaponList;
        return true;
    }

    public bool TryGetWeaponsList(EntityUid uid, out HashSet<EntityUid> weaponList)
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
