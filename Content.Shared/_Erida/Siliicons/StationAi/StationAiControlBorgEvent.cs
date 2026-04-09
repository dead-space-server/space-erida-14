using System;
using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.StationAi;

[Serializable, NetSerializable]
public sealed partial class StationAiControlBorgEvent : BaseStationAiAction
{
    public bool TakeControl;
}

[Serializable, NetSerializable]
public sealed partial class StationAiSetBorgLockEvent : BaseStationAiAction
{
    public bool Locked;
}

public sealed partial class StationAiReturnToCoreEvent : InstantActionEvent
{
}
