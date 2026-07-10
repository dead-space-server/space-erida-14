// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Religion;
using Content.Server.Damage.Systems;
using Content.Server.Temperature.Components;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Actions;
using Content.Shared.Ghost;
using Content.Shared._Goobstation.Heretic;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Temperature.Components;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Server._Goobstation.Heretic.EntitySystems;

public sealed partial class IceSpearSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _action = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedProjectileSystem _projectile = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IceSpearComponent, ThrowDoHitEvent>(OnThrowDoHit,
            after: new[] { typeof(DamageOtherOnHitSystem), typeof(SharedProjectileSystem) });
    }

    private void OnThrowDoHit(Entity<IceSpearComponent> ent, ref ThrowDoHitEvent args)
    {
        if (!HasComp<MobStateComponent>(args.Target))
            return;

        var hitNullRodUser = IsTouchSpellDenied(args.Target); // hit a null rod

        if (!HasComp<GhostComponent>(args.Target) &&
            HasComp<TemperatureComponent>(args.Target) && !hitNullRodUser)
            EnsureComp<IceCubeComponent>(args.Target);

        if (Exists(ent.Comp.ActionId))
            _action.SetIfBiggerCooldown(ent.Comp.ActionId, ent.Comp.ShatterCooldown);

        if (TryComp(ent, out EmbeddableProjectileComponent? embeddable))
            _projectile.EmbedDetach(ent, embeddable);

        var coords = Transform(ent).Coordinates;
        _audio.PlayPvs(ent.Comp.ShatterSound, coords);
        QueueDel(ent);
    }

    private bool IsTouchSpellDenied(EntityUid target)
    {
        var ev = new BeforeCastTouchSpellEvent(target);
        RaiseLocalEvent(target, ev, true);

        return ev.Cancelled;
    }
}
