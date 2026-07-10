// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class RustObjectsInRadiusComponent : Component
{
    [DataField]
    public float RustRadius = 1.5f;

    [DataField]
    public float LookupRange = 0.1f;

    [DataField]
    public int RustStrength = 10;

    [DataField]
    public EntProtoId TileRune = "TileHereticRustRune";

    [DataField, AutoPausedField]
    public TimeSpan NextRustTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan RustPeriod = TimeSpan.FromSeconds(0.1);
}
