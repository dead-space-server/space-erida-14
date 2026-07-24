// SPDX-FileCopyrightText: 2026 ADT Development
// SPDX-FileCopyrightText: 2026 OpenWendor
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared._ADT.Grab;

[Serializable, NetSerializable]
public enum GrabStage
{
    No = 0,
    Soft = 1,
    Hard = 2,
    Suffocate = 3,
}

public enum GrabStageDirection
{
    Increase,
    Decrease,
}

public enum GrabResistResult
{
    TooSoon,
    Failed,
    Succeeded,
}

public enum GrabAttemptResult
{
    Succeeded,
    OnCooldown,
    Failed,
}
