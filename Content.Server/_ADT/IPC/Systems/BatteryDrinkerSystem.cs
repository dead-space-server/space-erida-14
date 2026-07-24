// SPDX-FileCopyrightText: 2026 ADT Development
// SPDX-FileCopyrightText: 2026 OpenWendor
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Content.Server._ADT.Silicon.BatterySlot;
using Content.Server._ADT.Silicon.Charge;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Shared._ADT.Silicon;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Power.Components;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Server._ADT.Power;

public sealed partial class BatteryDrinkerSystem : EntitySystem
{
    [Dependency] private BatterySystem _battery = default!;
    [Dependency] private SiliconChargeSystem _silicon = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, GetVerbsEvent<AlternativeVerb>>(AddAltVerb);
        SubscribeLocalEvent<BatteryDrinkerComponent, BatteryDrinkerDoAfterEvent>(OnDoAfter);
    }

    private void AddAltVerb(EntityUid uid, BatteryComponent batteryComponent, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<BatteryDrinkerComponent>(args.User, out var drinkerComp) ||
            !TestDrinkableBattery(uid, drinkerComp) ||
            !TryGetFillableBattery(args.User, out var drinkerBattery, out _))
            return;

        AlternativeVerb verb = new()
        {
            Act = () => DrinkBattery(uid, args.User, drinkerComp),
            Text = Loc.GetString("battery-drinker-verb-drink"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/smite.svg.192dpi.png")),
        };

        args.Verbs.Add(verb);
    }

    private bool TestDrinkableBattery(EntityUid target, BatteryDrinkerComponent drinkerComp)
    {
        if (!drinkerComp.DrinkAll && !HasComp<BatteryDrinkerSourceComponent>(target))
            return false;

        return true;
    }

    private bool TryGetIPCBattery(EntityUid uid,
        [NotNullWhen(true)] out BatteryComponent? battery,
        [NotNullWhen(true)] out EntityUid batteryUid)
    {
        battery = null;
        batteryUid = default;

        if (!TryComp<BatterySlotRequiresLockComponent>(uid, out var slotComp))
            return false;

        if (!TryComp<ContainerManagerComponent>(uid, out var containerManager))
            return false;

        if (!containerManager.Containers.TryGetValue(slotComp.ItemSlot, out var container))
            return false;

        if (container.ContainedEntities.Count == 0)
            return false;

        var cellUid = container.ContainedEntities[0];

        if (!TryComp(cellUid, out battery))
            return false;

        batteryUid = cellUid;
        return true;
    }

    private bool TryGetFillableBattery(EntityUid uid,
        [NotNullWhen(true)] out BatteryComponent? battery,
        [NotNullWhen(true)] out EntityUid batteryUid)
    {
        if (TryGetIPCBattery(uid, out battery, out batteryUid))
            return true;

        if (_silicon.TryGetSiliconBattery(uid, out battery, out batteryUid))
            return true;

        if (TryComp(uid, out battery))
        {
            batteryUid = uid;
            return true;
        }

        batteryUid = default;
        return false;
    }

    private void DrinkBattery(EntityUid target, EntityUid user, BatteryDrinkerComponent drinkerComp)
    {
        var source = target;
        var drinker = user;

        if (!TryComp<BatteryComponent>(source, out var sourceBattery))
            return;

        if (!TryGetFillableBattery(drinker, out var drinkerBattery, out var drinkerBatteryUid))
        {
            _popup.PopupEntity(Loc.GetString("battery-drinker-no-battery"), drinker, drinker);
            return;
        }
        if (!TryComp<BatteryDrinkerSourceComponent>(source, out var sourceComp))
        {
            _popup.PopupEntity(Loc.GetString("battery-drinker-no-source"), drinker, drinker);
            return;
        }

        var amountToDrink = drinkerBattery.MaxCharge * 0.10f;
        amountToDrink = MathF.Min(amountToDrink, _battery.GetCharge((source, sourceBattery)));
        amountToDrink = MathF.Min(amountToDrink, drinkerBattery.MaxCharge - _battery.GetCharge((drinkerBatteryUid, drinkerBattery)));

        if (sourceComp.MaxAmount > 0)
            amountToDrink = MathF.Min(amountToDrink, (float)sourceComp.MaxAmount);

        if (amountToDrink <= 0)
        {
            _popup.PopupEntity(Loc.GetString("battery-drinker-empty", ("target", source)), drinker, drinker);
            return;
        }

        if (float.IsNaN(amountToDrink) || float.IsInfinity(amountToDrink))
        {
            _popup.PopupEntity(Loc.GetString("battery-drinker-empty", ("target", source)), drinker, drinker);
            return;
        }

        var ev = new BatteryDrinkerDoAfterEvent
        {
            AmountToDrink = amountToDrink
        };

        var doAfterArgs = new DoAfterArgs(EntityManager, drinker, TimeSpan.FromSeconds(drinkerComp.DrinkSpeed), ev, eventTarget: drinker, target: source)
        {
            NeedHand = true,
            BreakOnMove = true,
            BreakOnDamage = true,
            MovementThreshold = 0.5f,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(EntityUid uid, BatteryDrinkerComponent comp, BatteryDrinkerDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is not { } source)
            return;

        var drinker = uid;
        var amountToDrink = args.AmountToDrink;

        if (!TryComp<BatteryComponent>(source, out var sourceBattery))
            return;

        if (!TryGetFillableBattery(drinker, out var drinkerBattery, out var drinkerBatteryUid))
            return;

        if (_battery.GetCharge((source, sourceBattery)) < amountToDrink)
            return;

        var newCharge = _battery.GetCharge((drinkerBatteryUid, drinkerBattery)) + amountToDrink;
        if (float.IsNaN(newCharge) || float.IsInfinity(newCharge))
        {
            _popup.PopupEntity(Loc.GetString("battery-drinker-error", ("target", source)), drinker, drinker);
            return;
        }

        var tryUse = _battery.TryUseCharge((source, sourceBattery), amountToDrink);
        if (tryUse)
        {
            _battery.ChangeCharge(drinkerBatteryUid, amountToDrink);
        }

        args.Handled = true;
    }
}
