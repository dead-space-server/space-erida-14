using Robust.Shared.Timing;
using System.Linq;
using Content.Shared._Erida.Chemistry.Components;

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

    public void SetTolerance(EntityUid playerEntity, string drugId, float tolerance)
    {
        var comp = EntityManager.GetComponent<MedicalToleranceComponent>(playerEntity);
        comp.Tolerances[drugId] = tolerance;
    }

    public void IncrementTolerance(EntityUid playerEntity, string drugId, float increment)
    {
        var comp = EntityManager.GetComponent<MedicalToleranceComponent>(playerEntity);
        if (!comp.Tolerances.ContainsKey(drugId))
        {
            comp.Tolerances.Add(drugId, 0f);
        }
        comp.Tolerances[drugId] += increment;
    }

    public float GetTolerance(EntityUid playerEntity, string drugId)
    {
        var comp = EntityManager.GetComponent<MedicalToleranceComponent>(playerEntity);
        return comp.Tolerances.TryGetValue(drugId, out var tolerance) ? tolerance : 0f;
    }
}

