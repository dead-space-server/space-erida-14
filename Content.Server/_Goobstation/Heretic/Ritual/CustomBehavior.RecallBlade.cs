// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic;
using Content.Shared._Goobstation.Heretic.Prototypes;
using Robust.Server.GameObjects;

namespace Content.Server._Goobstation.Heretic.Ritual;

public sealed partial class RitualRecallBladeBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;

        var entMan = args.EntityManager;
        var heretic = args.Mind.Comp;

        var transform = entMan.System<TransformSystem>();
        if (GetLostBlade(args.Platform, args.Performer, heretic, args.EntityManager, transform) != null)
            return true;

        outstr = Loc.GetString("heretic-ritual-fail-no-lost-blades");
        return false;
    }

    public override void Finalize(RitualData args)
    {
        var entMan = args.EntityManager;
        var heretic = args.Mind.Comp;

        var transform = entMan.System<TransformSystem>();
        if (GetLostBlade(args.Platform, args.Performer, heretic, args.EntityManager, transform) is not { } blade)
            return;

        transform.AttachToGridOrMap(blade);
        transform.SetMapCoordinates(blade, transform.GetMapCoordinates(args.Platform));
    }

    private EntityUid? GetLostBlade(EntityUid origin,
        EntityUid heretic,
        HereticComponent comp,
        IEntityManager entMan,
        TransformSystem transform)
    {
        if (comp.CurrentPath is not { } path || !comp.LimitedTransmutations.TryGetValue($"Blade{path}", out var blades))
            return null;

        var originCoords = transform.GetMapCoordinates(origin);
        var hereticCoords = transform.GetMapCoordinates(heretic);

        if (originCoords.MapId != hereticCoords.MapId)
            return null;

        var dist = (originCoords.Position - hereticCoords.Position).Length();

        var range = MathF.Max(1.5f, dist + 0.5f);

        foreach (var blade in blades)
        {
            if (!entMan.EntityExists(blade))
                continue;

            if (originCoords.InRange(transform.GetMapCoordinates(blade), range))
                continue;

            return blade;
        }

        return null;
    }
}
