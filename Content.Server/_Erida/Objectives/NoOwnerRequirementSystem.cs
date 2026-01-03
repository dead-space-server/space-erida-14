using Content.Server.CrewManifest;
using Content.Server.Station.Systems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server._Erida.Objectives;

public sealed class NoOwnerRequirementSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _minds = default!;
    [Dependency] private readonly CrewManifestSystem _crewManifest = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NoOwnerRequirementComponent, RequirementCheckEvent>(OnCheck);
    }

    private void OnCheck(Entity<NoOwnerRequirementComponent> ent, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

        var allAliveMinds = _minds.GetAliveHumans(args.Mind.OwnedEntity);

        foreach (var mind in allAliveMinds)
        {
            var stations = _entityManager.EntitySysManager.GetEntitySystem<StationSystem>().GetStations();
            if (_random.Prob(ent.Comp.IgnoreChance))
                return;

            var stationComp = stations[0];

            var (_, crewManifest) = _crewManifest.GetCrewManifest(stationComp);
            if (crewManifest == null)
                continue;

            var isValid = crewManifest.Entries.Any(x => ent.Comp.Job.Contains(x.JobTitle));

            if (!isValid)
            {
                args.Cancelled = true;
                return;
            }
            else
                return;
        }
    }
}
