// SPDX-FileCopyrightText: 2026 Lytheriia
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Erida.Lactation;

[Serializable, NetSerializable]
public sealed partial class LactationDoAfterEvent : SimpleDoAfterEvent
{
    public LactationStatus Status;
};

public enum LactationStatus
{
    Drink,
    Collecting
};
