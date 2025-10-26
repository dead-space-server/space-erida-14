using Robust.Shared.Timing;
using System.Linq;
using Content.Shared.Chemistry.Components;

public sealed class ToleranceRegenerationSystem : EntitySystem
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


            foreach (var drugId in medTolComponent.Tolerances.Keys.ToList())
            {

                var reducedTolerance = Math.Max(medTolComponent.Tolerances[drugId] - 0.01f, 0f);
                medTolComponent.SetTolerance(drugId, reducedTolerance);
            }
        }
    }
}
