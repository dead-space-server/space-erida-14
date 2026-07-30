// SPDX-FileCopyrightText: 2026 Lytheriia
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<bool> AutomaticVoteEnabled =
        CVarDef.Create("autovote.enabled", false, CVar.SERVERONLY);

    public static readonly CVarDef<TimeSpan> AutomaticVoteDuration =
        CVarDef.Create("autovote.duration", TimeSpan.FromSeconds(30), CVar.SERVERONLY);

    public static readonly CVarDef<TimeSpan> AutomaticVoteStartAt =
        CVarDef.Create("autovote.startat", TimeSpan.FromSeconds(55), CVar.SERVERONLY);

    public static readonly CVarDef<int> AutomaticVoteMinPlayersForForce =
            CVarDef.Create("autovote.minplayerceforce", 15, CVar.SERVERONLY);

    public static readonly CVarDef<TimeSpan> AutomaticVoteFailedTimeReduce =
            CVarDef.Create("autovote.failedreduce", TimeSpan.FromSeconds(-60), CVar.SERVERONLY);
}
