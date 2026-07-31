// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Heretic.Ui;
using Content.Server.EUI;
using Content.Shared._Goobstation.Heretic;
using Content.Shared._Goobstation.Heretic.Prototypes;
using Robust.Shared.Player;

namespace Content.Server._Goobstation.Heretic.Ritual;

public sealed partial class RitualFeastOfOwlsBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;

        return true;
    }

    public override void Finalize(RitualData args)
    {
        if (args.Mind.Comp.Ascended || !args.Mind.Comp.CanAscend)
            return;

        if (!args.EntityManager.TryGetComponent(args.Performer, out ActorComponent? actor))
            return;

        var eui = IoCManager.Resolve<EuiManager>();
        eui.OpenEui(new FeastOfOwlsEui(args.Performer, args.Mind, args.Platform, args.EntityManager), actor.PlayerSession);
    }
}
