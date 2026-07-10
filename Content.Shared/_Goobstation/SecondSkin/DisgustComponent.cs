// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.SecondSkin;

[RegisterComponent, NetworkedComponent]
public sealed partial class DisgustComponent : Component
{
    [DataField]
    public float Level;

    [DataField]
    public float ReductionRate = 2f;

    [DataField]
    public float AccumulationMultiplier = 1f;

    [DataField]
    public float UpdateTime = 1f;

    [ViewVariables]
    public float Accumulator;

    [DataField(serverOnly: true), NonSerialized]
    public Dictionary<float, List<EntityEffect>> EffectsThresholds = new();

    [DataField]
    public ProtoId<AlertPrototype> Alert = "Disgust";

    [DataField]
    public Dictionary<float, short> SeverityLevels = new()
    {
        { 5f, 1 },
        { 30f, 2 },
        { 60f, 3 },
    };
}
