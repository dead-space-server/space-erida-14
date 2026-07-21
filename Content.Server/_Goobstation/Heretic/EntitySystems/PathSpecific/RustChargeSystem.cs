// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 the biggest bruh <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Destructible;
using Content.Shared._Goobstation.Heretic.Systems;

namespace Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;

public sealed partial class RustChargeSystem : SharedRustChargeSystem
{
    [Dependency] private DestructibleSystem _destructible = default!;

    protected override void DestroyStructure(EntityUid uid, EntityUid user)
    {
        base.DestroyStructure(uid, user);

        if (!TryComp(uid, out DestructibleComponent? destructible) || destructible.Thresholds.Count == 0)
        {
            Del(uid);
            return;
        }

        var threshold = destructible.Thresholds[^1];
        RaiseLocalEvent(uid, new DamageThresholdReached(destructible, threshold), true);
        _destructible.Execute(threshold, uid, user);
    }
}
