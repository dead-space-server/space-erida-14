// SPDX-FileCopyrightText: 2026 ADT Development
// SPDX-FileCopyrightText: 2026 OpenWendor
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._ADT.Silicon.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SeeingStaticComponent : Component
{
    [AutoNetworkedField]
    public float Multiplier = 1f;
}
