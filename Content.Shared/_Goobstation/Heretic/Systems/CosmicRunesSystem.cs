// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Goobstation.BlockTeleport;
using Content.Shared._Goobstation.FadingTimedDespawn;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic.Systems.Abilities;
using Content.Shared.Coordinates;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Heretic.Systems;

public sealed partial class CosmicRunesSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private IGameTiming _timing = default!;

    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedStarMarkSystem _starMark = default!;
    [Dependency] private SharedHereticAbilitySystem _heretic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCosmicRuneComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<HereticCosmicRuneComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<HereticCosmicRuneComponent, AfterInteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(Entity<HereticCosmicRuneComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (HasComp<FadingTimedDespawnComponent>(ent))
            return;

        if (TryComp(args.Used, out StarTouchComponent? starTouch))
        {
            _heretic.InvokeTouchSpell<StarTouchComponent>((args.Used, starTouch), args.User);
            EnsureComp<FadingTimedDespawnComponent>(ent).Lifetime = 0f;
            if (Exists(ent.Comp.LinkedRune))
                EnsureComp<FadingTimedDespawnComponent>(ent.Comp.LinkedRune.Value).Lifetime = 0f;
            args.Handled = true;
        }
    }

    private void OnActivate(Entity<HereticCosmicRuneComponent> ent, ref ActivateInWorldEvent args)
    {
        if (Teleport(ent, args.User))
            args.Handled = true;
    }

    private void OnInteract(Entity<HereticCosmicRuneComponent> ent, ref InteractHandEvent args)
    {
        if (Teleport(ent, args.User))
            args.Handled = true;
    }

    private bool Teleport(Entity<HereticCosmicRuneComponent> ent, EntityUid user)
    {
        var time = _timing.CurTime;

        if (time < ent.Comp.NextUse)
            return false;

        if (HasComp<FadingTimedDespawnComponent>(ent))
            return false;

        if (!Exists(ent.Comp.LinkedRune) || !TryComp(ent.Comp.LinkedRune.Value, out TransformComponent? xform) ||
            !xform.Coordinates.IsValid(EntityManager) ||
            HasComp<FadingTimedDespawnComponent>(ent.Comp.LinkedRune.Value))
        {
            if (_net.IsServer) // Client can have rune deleted due to PVS but can exist on server
                _popup.PopupEntity(Loc.GetString("heretic-cosmic-rune-fail-unlinked"), user, user);
            return false;
        }

        if (HasComp<StarMarkComponent>(user))
        {
            _popup.PopupClient(Loc.GetString("heretic-cosmic-rune-fail-star-mark"), user, user);
            return false;
        }

        if (!_transform.InRange(ent.Owner, user, ent.Comp.Range))
        {
            _popup.PopupClient(Loc.GetString("heretic-cosmic-rune-fail-range"), user, user);
            return false;
        }

        var ev = new TeleportAttemptEvent();
        RaiseLocalEvent(user, ref ev);
        if (ev.Cancelled)
            return false;

        ent.Comp.NextUse = time + ent.Comp.Delay;
        DirtyField(ent.Owner, ent.Comp, nameof(HereticCosmicRuneComponent.NextUse));
        if (TryComp(ent.Comp.LinkedRune.Value, out HereticCosmicRuneComponent? rune2))
        {
            rune2.NextUse = time + rune2.Delay;
            DirtyField(ent.Comp.LinkedRune.Value, rune2, nameof(HereticCosmicRuneComponent.NextUse));
        }

        if (_net.IsServer)
        {
            _audio.PlayPvs(ent.Comp.Sound, ent);
            _audio.PlayPvs(ent.Comp.Sound, ent.Comp.LinkedRune.Value);
            SpawnAttachedTo(ent.Comp.Effect, ent.Owner.ToCoordinates());
            SpawnAttachedTo(ent.Comp.Effect, ent.Comp.LinkedRune.Value.ToCoordinates());
        }

        var toTeleport = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.Range, LookupFlags.Dynamic)
            .Where(HasComp<StarMarkComponent>)
            .ToHashSet();
        toTeleport.Add(user);
        EntityUid? pulling = null;
        PullerComponent? puller = null;

        var isUserCosmosHeretic = HasComp<StarGazerComponent>(user) || HasComp<CosmosPassiveComponent>(user);

        if (isUserCosmosHeretic && TryComp(user, out puller) && puller.Pulling != null)
        {
            pulling = puller.Pulling.Value;
            toTeleport.Add(pulling.Value);
        }

        foreach (var entity in toTeleport)
        {
            _pulling.StopAllPulls(entity);
            _transform.SetCoordinates(entity, xform.Coordinates);
            _starMark.TryApplyStarMark(entity);
        }

        if (pulling != null)
            _pulling.TryStartPull(user, pulling.Value, puller, null);

        return true;
    }
}
