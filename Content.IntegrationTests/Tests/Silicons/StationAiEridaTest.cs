// SPDX-FileCopyrightText: 2026 Yaroslav Yudaev <ydaevy10@gmail.com>
// SPDX-License-Identifier: MIT
// Erida edit - Station AI borg control regression tests

using Content.IntegrationTests.Fixtures;
using Content.Shared.Hands.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Storage.Components;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Silicons;

[TestFixture]
public sealed class StationAiEridaTest : GameTest
{
    [TestPrototypes]
    private const string Prototypes = @"
- type: entity
  id: StationAiBorgChargerTest
  name: test borg charger
  components:
  - type: EntityStorage
  - type: StationAiWhitelist

- type: entity
  id: StationAiControllableBorgTest
  name: test controllable borg
  components:
  - type: BorgChassis
    active: true
  - type: BorgControl
  - type: Actions
  - type: ContainerContainer
    containers:
      borg_brain: !type:ContainerSlot { }
      borg_module: !type:Container { }
";

    [Test]
    public async Task StationAiBorgControlIsAvailableOnClient()
    {
        var pair = Pair;
        var server = pair.Server;
        var client = pair.Client;
        var map = await pair.CreateTestMap();

        EntityUid borg = default;
        await server.WaitPost(() =>
        {
            borg = server.EntMan.SpawnEntity("BorgChassisSelectable", map.GridCoords);
        });

        await pair.ReallyBeIdle(5);

        var clientBorg = client.EntMan.GetEntity(server.EntMan.GetNetEntity(borg));

        Assert.Multiple(() =>
        {
            Assert.That(server.EntMan.HasComponent<BorgControlComponent>(borg), Is.True);
            Assert.That(client.EntMan.HasComponent<BorgControlComponent>(clientBorg), Is.True);
        });
    }

    [Test]
    public async Task StationAiCanToggleBorgChargerWithoutHands()
    {
        var pair = Pair;
        var server = pair.Server;
        var map = await pair.CreateTestMap();

        EntityUid stationAi = default;
        EntityUid charger = default;

        await server.WaitPost(() =>
        {
            stationAi = server.EntMan.SpawnEntity("StationAiBrain", map.GridCoords);
            charger = server.EntMan.SpawnEntity("StationAiBorgChargerTest", map.GridCoords);

            var ev = new StationAiToggleBorgChargerEvent { User = stationAi };
            server.EntMan.EventBus.RaiseLocalEvent(charger, (object) ev);
        });

        await server.WaitPost(() =>
        {
            Assert.That(server.EntMan.HasComponent<HandsComponent>(stationAi), Is.False);
            Assert.That(server.EntMan.GetComponent<EntityStorageComponent>(charger).Open, Is.True);
        });
    }

    [Test]
    public async Task StationAiCanTakeControlOfBorg()
    {
        var pair = Pair;
        var server = pair.Server;
        var map = await pair.CreateTestMap();

        EntityUid stationAi = default;
        EntityUid borg = default;
        Entity<MindComponent> mind = default;

        await server.WaitPost(() =>
        {
            var mindSystem = server.System<SharedMindSystem>();
            stationAi = server.EntMan.SpawnEntity("StationAiBrain", map.GridCoords);
            borg = server.EntMan.SpawnEntity("StationAiControllableBorgTest", map.GridCoords);

            server.EntMan.EnsureComponent<StationAiHeldComponent>(stationAi);
            mind = mindSystem.CreateMind(null);
            mindSystem.TransferTo(mind.Owner, stationAi, mind: mind.Comp);

            var ev = new StationAiControlBorgEvent
            {
                User = stationAi,
                TakeControl = true
            };
            server.EntMan.EventBus.RaiseLocalEvent(borg, (object) ev);
        });

        await server.WaitPost(() =>
        {
            Assert.That(server.EntMan.GetComponent<MindComponent>(mind.Owner).VisitingEntity, Is.EqualTo(borg));
            Assert.That(server.EntMan.HasComponent<VisitingMindComponent>(borg), Is.True);
            Assert.That(server.EntMan.GetComponent<BorgControlComponent>(borg).OriginalAi, Is.EqualTo(stationAi));
        });
    }
}
