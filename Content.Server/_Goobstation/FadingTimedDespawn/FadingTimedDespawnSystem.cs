using Content.Shared._Goobstation.FadingTimedDespawn;

namespace Content.Server._Goobstation.FadingTimedDespawn;

public sealed class FadingTimedDespawnSystem : SharedFadingTimedDespawnSystem
{
    protected override bool CanDelete(EntityUid uid)
    {
        return true;
    }
}
