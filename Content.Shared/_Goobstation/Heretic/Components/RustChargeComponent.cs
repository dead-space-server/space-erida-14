// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class RustChargeComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            {"Blunt", 50f},
        },
    };

    [DataField]
    public ProtoId<TagPrototype> IgnoreTag = "IgnoreImmovableRod";

    [DataField]
    public SoundSpecifier HitSound = new SoundCollectionSpecifier("MetalSlam");

    [DataField]
    public List<EntityUid> DamagedEntities = new();

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2);
}
