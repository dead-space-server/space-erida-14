using Content.Shared._Erida.Weapons.Ranged.MakedonShooting.Components;
using Content.Shared.Alert;
using Content.Shared.Examine;
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
    [Dependency] private AlertsSystem _alertsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<RequestShootEvent>(OnShootRequest);

        SubscribeLocalEvent<DualWieldRangedWeaponComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<DualWieldRangedWeaponComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);

        SubscribeLocalEvent<DualWieldRangedWeaponComponent, ItemWieldedEvent>(OnItemWieldedHand);
        SubscribeLocalEvent<DualWieldRangedWeaponComponent, ItemUnwieldedEvent>(OnItemUnwieldedHand);

        SubscribeLocalEvent<DualWieldRangedWeaponComponent, GunRefreshModifiersEvent>(OnGunRefreshModifiers);
        SubscribeLocalEvent<DualWieldRangedWeaponComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<DualWieldRangedOwnerComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<DualWieldRangedOwnerComponent, ComponentRemove>(OnCompRemoved);
        SubscribeLocalEvent<DualWieldRangedOwnerComponent, ToggleDualWieldEvent>(OnToggleDualWield);
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

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(args.User);

        AddWeaponToListWithCheck(entity, args.User, gComp, dwroComp);
    }

    private void OnGotUnequippedHand(Entity<DualWieldRangedWeaponComponent> entity, ref GotUnequippedHandEvent args)
    {
        if (!TryComp<GunComponent>(entity, out var gComp))
            return;

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(args.User);

        RemoveWeaponFromListWithCheck(entity, args.User, gComp, dwroComp);
    }

    private void OnItemWieldedHand(Entity<DualWieldRangedWeaponComponent> entity, ref ItemWieldedEvent args)
    {
        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(args.User);

        AddWeaponToListWithCheck(entity, args.User, null, dwroComp);
    }

    private void OnItemUnwieldedHand(Entity<DualWieldRangedWeaponComponent> entity, ref ItemUnwieldedEvent args)
    {
        if (!HasComp<GunRequiresWieldComponent>(entity))
            return;

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(args.User);

        RemoveWeaponFromListWithCheck(entity, args.User, null, dwroComp);
    }

    public bool AddWeaponToListWithCheck(Entity<DualWieldRangedWeaponComponent> weapon, EntityUid user, GunComponent? gComp = null, DualWieldRangedOwnerComponent? dwroComp = null)
    {
        if (!Resolve(user, ref dwroComp))
            return false;

        if (!dwroComp.DualWieldEnabled)
            return false;

        return AddWeaponToList(weapon, (user, dwroComp), gComp);
    }

    public bool AddWeaponToList(Entity<DualWieldRangedWeaponComponent> weapon, Entity<DualWieldRangedOwnerComponent> user, GunComponent? gComp = null)
    {
        if (!Resolve(weapon, ref gComp))
            return false;

        if (TryComp<WieldableComponent>(weapon, out var wComp)
            && HasComp<GunRequiresWieldComponent>(weapon)
            && !wComp.Wielded)
            return false;

        user.Comp.WeaponList.Add(weapon);
        DirtyField(user, user.Comp, nameof(DualWieldRangedOwnerComponent.WeaponList));

        if (user.Comp.NeedToUpdateOnUp)
            UpdateStateOfList(user, user.Comp);
        else if (user.Comp.DualWield)
            weapon.Comp.DualCurrent = true;

        if (user.Comp.DualWield)
            _gunSystem.RefreshModifiers((weapon, gComp));

        return true;
    }

    public bool RemoveWeaponFromListWithCheck(Entity<DualWieldRangedWeaponComponent> weapon, EntityUid user, GunComponent? gComp = null, DualWieldRangedOwnerComponent? dwroComp = null)
    {
        if (!Resolve(user, ref dwroComp))
            return false;

        if (!dwroComp.DualWieldEnabled)
            return false;

        return RemoveWeaponFromList(weapon, (user, dwroComp), gComp);
    }

    public bool RemoveWeaponFromList(Entity<DualWieldRangedWeaponComponent> weapon, Entity<DualWieldRangedOwnerComponent> user, GunComponent? gComp = null)
    {
        if (!Resolve(weapon, ref gComp))
            return false;

        weapon.Comp.DualCurrent = false;
        user.Comp.WeaponList.Remove(weapon);
        DirtyField(user, user.Comp, nameof(DualWieldRangedOwnerComponent.WeaponList));

        if (user.Comp.NeedToUpdateOnDown)
            UpdateStateOfList(user, user.Comp);

        _gunSystem.RefreshModifiers((weapon, gComp));

        return true;
    }

    public bool UpdateStateOfList(EntityUid user, DualWieldRangedOwnerComponent? dwroComp = null, HashSet<EntityUid>? weapons = null)
    {
        if (!Resolve(user, ref dwroComp))
            return false;

        weapons ??= dwroComp.WeaponList;

        UpdateDualCurrentList(weapons, dwroComp.DualWield && dwroComp.DualWieldEnabled);

        return true;
    }

    private void UpdateDualCurrentList(HashSet<EntityUid> weaponList, bool state)
    {
        foreach (var weapon in weaponList)
        {
            if (!TryComp<DualWieldRangedWeaponComponent>(weapon, out var drwComp)
                || !TryComp<GunComponent>(weapon, out var gComp))
                continue;

            drwComp.DualCurrent = state;
            _gunSystem.RefreshModifiers((weapon, gComp));
        }
    }

    public bool UpdateWeaponList(EntityUid uid)
    {
        if (!TryComp<HandsComponent>(uid, out var hComp))
            return false;

        var dwroComp = EnsureComp<DualWieldRangedOwnerComponent>(uid);

        if (dwroComp.WeaponList.Count > 0)
            UpdateDualCurrentList(dwroComp.WeaponList, false);

        dwroComp.WeaponList = [];

        if (!dwroComp.DualWieldEnabled)
        {
            DirtyField(uid, dwroComp, nameof(DualWieldRangedOwnerComponent.WeaponList));
            return true;
        }

        var itemsInHands = _handsSystem.EnumerateHeld((uid, hComp));

        foreach (var item in itemsInHands)
        {
            if (!TryComp<DualWieldRangedWeaponComponent>(item, out var dwrwComp)
                || !TryComp<GunComponent>(item, out var gComp))
                continue;

            AddWeaponToList((item, dwrwComp), (uid, dwroComp), gComp);
        }

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

    private void OnCompInit(Entity<DualWieldRangedOwnerComponent> entity, ref ComponentInit args)
    {
        _alertsSystem.ShowAlert(entity.Owner, entity.Comp.Alert, entity.Comp.DualWieldEnabled ? (short)1 : (short)0);
    }

    private void OnCompRemoved(Entity<DualWieldRangedOwnerComponent> entity, ref ComponentRemove args)
    {
        _alertsSystem.ClearAlert(entity.Owner, entity.Comp.Alert);
    }
    private void OnToggleDualWield(Entity<DualWieldRangedOwnerComponent> ent, ref ToggleDualWieldEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.DualWieldEnabled = !ent.Comp.DualWieldEnabled;
        _alertsSystem.ShowAlert(ent.Owner, ent.Comp.Alert, (short)(ent.Comp.DualWieldEnabled ? 1 : 0));
        DirtyField(ent.AsNullable(), nameof(ent.Comp.DualWieldEnabled), null);
        UpdateWeaponList(ent);

        args.Handled = true;
    }

    private void OnExamine(Entity<DualWieldRangedWeaponComponent> entity, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        using (args.PushGroup(nameof(DualWieldRangedWeaponComponent)))
        {
            if (entity.Comp.MinAngle == entity.Comp.MaxAngle)
            {
                args.PushMarkup(Loc.GetString("dualwield-angle-increase-examine",
                    ("angle", $"{entity.Comp.MinAngle.Degrees:0}")));
                return;
            }

            args.PushMarkup(Loc.GetString("dualwield-angle-increase-extended-examine",
                    ("minAngle", $"{entity.Comp.MinAngle.Degrees:0}"), ("maxAngle", $"{entity.Comp.MaxAngle.Degrees:0}")));
        }
    }
}
