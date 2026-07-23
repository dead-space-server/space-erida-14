// SPDX-FileCopyrightText: 2026 vanomorodellefake <vanomorodellefake29@gmail.com>
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Erida.ChangeSprite.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChangeSpriteComponent : Component
{
    [DataField]
    public List<ProtoId<ChangeSpritePrototype>>? Sprites = default!;

    [DataField]
    public EntProtoId ChangeSpriteAction = "ActionChangeSprite";

    [DataField, AutoNetworkedField]
    public EntityUid? ChangeSpriteActionEntity;
}
