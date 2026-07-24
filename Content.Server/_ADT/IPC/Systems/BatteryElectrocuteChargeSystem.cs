// SPDX-FileCopyrightText: 2026 ADT Development
// SPDX-FileCopyrightText: 2026 OpenWendor
// SPDX-License-Identifier: MIT

using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Electrocution;
using Content.Shared.Power.Components;
using Robust.Shared.Random;

namespace Content.Server._ADT.Power.Systems;

public sealed partial class BatteryElectrocuteChargeSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private BatterySystem _battery = default!;

    private const float DamagePerWatt = 0.0015f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, ElectrocutedEvent>(OnElectrocuted);
    }

    private void OnElectrocuted(EntityUid uid, BatteryComponent battery, ref ElectrocutedEvent args)
    {
        float shockDamage = 7.5f;
        if (args.SourceUid != null && TryComp<ElectrifiedComponent>(args.SourceUid.Value, out var electrified))
        {
            shockDamage = electrified.ShockDamage;
        }

        if (shockDamage <= 0)
            return;

        var damagePerWatt = DamagePerWatt * 2;

        var damage = shockDamage * args.SiemensCoefficient;
        var charge = Math.Min(damage / damagePerWatt, battery.MaxCharge * 0.25f) * _random.NextFloat(0.75f, 1.25f);

        _battery.ChangeCharge((uid, battery), charge);

        _popup.PopupEntity(Loc.GetString("battery-electrocute-charge"), uid, uid);
    }
}
