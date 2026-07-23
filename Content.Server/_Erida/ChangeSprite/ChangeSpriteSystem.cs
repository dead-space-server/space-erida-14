// SPDX-FileCopyrightText: 2026 vanomorodellefake <vanomorodellefake29@gmail.com>
// SPDX-License-Identifier: MIT

using Content.Shared._Erida.ChangeSprite;
using Content.Shared._Erida.ChangeSprite.Components;
using Content.Shared.Actions;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._Erida.ChangeSprite;

public sealed partial class ChangeSpriteSystem : EntitySystem
{
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private SharedActionsSystem _action = default!;
    private const string ChangeSpriteBuiXmlGeneratedName = "ChangeSpriteBoundUserInterface";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangeSpriteComponent, ComponentInit>(OnChangeSpriteInit);
        SubscribeLocalEvent<ChangeSpriteComponent, ChangeSpriteActionEvent>(OnChangeSpriteActionEvent);
        SubscribeLocalEvent<ChangeSpriteComponent, ChangeSpriteMessage>(OnChangeSpriteMessage);
        SubscribeLocalEvent<ChangeSpriteComponent, ChangeSpriteNothingMessage>(OnChangeSpriteNothingMessage);
    }

    private void OnChangeSpriteInit(Entity<ChangeSpriteComponent> ent, ref ComponentInit args)
    {
        if (ent.Comp.Sprites == null)
        {
            RemComp(ent.Owner, ent.Comp);
            return;
        }

        _action.AddAction(ent, ref ent.Comp.ChangeSpriteActionEntity, ent.Comp.ChangeSpriteAction);

        if (ent.Comp.ChangeSpriteActionEntity is { } actionEnt)
        {
            _action.SetEntityIcon(actionEnt, ent.Owner);
        }

        var userInterfaceComp = EnsureComp<UserInterfaceComponent>(ent);
        _ui.SetUi((ent, userInterfaceComp), ChangeSpriteUiKey.Key, new InterfaceData(ChangeSpriteBuiXmlGeneratedName));
    }

    private void OnChangeSpriteActionEvent(Entity<ChangeSpriteComponent> ent, ref ChangeSpriteActionEvent args)
    {
        if (!TryComp<UserInterfaceComponent>(ent, out var userInterfaceComp))
            return;

        if (!_ui.IsUiOpen((ent, userInterfaceComp), ChangeSpriteUiKey.Key, args.Performer))
        {
            _ui.OpenUi((ent, userInterfaceComp), ChangeSpriteUiKey.Key, args.Performer);
        }
    }

    private void OnChangeSpriteMessage(Entity<ChangeSpriteComponent> ent, ref ChangeSpriteMessage args)
    {
        RemoveChangeSpriteAction(ent.Comp);

        if (ent.Comp.Sprites == null
            || !ent.Comp.Sprites.Contains(args.ProtoId))
            return;

        if (!TryComp<AppearanceComponent>(ent, out var appearanceComp))
            return;

        if (!_proto.HasIndex(args.ProtoId))
            return;

        _appearance.SetData(ent, ChangeSpriteVisuals.SpriteId, args.ProtoId, appearanceComp);
    }

    private void OnChangeSpriteNothingMessage(Entity<ChangeSpriteComponent> ent, ref ChangeSpriteNothingMessage args)
    {
        RemoveChangeSpriteAction(ent.Comp);
    }

    private void RemoveChangeSpriteAction(ChangeSpriteComponent comp)
    {
        if (comp.ChangeSpriteActionEntity is not { } actionEnt)
            return;

        _action.RemoveAction(actionEnt);
    }
}
