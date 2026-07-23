// SPDX-FileCopyrightText: 2026 Lytheriia
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Erida.Lactation;

/// <summary>
/// List of allowed interactions for entity.
/// This will eliminate the need to create a new whitelist component for each system.
/// </summary>
/// <remarks>
/// All new fields should default to <c>false</c>.
/// </remarks>
[RegisterComponent]
public sealed partial class InteractionWhitelistComponent : Component
{
    [DataField]
    public bool Lactation = false;
}

