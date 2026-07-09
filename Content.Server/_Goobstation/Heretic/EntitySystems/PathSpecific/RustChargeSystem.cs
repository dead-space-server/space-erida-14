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
