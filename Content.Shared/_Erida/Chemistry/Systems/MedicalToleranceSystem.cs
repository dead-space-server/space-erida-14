using Robust.Shared.Timing;
using System.Linq;
using Content.Shared._Erida.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;

namespace Content.Shared._Erida.Chemistry.Systems;

public sealed class MedicalToleranceSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _gameTiming.CurTime;

        var playersWithMedTolComponent = EntityQuery<MedicalToleranceComponent>().Select(pair => pair.Owner).ToList();

        foreach (var playerEntity in playersWithMedTolComponent)
        {

            var medTolComponent = EntityManager.GetComponent<MedicalToleranceComponent>(playerEntity);

        }
    }

    public void SetTolerance(EntityUid playerEntity, ReagentId reagentId, float tolerance)
    {
        var comp = EntityManager.GetComponent<MedicalToleranceComponent>(playerEntity);
        comp.Tolerances[reagentId] = tolerance;
    }

    public void IncrementTolerance(EntityUid playerEntity, ReagentId reagentId, float increment)
    {
        var comp = EntityManager.GetComponent<MedicalToleranceComponent>(playerEntity);
        if (!comp.Tolerances.ContainsKey(reagentId))
        {
            comp.Tolerances.Add(reagentId, 0f);
        }
        comp.Tolerances[reagentId] += increment;
    }

    public float GetTolerance(EntityUid playerEntity, ReagentId reagentId)
    {
        var comp = EntityManager.GetComponent<MedicalToleranceComponent>(playerEntity);
        return comp.Tolerances.TryGetValue(reagentId, out var tolerance) ? tolerance : 0f;
    }
}
