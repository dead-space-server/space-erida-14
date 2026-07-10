// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.StatusIcon;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StarGazerComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> MasterIcon = "GhoulHereticMaster";

    [DataField]
    public float MaxDistance = 20f;

    [ViewVariables, NonSerialized]
    public ICommonSession? ResettingMindSession;

    [DataField]
    public float GhostRoleTimer = 20f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float GhostRoleAccumulator;

    [DataField]
    public float ResetDistanceTimer = 5f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float ResetDistanceAccumulator;

    [DataField]
    public EntProtoId TeleportEffect = "EffectCosmicCloud";

    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/_GoobStation/Heretic/cosmic_energy.ogg");
}
