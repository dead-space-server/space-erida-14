// SPDX-FileCopyrightText: 2026 vanomorodellefake <vanomorodellefake29@gmail.com>
// SPDX-License-Identifier: MIT

using Content.Shared.Mobs;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Erida.ChangeSprite;

[Prototype]
public sealed partial class ChangeSpritePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/Actions/scream.png"));

    [DataField(required: true)]
    public List<ChangeSpriteLayer> Layers = default!;

    [DataField]
    public Dictionary<MobState, Dictionary<string, string>> DamageStateVisualLayers = [];
}

[DataDefinition]
public sealed partial class ChangeSpriteLayer
{
    [DataField(required: true)]
    public SpriteSpecifier Sprite = default!;

    [DataField(required: true)]
    public string LayerKey = default!;

    [DataField]
    public bool Visible = true;

    [DataField]
    public string StateId = default!;
}
