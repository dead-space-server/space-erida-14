// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CosmosComboComponent : Component
{
    [DataField]
    public Dictionary<EntityUid, int> HitEntities = new();

    [DataField]
    public float ComboDuration = 3f;

    [DataField]
    public float ComboIncreaseTime = 0.5f;

    [DataField]
    public float MaxComboDuration = 10f;

    [DataField]
    public float ComboTimer = 3f;

    [DataField]
    public int ComboCounter;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_GoobStation/Heretic/cosmic_energy.ogg");

    [DataField]
    public DamageSpecifier DamageToSecondTargets = new()
    {
        DamageDict =
        {
            { "Heat", 14 },
        },
    };

    [DataField]
    public DamageSpecifier DamageToThirdTargets = new()
    {
        DamageDict =
        {
            { "Heat", 28 },
        },
    };

    [DataField]
    public EntProtoId SecondTargetEffect = "EffectCosmicExplosion";

    [DataField]
    public EntProtoId ThirdTargetEffect = "EffectCosmicDomain";
}
