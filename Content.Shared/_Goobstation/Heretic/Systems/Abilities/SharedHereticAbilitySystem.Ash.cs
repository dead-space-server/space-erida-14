// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BramvanZijp <56019239+BramvanZijp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic;

namespace Content.Shared._Goobstation.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeAsh()
    {
        SubscribeLocalEvent<EventHereticVolcanoBlast>(OnVolcanoBlast);
    }

    private void OnVolcanoBlast(EventHereticVolcanoBlast args)
    {
        if (!TryUseAbility(args, false))
            return;

        var ent = args.Performer;

        if (!_statusNew.TrySetStatusEffectDuration(ent,
                SharedFireBlastSystem.FireBlastStatusEffect,
                TimeSpan.FromSeconds(2)))
            return;

        args.Handled = true;

        var fireBlasted = EnsureComp<FireBlastedComponent>(ent);
        fireBlasted.Damage = -2f;

        if (!Heretic.TryGetHereticComponent(ent, out var heretic, out _) ||
            heretic is not { Ascended: true, CurrentPath: "Ash" })
            return;

        fireBlasted.MaxBounces *= 2;
        fireBlasted.BeamTime *= 0.66f;
    }
}
