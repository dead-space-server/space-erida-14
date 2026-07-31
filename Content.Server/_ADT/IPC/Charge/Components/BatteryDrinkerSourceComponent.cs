// SPDX-FileCopyrightText: 2026 ADT Development
// SPDX-FileCopyrightText: 2026 OpenWendor
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;

namespace Content.Server._ADT.Silicon.Charge;

[RegisterComponent]
public sealed partial class BatteryDrinkerSourceComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int? MaxAmount = null;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DrinkSpeedMulti = 1f;

    [DataField]
    public SoundSpecifier? DrinkSound = null;
}
