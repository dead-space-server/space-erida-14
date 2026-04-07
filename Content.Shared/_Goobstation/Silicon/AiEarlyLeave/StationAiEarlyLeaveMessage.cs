using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Silicons;

[Serializable, NetSerializable]
public sealed class StationAiEarlyLeaveMessage : EuiMessageBase
{
    public readonly bool Confirmed;
    public StationAiEarlyLeaveMessage(bool confirmed)
    {
        Confirmed = confirmed;
    }
}
