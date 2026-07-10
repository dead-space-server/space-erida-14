// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Shared._Goobstation.Conversion;
using Content.Shared._Goobstation.Heretic;
using Content.Shared.Mind;

namespace Content.Shared._Goobstation.Heretic.Systems;

public abstract partial class SharedHereticSystem : EntitySystem
{
    [Dependency] private SharedMindSystem _mind = default!;

    private EntityQuery<HereticComponent> _hereticQuery;
    private EntityQuery<GhoulComponent> _ghoulQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCheckEvent>(OnCheck);
        SubscribeLocalEvent<BeforeConversionEvent>(OnBeforeConversion);

        _hereticQuery = GetEntityQuery<HereticComponent>();
        _ghoulQuery = GetEntityQuery<GhoulComponent>();
    }

    private void OnBeforeConversion(ref BeforeConversionEvent ev)
    {
        if (TryGetHereticComponent(ev.Uid, out _, out _))
            ev.Blocked = true;
    }

    private void OnCheck(ref HereticCheckEvent ev)
    {
        ev.Result = TryGetHereticComponent(ev.Uid, out _, out _);
    }

    public bool TryGetHereticComponent(
        EntityUid uid,
        [NotNullWhen(true)] out HereticComponent? heretic,
        out EntityUid mind)
    {
        heretic = null;
        return _mind.TryGetMind(uid, out mind, out _) && _hereticQuery.TryComp(mind, out heretic);
    }

    public bool IsHereticOrGhoul(EntityUid uid)
    {
        return _ghoulQuery.HasComp(uid) || TryGetHereticComponent(uid, out _, out _);
    }
}
