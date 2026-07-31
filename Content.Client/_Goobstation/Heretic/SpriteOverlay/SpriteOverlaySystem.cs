// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared._Goobstation.Heretic.SpriteOverlay;
using Robust.Client.GameObjects;

namespace Content.Client._Goobstation.Heretic.SpriteOverlay;

public abstract partial class SpriteOverlaySystem<T> : EntitySystem where T : BaseSpriteOverlayComponent
{
    [Dependency] protected SpriteSystem Sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<T, AppearanceChangeEvent>((uid, comp, _) => AddOverlay(uid, comp));
        SubscribeLocalEvent<T, ComponentStartup>((uid, comp, _) => AddOverlay(uid, comp));
        SubscribeLocalEvent<T, ComponentShutdown>((uid, comp, _) => RemoveOverlay(uid, comp));
    }

    // comp is separate from ent so that it is possible other entity's component for overlays
    public virtual void RemoveOverlay(Entity<SpriteComponent?> ent, T comp)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;
        if (!Sprite.LayerMapTryGet(ent, comp.Key, out var layer, false))
            return;
        Sprite.RemoveLayer(ent, layer);
        var ev = new SpriteOverlayUpdatedEvent<T>(this, comp, false);
        RaiseLocalEvent(ent, ref ev);
    }

    // source is owner of comp (if null it just assumes ent is owner)
    public virtual void AddOverlay(Entity<SpriteComponent?> ent, T comp, EntityUid? source = null)
    {
        if (comp.Sprite == null)
        {
            RemoveOverlay(ent, comp);
            return;
        }

        if (!Resolve(ent, ref ent.Comp))
            return;
        var index = GetLayerIndex((ent, ent.Comp), comp);
        if (!Sprite.LayerMapTryGet(ent, comp.Key, out var layer, false))
        {
            layer = Sprite.AddLayer(ent, comp.Sprite, index);
            Sprite.LayerMapSet(ent, comp.Key, layer);
        }
        else if (index is { } i && layer != i)
        {
            Sprite.RemoveLayer(ent, layer, false);
            layer = Sprite.AddLayer(ent, comp.Sprite, i);
            Sprite.LayerMapSet(ent, comp.Key, layer);
        }

        if (comp.Unshaded)
            ent.Comp.LayerSetShader(layer, "unshaded");
        if (comp.Offset != Vector2.Zero)
            Sprite.LayerSetOffset(ent, layer, comp.Offset);
        UpdateOverlayLayer((ent.Owner, ent.Comp), comp, layer, source);
        var ev = new SpriteOverlayUpdatedEvent<T>(this, comp, true);
        RaiseLocalEvent(ent, ref ev);
    }

    protected virtual void UpdateOverlayLayer(Entity<SpriteComponent> ent, T comp, int layer, EntityUid? source = null)
    {
    }

    protected virtual int? GetLayerIndex(Entity<SpriteComponent> ent, T comp)
    {
        return null;
    }
}

[ByRefEvent]
public readonly record struct SpriteOverlayUpdatedEvent<T>(SpriteOverlaySystem<T> Sys, T Comp, bool Added)
    where T : BaseSpriteOverlayComponent;
