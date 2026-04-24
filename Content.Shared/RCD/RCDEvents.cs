using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.RCD;

[Serializable, NetSerializable]
public sealed class RCDSystemMessage(ProtoId<RCDPrototype> protoId) : BoundUserInterfaceMessage
{
    public ProtoId<RCDPrototype> ProtoId = protoId;
}

[Serializable, NetSerializable]
public sealed class RCDConstructionGhostRotationEvent(NetEntity netEntity, Direction direction) : EntityEventArgs
{
    public readonly NetEntity NetEntity = netEntity;
    public readonly Direction Direction = direction;
}

// Erida start
[Serializable, NetSerializable]
public sealed class RPDSelectedLayerEvent(NetEntity netEntity, byte layer) : EntityEventArgs
{
    public readonly NetEntity NetEntity = netEntity;
    public readonly byte Layer = layer;
}
// Erida end

[Serializable, NetSerializable]
public enum RcdUiKey : byte
{
    Key
}
