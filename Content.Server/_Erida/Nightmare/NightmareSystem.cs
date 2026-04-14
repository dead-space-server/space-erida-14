using System.Runtime.CompilerServices;
using Content.Server._Erida.LightIntension;
using Content.Server._Erida.Nightmare.Components;
using Robust.Shared.Timing;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server._Erida.Nightmare;

public sealed class NightmareSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming _timing = default!;
    [Dependency] protected readonly LightIntensionSystem _lightIntension = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<NightmareComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var nmComp, out var xform))
        {
            if (nmComp.timeToCheck < curTime)
            {
                nmComp.timeToCheck = curTime + TimeSpan.FromSeconds(nmComp.timeBetweenChecks);

                var lightIntension = _lightIntension.TryGetLightLevel((uid, xform));
                Logger.Debug($"LightIntension: {lightIntension}");
            }
        }
    }
}
