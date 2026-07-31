// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class HereticCosmicRuneComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedRune;

    [DataField]
    public float Range = 0.75f;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_GoobStation/Heretic/cosmic_energy.ogg");

    [DataField]
    public EntProtoId Effect = "HereticRuneCosmosLight";

    [DataField, AutoPausedField, AutoNetworkedField]
    public TimeSpan NextUse = TimeSpan.Zero;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(2);
}
