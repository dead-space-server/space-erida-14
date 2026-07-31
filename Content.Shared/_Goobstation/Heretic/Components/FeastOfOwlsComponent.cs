// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent]
public sealed partial class FeastOfOwlsComponent : Component
{
    [DataField]
    public int Reward = 5;

    [ViewVariables]
    public int CurrentStep;

    [DataField]
    public float Timer = 2f;

    [ViewVariables]
    public float ElapsedTime = 2f;

    [DataField]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan JitterStutterTime = TimeSpan.FromSeconds(1);

    [DataField]
    public SoundSpecifier KnowledgeGainSound = new SoundPathSpecifier("/Audio/_GoobStation/Heretic/eatfood.ogg");
}
