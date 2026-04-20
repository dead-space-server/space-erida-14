using System;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.Borgs;

[Serializable, NetSerializable]
public enum BorgRadialAction : byte
{
    TogglePanel,
    TakeControl,
    ReleaseControl
}

[Serializable, NetSerializable]
public sealed partial class BorgRadialActionEvent : EntityEventArgs
{
    public BorgRadialAction Action;

    public BorgRadialActionEvent()
    {
    }

    public BorgRadialActionEvent(BorgRadialAction action)
    {
        Action = action;
    }
}
