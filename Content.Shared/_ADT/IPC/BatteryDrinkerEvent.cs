// SPDX-FileCopyrightText: 2026 ADT Development
// SPDX-FileCopyrightText: 2026 OpenWendor
// SPDX-License-Identifier: MIT

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._ADT.Silicon;

[Serializable, NetSerializable]
public sealed partial class BatteryDrinkerDoAfterEvent : SimpleDoAfterEvent
{
    public float AmountToDrink;
}
