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
