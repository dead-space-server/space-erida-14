// SPDX-FileCopyrightText: 2026 Lytheriia
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Erida.Lactation;

/// <summary>
/// List of allowed interactions for entity.
/// This will eliminate the need to create a new whitelist component for each system.
/// All new fields shoud be `= false`
/// </summary>
[RegisterComponent]
public sealed partial class InteractionWhitelistComponent : Component
{
    [DataField]
    public bool Lactation = false;
}

