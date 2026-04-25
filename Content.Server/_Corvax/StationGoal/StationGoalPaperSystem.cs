using System.Linq;
using Content.Server.Fax;
using Content.Shared.GameTicking;
using Content.Server.Station.Systems;
using Content.Shared.Fax.Components;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.CCVar;

namespace Content.Server._Corvax.StationGoal
{
    /// <summary>
    ///     System to spawn paper with station goal.
    /// </summary>
    public sealed class StationGoalPaperSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly FaxSystem _fax = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly StationSystem _station = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
        }

        private void OnRoundStarted(RoundStartedEvent ev)
        {
            if (!_cfg.GetCVar(CCVars.StationGoal))
                return;

            var playerCount = _playerManager.PlayerCount;

            var query = EntityQueryEnumerator<StationGoalComponent>();
            while (query.MoveNext(out var uid, out var station))
            {
                var tempGoals = _proto.EnumeratePrototypes<StationGoalPrototype>().ToHashSet();
                var whiteListedGoals = station.WhiteListedGoals.Select(id => _proto.Index(id)).ToHashSet();
                var specialGoals = station.SpecialGoals.Select(id => _proto.Index(id)).ToHashSet();
                if (whiteListedGoals.Count == 0)
                {
                    var blackListedGoals = station.BlackListedGoals.Select(id => _proto.Index(id)).ToHashSet();

                    tempGoals.ExceptWith(blackListedGoals);
                }
                else
                    tempGoals = whiteListedGoals;

                StationGoalPrototype? selGoal = null;
                while (tempGoals.Count > 0)
                {
                    var goalProto = _random.Pick(tempGoals);

                    if (playerCount > goalProto.MaxPlayers ||
                        playerCount < goalProto.MinPlayers)
                    {
                        tempGoals.Remove(goalProto);
                        continue;
                    }

                    if (goalProto.SendOnlyWhenWhitelisted)
                        if (!whiteListedGoals.Contains(goalProto))
                            continue;

                    if (goalProto.IsSpecialGoal)
                        if (!specialGoals.Contains(goalProto))
                            continue;

                    selGoal = goalProto;
                    break;
                }

                if (selGoal is null)
                    return;

                if (SendStationGoal(uid, selGoal))
                {
                    Log.Info($"Goal {selGoal.ID} has been sent to station {MetaData(uid).EntityName}");
                }
            }
        }

        public bool SendStationGoal(EntityUid ent, ProtoId<StationGoalPrototype> goal)
        {
            return SendStationGoal(ent, _proto.Index(goal));
        }

        /// <summary>
        ///     Send a station goal on selected station to all faxes which are authorized to receive it.
        /// </summary>
        /// <returns>True if at least one fax received paper</returns>
        public bool SendStationGoal(EntityUid ent, StationGoalPrototype goal)
        {
            var printout = new FaxPrintout(
                Loc.GetString(goal.Text, ("station", MetaData(ent).EntityName)),
                Loc.GetString("station-goal-fax-paper-name"),
                null,
                null,
                "paper_stamp-centcom",
                [new() { StampedName = Loc.GetString("stamp-component-stamped-name-centcom"), StampedColor = Color.FromHex("#006600") }]
            );

            var wasSent = false;
            var query = EntityQueryEnumerator<FaxMachineComponent>();
            while (query.MoveNext(out var faxUid, out var fax))
            {
                if (!fax.ReceiveAllStationGoals && !(fax.ReceiveStationGoal && _station.GetOwningStation(faxUid) == ent))
                    continue;

                _fax.Receive(faxUid, printout, null, fax);

                foreach (var spawnEnt in goal.Spawns)
                    SpawnAtPosition(spawnEnt, Transform(faxUid).Coordinates);

                wasSent |= fax.ReceiveStationGoal;
            }

            return wasSent;
        }
    }
}
