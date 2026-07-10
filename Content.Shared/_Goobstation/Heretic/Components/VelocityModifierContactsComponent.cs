// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VelocityModifierContactsComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Modifier = 1.0f;

    [DataField, AutoNetworkedField]
    public bool IsActive = true;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;
}

[NetworkedComponent, RegisterComponent, AutoGenerateComponentState]
public sealed partial class VelocityModifiedByContactComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2? OriginalVelocity;
}
