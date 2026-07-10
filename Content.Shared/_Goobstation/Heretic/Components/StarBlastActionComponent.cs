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

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StarBlastActionComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Projectile;

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(25);

    [DataField]
    public EntProtoId Effect = "EffectCosmicCloud";

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_GoobStation/Heretic/cosmic_energy.ogg");
}
