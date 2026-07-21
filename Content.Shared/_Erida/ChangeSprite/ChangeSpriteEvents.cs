// SPDX-FileCopyrightText: 2026 vanomorodellefake <vanomorodellefake29@gmail.com>
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Erida.ChangeSprite;

public sealed partial class ChangeSpriteActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed class ChangeSpriteMessage(ProtoId<ChangeSpritePrototype> protoId) : BoundUserInterfaceMessage
{
    public readonly ProtoId<ChangeSpritePrototype> ProtoId = protoId;
}

[Serializable, NetSerializable]
public sealed class ChangeSpriteNothingMessage() : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public enum ChangeSpriteUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public enum ChangeSpriteVisuals : byte
{
    SpriteId
}
