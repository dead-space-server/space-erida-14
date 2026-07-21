// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic;
using Content.Shared._Goobstation.Heretic.Prototypes;
using Content.Shared.Speech.Muting;

namespace Content.Server._Goobstation.Heretic.Ritual;

public sealed partial class RitualMuteGhoulifyBehavior : RitualSacrificeBehavior
{
    public override void Finalize(RitualData args)
    {
        if (args is { Limit: > 0, Limited: not null } && args.Limited.Count >= args.Limit)
            return;

        for (var i = 0; i < Math.Min(uids.Count, Max); i++)
        {
            var uid = uids[i];

            var minion = args.EntityManager.EnsureComponent<HereticMinionComponent>(uid);
            minion.BoundHeretic = args.Performer;

            var ghoul = new GhoulComponent
            {
                TotalHealth = 100f,
                GiveBlade = true,
            };
            args.EntityManager.AddComponent(uid, ghoul, overwrite: true);
            args.EntityManager.EnsureComponent<MutedComponent>(uid);
            args.EntityManager.EnsureComponent<HereticBladeUserBonusDamageComponent>(uid);

            if (args.Limited == null)
                continue;

            args.Limited.Add(uid);

            if (args.Limit > 0 && args.Limited.Count >= args.Limit)
                break;
        }
    }
}
