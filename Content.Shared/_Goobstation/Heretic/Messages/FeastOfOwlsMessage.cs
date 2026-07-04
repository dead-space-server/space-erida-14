using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Messages;

[Serializable, NetSerializable]
public sealed class FeastOfOwlsMessage(bool accepted) : EuiMessageBase
{
    public readonly bool Accepted = accepted;
}
