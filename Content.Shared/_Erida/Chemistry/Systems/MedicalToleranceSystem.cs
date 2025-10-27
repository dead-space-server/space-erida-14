using Robust.Shared.Timing;
using System.Linq;
using Content.Shared._Erida.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared._Erida.Chemistry.Helpers;

namespace Content.Shared._Erida.Chemistry.Systems;

public sealed class MedicalToleranceSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var enumerator = EntityQueryEnumerator<MedicalToleranceComponent>();
        while (enumerator.MoveNext(out var _, out var medTolComponent))
        {

            var keysCopy = medTolComponent.Tolerances.Keys.ToList();

            foreach (var reagentId in keysCopy)
            {

                float oldTolerance = medTolComponent.Tolerances[reagentId];
                float newTolerance = Math.Max(oldTolerance - frameTime * MedicalToleranceComponent.ToleranceDecay, 0f);

                if (newTolerance == 0f)
                {
                    medTolComponent.Tolerances.Remove(reagentId);
                }
                else
                {
                    medTolComponent.Tolerances[reagentId] = newTolerance;
                }
            }
        }
    }

    public void ApplyDrugEffect(EntityUid playerEntity, ReagentId reagentId, float originalEffect)
    {

        float tolerance = GetTolerance(playerEntity, reagentId);


        float adjustedEffect = DrugMechanicsHelper.CalculateAdjustedDrugEffect(originalEffect, tolerance);

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
        comp.Tolerances[reagentId] = Math.Min(comp.Tolerances[reagentId] + increment, 1f);
    }

    public float GetTolerance(EntityUid playerEntity, ReagentId reagentId)
    {
        var comp = EntityManager.GetComponent<MedicalToleranceComponent>(playerEntity);
        return comp.Tolerances.TryGetValue(reagentId, out var tolerance) ? tolerance : 0f;
    }
}
