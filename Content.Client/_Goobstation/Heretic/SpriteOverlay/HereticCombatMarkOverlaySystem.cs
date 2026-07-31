// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic;
using Robust.Client.GameObjects;

namespace Content.Client._Goobstation.Heretic.SpriteOverlay;

public sealed class HereticCombatMarkOverlaySystem : SpriteOverlaySystem<HereticCombatMarkComponent>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HereticCombatMarkComponent, AfterAutoHandleStateEvent>((uid, comp, _) =>
            AddOverlay(uid, comp));
    }

    protected override int? GetLayerIndex(Entity<SpriteComponent> ent, HereticCombatMarkComponent comp)
    {
        return comp.Path == "Cosmos" ? 0 : null; // Cosmos mark should be behind the sprite
    }

    protected override void UpdateOverlayLayer(Entity<SpriteComponent> ent,
        HereticCombatMarkComponent comp,
        int layer,
        EntityUid? source = null)
    {
        base.UpdateOverlayLayer(ent, comp, layer, source);
        var state = comp.Path.ToLower();
        Sprite.LayerSetRsiState(ent.AsNullable(), layer, state);
    }
}
