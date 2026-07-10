// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Heretic.EntitySystems;
using Content.Shared._Goobstation.Heretic;
using Content.Shared._Goobstation.Heretic.Prototypes;

namespace Content.Server._Goobstation.Heretic.Ritual;

public sealed partial class RitualCanAscend : RitualCustomBehavior
{
    [DataField]
    public bool CheckObjectives = true;

    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        var heretic = args.Mind.Comp;

        if (heretic.Ascended)
        {
            outstr = Loc.GetString("heretic-ritual-fail-already-ascended");
            return false;
        }

        if (!heretic.CanAscend)
        {
            outstr = Loc.GetString("heretic-ritual-fail-cannot-ascend");
            return false;
        }

        if (!CheckObjectives)
            return true;

        if (!args.EntityManager.System<HereticSystem>().ObjectivesAllowAscension(args.Mind))
        {
            outstr = Loc.GetString("heretic-ritual-fail-cannot-ascend-objectives");
            return false;
        }

        return true;
    }

    public override void Finalize(RitualData args)
    {
        // do nothing
    }
}
