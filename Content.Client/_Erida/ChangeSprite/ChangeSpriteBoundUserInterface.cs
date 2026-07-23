// SPDX-FileCopyrightText: 2026 vanomorodellefake <vanomorodellefake29@gmail.com>
// SPDX-License-Identifier: MIT

using Content.Client.UserInterface.Controls;
using Content.Shared._Erida.ChangeSprite;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Client.UserInterface;
using Robust.Client.GameObjects;
using Content.Shared._Erida.ChangeSprite.Components;

namespace Content.Client._Erida.ChangeSprite.UI;

[UsedImplicitly]
public sealed partial class ChangeSpriteBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private SimpleRadialMenu? _menu;

    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SimpleRadialMenu>();
        Update();
        _menu.OpenOverMouseScreenPosition();
    }

    public override void Update()
    {
        if (_menu == null)
            return;

        if (!EntMan.TryGetComponent<ChangeSpriteComponent>(Owner, out var csComp))
            return;

        var models = ConvertToButtons(Owner, csComp.Sprites);

        _menu.SetButtons(models);
    }

    private IEnumerable<RadialMenuOptionBase> ConvertToButtons(EntityUid currentSprite, IEnumerable<ProtoId<ChangeSpritePrototype>>? changeSpritePrototypes)
    {
        var list = new List<RadialMenuOptionBase>();

        if (_entityManager.TryGetComponent<SpriteComponent>(currentSprite, out var spriteComponent))
        {
            var nullOption = new RadialMenuActionOption<object>(SendOldSpriteSelected, data: null!)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(currentSprite),
                ToolTip = Loc.GetString("changesprite-no-change")
            };

            list.Add(nullOption);
        }

        if (changeSpritePrototypes == null)
            return list;

        foreach (var changeProtoIDSprite in changeSpritePrototypes)
        {

            if (!_prototypeManager.TryIndex(changeProtoIDSprite, out var changeSprite))
                continue;

            var option = new RadialMenuActionOption<ProtoId<ChangeSpritePrototype>>(SendNewSpriteSelected, changeProtoIDSprite)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(changeSprite.Icon),
                ToolTip = Loc.GetString(changeSprite.Name)
            };
            list.Add(option);
        }

        return list;
    }

    private void SendNewSpriteSelected(ProtoId<ChangeSpritePrototype> prototype)
    {
        SendPredictedMessage(new ChangeSpriteMessage(prototype));
    }

    private void SendOldSpriteSelected(object? _)
    {
        SendPredictedMessage(new ChangeSpriteNothingMessage());
    }
}
