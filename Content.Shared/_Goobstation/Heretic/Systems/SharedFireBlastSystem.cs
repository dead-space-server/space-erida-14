// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Goobstation.Physics;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Heretic.Systems;

public abstract partial class SharedFireBlastSystem : EntitySystem
{
    [Dependency] protected SharedTransformSystem Xform = default!;
    [Dependency] protected StatusEffectsSystem Status = default!;
    [Dependency] protected DamageableSystem Dmg = default!;

    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedStaminaSystem _stam = default!;

    public static readonly EntProtoId FireBlastStatusEffect = "StatusEffectFireBlasted";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireBlastedStatusEffectComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<FireBlastedStatusEffectComponent, StatusEffectRemovedEvent>(OnRemoved);
    }

    private void OnRemoved(Entity<FireBlastedStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (TerminatingOrDeleted(args.Target))
            return;

        RemCompDeferred<FireBlastedComponent>(args.Target);
    }

    private void OnApplied(Entity<FireBlastedStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        EnsureComp<FireBlastedComponent>(args.Target);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<FireBlastedComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var blast, out var dmg))
        {
            blast.Accumulator += frameTime;

            if (blast.Accumulator < blast.TickInterval)
                continue;

            blast.Accumulator = 0f;

            UpdateBeams((uid, blast));

            if (blast.Damage == 0f)
                continue;

            var damage = new DamageSpecifier
            {
                DamageDict =
                {
                    { "Heat", blast.Damage },
                },
            };

            Dmg.TryChangeDamage(uid,
                damage,
                true,
                false);

            var stamDmg = blast.Damage * blast.StaminaDamageMultiplier;

            _stam.TakeOvertimeStaminaDamage(uid, stamDmg);
        }
    }

    private void UpdateBeams(Entity<FireBlastedComponent, ComplexJointVisualsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp2, false))
            return;

        var hasFireBeams = false;

        foreach (var (netEnt, _) in ent.Comp2.Data.Where(x => x.Value.Id == ent.Comp1.FireBlastBeamDataId).ToList())
        {
            if (!TryGetEntity(netEnt, out var target) || TerminatingOrDeleted(target) ||
                !Xform.InRange(target.Value, ent.Owner, ent.Comp1.FireBlastRange))
            {
                ent.Comp2.Data.Remove(netEnt);
                continue;
            }

            hasFireBeams = true;

            BeamCollision(ent, target.Value);
        }

        Dirty(ent.Owner, ent.Comp2);

        if (hasFireBeams)
            return;

        ent.Comp1.ShouldBounce = false;
        Status.TryRemoveStatusEffect(ent, FireBlastStatusEffect);
    }

    protected virtual void BeamCollision(Entity<FireBlastedComponent> origin, EntityUid target)
    {
    }
}
