using Content.Shared._Lavaland.Chemistry.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Chemistry.Systems;

public sealed partial class BloodstreamRegenerationSystem : EntitySystem
{
    [Dependency] private SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodstreamRegenerationComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BloodstreamRegenerationComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextRegenTime = _timing.CurTime + ent.Comp.Duration;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BloodstreamRegenerationComponent, BloodstreamComponent>();
        while (query.MoveNext(out var uid, out var regen, out var bloodstream))
        {
            if (_timing.CurTime < regen.NextRegenTime)
                continue;

            regen.NextRegenTime += regen.Duration;
            Dirty(uid, regen);

            if (!_solutionContainer.ResolveSolution(uid, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var bloodSolution))
                continue;

            var amount = FixedPoint2.Min(bloodSolution.AvailableVolume, regen.Generated.Volume);
            if (amount <= FixedPoint2.Zero)
                continue;

            var generated = amount == regen.Generated.Volume
                ? regen.Generated
                : regen.Generated.Clone().SplitSolution(amount);

            _bloodstream.TryAddToBloodstream((uid, bloodstream), generated);
        }
    }
}
