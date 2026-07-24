// SPDX-FileCopyrightText: 2026 ADT Development
// SPDX-FileCopyrightText: 2026 OpenWendor
// SPDX-License-Identifier: MIT

using Robust.Shared.Random;
using Content.Shared._ADT.Silicon.Components;
using Content.Server.Power.Components;
using Content.Shared.Mobs.Systems;
using Content.Server.Temperature.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.PowerCell.Components;
using Content.Shared.Alert;
using Content.Shared._ADT.Silicon.Systems;
using Content.Shared.Movement.Systems;
using Content.Server.Body.Components;
using Content.Shared.Mind.Components;
using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Timing;
using Content.Shared._ADT.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;
using Content.Shared.Movement.Components;
using Robust.Shared.Physics.Components;
using Content.Shared.Power.Components;
using Content.Shared.Temperature.Components;
using Content.Shared.Atmos.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;

namespace Content.Server._ADT.Silicon.Charge;

public sealed partial class SiliconChargeSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private FlammableSystem _flammable = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private MovementSpeedModifierSystem _moveMod = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IConfigurationManager _config = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedJetpackSystem _jetpack = default!;
    [Dependency] private PowerCellSystem _powerCell = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconComponent, ComponentStartup>(OnSiliconStartup);
    }

    public bool TryGetSiliconBattery(EntityUid silicon, [NotNullWhen(true)] out BatteryComponent? batteryComp, out EntityUid batteryUid)
    {
        batteryComp = null;
        batteryUid = EntityUid.Invalid;

        if (!HasComp<SiliconComponent>(silicon))
            return false;

        if (TryComp(silicon, out batteryComp))
        {
            batteryUid = silicon;
            return true;
        }

        if (_powerCell.TryGetBatteryFromSlot(silicon, out var battery) && battery.HasValue)
        {
            batteryComp = battery.Value.Comp;
            batteryUid = battery.Value.Owner;
            return true;
        }

        return false;
    }

    private void OnSiliconStartup(EntityUid uid, SiliconComponent component, ComponentStartup args)
    {
        if (!HasComp<PowerCellSlotComponent>(uid))
            return;

        if (component.EntityType.GetType() != typeof(SiliconType))
            DebugTools.Assert("SiliconComponent.EntityType is not a SiliconType enum.");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SiliconComponent>();
        while (query.MoveNext(out var silicon, out var siliconComp))
        {
            if (_mobState.IsDead(silicon)
                || !siliconComp.BatteryPowered)
                continue;

            if (siliconComp.EntityType.Equals(SiliconType.Npc))
            {
                var updateTime = _config.GetCVar(SimpleStationCCVars.SiliconNpcUpdateTime);
                if (_timing.CurTime - siliconComp.LastDrainTime < TimeSpan.FromSeconds(updateTime))
                    continue;

                siliconComp.LastDrainTime = _timing.CurTime;
            }

            if (!TryGetSiliconBattery(silicon, out var batteryComp, out var batteryUid))
            {
                UpdateChargeState(silicon, 0, siliconComp);
                if (_alerts.IsShowingAlert(silicon, siliconComp.BatteryAlert))
                {
                    _alerts.ClearAlert(silicon, siliconComp.BatteryAlert);
                    _alerts.ShowAlert(silicon, siliconComp.NoBatteryAlert);
                }
                continue;
            }

            if (TryComp<MindContainerComponent>(silicon, out var mindContComp)
                && !mindContComp.HasMind)
                continue;

            var drainRate = siliconComp.DrainPerSecond;

            var drainRateFinalAddi = 0f;

            if (!siliconComp.EntityType.Equals(SiliconType.Npc))
            {
                drainRateFinalAddi += SiliconHeatEffects(silicon, siliconComp, frameTime) - 1;
                drainRateFinalAddi += SiliconMovementEffects(silicon, siliconComp);
            }

            drainRate += Math.Clamp(drainRateFinalAddi, drainRate * -0.9f, batteryComp.MaxCharge / 240);

            _powerCell.TryUseCharge(silicon, frameTime * drainRate);

            var chargePercent = (short)MathF.Round(_battery.GetChargeLevel((batteryUid, batteryComp)) * 10f);

            UpdateChargeState(silicon, chargePercent, siliconComp);
        }
    }

    public void UpdateChargeState(EntityUid uid, short chargePercent, SiliconComponent component)
    {
        if (component.ChargeState != chargePercent)
        {
            component.ChargeState = chargePercent;
            Dirty(uid, component);
        }

        RaiseLocalEvent(uid, new SiliconChargeStateUpdateEvent(chargePercent));

        _moveMod.RefreshMovementSpeedModifiers(uid);

        if (_alerts.IsShowingAlert(uid, component.NoBatteryAlert) && chargePercent != 0)
        {
            _alerts.ClearAlert(uid, component.NoBatteryAlert);
            _alerts.ShowAlert(uid, component.BatteryAlert, chargePercent);
        }
    }

    private float SiliconHeatEffects(EntityUid silicon, SiliconComponent siliconComp, float frameTime)
    {
        if (!TryComp<TemperatureComponent>(silicon, out var temperComp)
            || !TryComp<ThermalRegulatorComponent>(silicon, out var thermalComp))
            return 0;

        if (!TryComp<TemperatureDamageComponent>(silicon, out var tempDamageComp))
            return 0;

        var upperThresh = thermalComp.NormalBodyTemperature + thermalComp.ThermalRegulationTemperatureThreshold;
        var upperThreshHalf = thermalComp.NormalBodyTemperature + thermalComp.ThermalRegulationTemperatureThreshold * 0.5f;

        if (temperComp.CurrentTemperature > upperThreshHalf)
        {
            var hotTempMulti = Math.Min(temperComp.CurrentTemperature / upperThreshHalf, 4);

            siliconComp.OverheatAccumulator += frameTime;
            if (!(siliconComp.OverheatAccumulator >= 5))
                return hotTempMulti;

            siliconComp.OverheatAccumulator -= 5;

            if (!TryComp<FlammableComponent>(silicon, out var flamComp)
                || flamComp is { OnFire: true }
                || !(temperComp.CurrentTemperature > tempDamageComp.HeatDamageThreshold))
                return hotTempMulti;

            _popup.PopupEntity(Loc.GetString("silicon-overheating"), silicon, silicon, PopupType.MediumCaution);
            if (!_random.Prob(Math.Clamp(temperComp.CurrentTemperature / (upperThresh * 5), 0.001f, 0.9f)))
                return hotTempMulti;

            _flammable.AdjustFireStacks(silicon, Math.Clamp(siliconComp.FireStackMultiplier, -10, 10), flamComp);
            _flammable.Ignite(silicon, silicon, flamComp);
            return hotTempMulti;
        }

        if (temperComp.CurrentTemperature < thermalComp.NormalBodyTemperature)
            return 0.5f + temperComp.CurrentTemperature / thermalComp.NormalBodyTemperature * 0.5f;

        return 0;
    }

    private float SiliconMovementEffects(EntityUid silicon, SiliconComponent siliconComp)
    {
        if (!TryComp(silicon, out MovementSpeedModifierComponent? movement) ||
            !TryComp(silicon, out PhysicsComponent? physics) ||
            !TryComp(silicon, out InputMoverComponent? input))
            return 0;

        if (input.HeldMoveButtons == MoveButtons.None || _jetpack.IsUserFlying(silicon))
            return siliconComp.DrainPerSecond * siliconComp.IdleDrainReduction * (-1);

        if (movement.CurrentSprintSpeed <= 0f)
            return siliconComp.DrainPerSecond * siliconComp.IdleDrainReduction * -1;

        return Math.Clamp(
            siliconComp.DrainPerSecond * ((physics.LinearVelocity.Length() / movement.CurrentSprintSpeed) - 1),
            siliconComp.DrainPerSecond * siliconComp.IdleDrainReduction * (-1),
            0f);
    }
}
