// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Goobstation.Heretic.Systems;

public abstract partial class SharedHereticCombatMarkSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;

    public virtual bool ApplyMarkEffect(EntityUid target,
        HereticCombatMarkComponent mark,
        string? path,
        EntityUid user,
        HereticComponent heretic)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        _audio.PlayPredicted(mark.TriggerSound, target, user);
        RemCompDeferred(target, mark);
        return true;
    }
}
