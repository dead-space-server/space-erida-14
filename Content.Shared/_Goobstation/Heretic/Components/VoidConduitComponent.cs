// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VoidConduitComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Range;

    [DataField]
    public bool Active;

    [DataField]
    public int MaxRange = 8;

    [DataField]
    public Vector2 MinMaxWindowDamageMultiplier = new(1f, 2f);

    [DataField]
    public Vector2 MinMaxAirlockDamageMultiplier = new(2f, 4f);

    [DataField]
    public DamageSpecifier StructureDamage = new()
    {
        DamageDict =
        {
            { "Structural", 50 },
        },
    };

    [DataField]
    public SoundSpecifier WindowDamageSound =
        new SoundCollectionSpecifier("GlassSmack", AudioParams.Default.WithVolume(-4f));

    [DataField]
    public SoundSpecifier AirlockDamageSound =
        new SoundPathSpecifier("/Audio/Weapons/smash.ogg", AudioParams.Default.WithVolume(-4f));

    [DataField]
    public SpriteSpecifier OverlaySprite =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/void_overlay.rsi"), "voidtile");
}
