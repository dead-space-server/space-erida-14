// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HealingAuraComponent : Component
{
    [DataField]
    public DamageSpecifier ToHeal = new()
    {
        DamageDict =
        {
            {"Blunt", -1.5f},
            {"Slash", -1.5f},
            {"Piercing", -1.5f},
            {"Heat", -1.5f},
            {"Cold", -1.5f},
            {"Shock", -1.5f},
            {"Asphyxiation", -1.5f},
            {"Bloodloss", -1.5f},
            {"Caustic", -1.5f},
            {"Poison", -1.5f},
            {"Radiation", -1.5f},
            {"Cellular", -1.5f},
            {"Holy", -1.5f},
        },
    };

    [DataField]
    public float Range = 4f;

    [DataField]
    public float HealDelay = 1f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Accumulator;

    [DataField]
    public FixedPoint2 PainHeal = -3f;

    [DataField]
    public FixedPoint2 BoneHeal = -3f;

    [DataField]
    public FixedPoint2 BleedHeal = -1f;

    [DataField]
    public FixedPoint2 BloodHeal = 10f;

    [DataField]
    public FixedPoint2 WoundHeal = -3f;

    /// <summary>
    /// Set this to 0 to disable self-heal
    /// </summary>
    [DataField]
    public float SelfHealMultiplier = 1f;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public Dictionary<string, float>? ComponentHealMultipliers;
}
