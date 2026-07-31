// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class FleshSurgeryComponent : Component, ITouchSpell
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);

    [DataField]
    public EntityUid? Action { get; set; }

    [DataField]
    public TimeSpan Cooldown { get; set; } = TimeSpan.FromSeconds(20);

    [DataField]
    public LocId Speech { get; set; } = "heretic-speech-flesh-surgery";

    [DataField]
    public SoundSpecifier? Sound { get; set; } = new SoundPathSpecifier("/Audio/Items/welder.ogg");
}
