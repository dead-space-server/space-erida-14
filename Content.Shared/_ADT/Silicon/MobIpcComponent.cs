// SPDX-FileCopyrightText: 2026 ADT Development
// SPDX-FileCopyrightText: 2026 OpenWendor
// SPDX-License-Identifier: MIT

namespace Content.Shared._ADT.Silicon;

[RegisterComponent]
public sealed partial class MobIpcComponent : Component
{
    [DataField]
    public bool DisablePointLightOnDeath = false;

    [DataField]
    public bool LightDisabledByDeath;
}
