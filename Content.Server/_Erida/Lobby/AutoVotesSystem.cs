// SPDX-FileCopyrightText: 2026 Lytheriia
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Erida.Lobby;

public sealed partial class AutoVotesSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IVoteManager _voteManager = default!;
    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IChatManager _chatManager = default!;
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    private List<string> _previousGamerules = [];
    private AutoVoteOptionData _previousChoosedMainOption = new();
    private TimeSpan _startAfter = TimeSpan.Zero;
    private bool _voteTriggered;

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(CCVars.AutomaticVoteStartAt,
            value =>
            {
                if (value != TimeSpan.Zero)
                    _startAfter = value;
            }, true);

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
    }

    private void FindAndStartMainVotes()
    {
        var options = new VoteOptions
        {
            Duration = _cfg.GetCVar(CCVars.AutomaticVoteDuration),
        };

        options.InitiatorText = Loc.GetString("ui-vote-initiator-server");

        if (_cfg.GetCVar(CCVars.AutomaticVoteMinPlayersForForce) < _playerManager.PlayerCount)
        {
            foreach (var proto in _prototypeManager.EnumeratePrototypes<AutoVotesPrototype>())
                if (proto.ShouldBeInFirstVote)
                {
                    options.Title = Loc.GetString(proto.Title);

                    foreach (var x in proto.Options)
                    {
                        if (_previousChoosedMainOption.Label == x.Label)
                            continue;

                        options.Options.Add((Loc.GetString(x.Label), x));
                        break;
                    }
                    break;
                }

        }
        else
            foreach (var proto in _prototypeManager.EnumeratePrototypes<AutoVotesPrototype>())
                if (proto.ShouldBeInFirstVote)
                {
                    options.Title = Loc.GetString(proto.Title);

                    foreach (var option in proto.Options)
                    {
                        options.Options.Add((Loc.GetString(option.Label), option));
                    }

                    break;
                }

        var vote = _voteManager.CreateVote(options);

        vote.OnFinished += (_, args) => OnFinished(args);
    }

    private void OnFinished(VoteFinishedEventArgs args)
    {
        AutoVoteOptionData winner;
        if (args.Winner == null)
        {
            winner = (AutoVoteOptionData)_random.Pick(args.Winners);
            _chatManager.DispatchServerAnnouncement(
                Loc.GetString("ui-vote-gamemode-tie", ("picked", Loc.GetString(winner.Label))));
        }
        else
        {
            winner = (AutoVoteOptionData)args.Winner;
            _chatManager.DispatchServerAnnouncement(
                Loc.GetString("ui-vote-gamemode-win", ("winner", Loc.GetString(winner.Label))));
        }

        _previousChoosedMainOption = winner;

        _adminLog.Add(LogType.Vote, LogImpact.Medium, $"Preset vote finished: {winner.Label}");

        switch (winner.AnswerData.Action)
        {
            case AutoVoteOptionAction.NextVote:
                {
                    StartNewVote(winner.AnswerData);
                    break;
                }
            case AutoVoteOptionAction.GameModeStart:
                {
                    StartGamePreset(winner.AnswerData);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    private void StartNewVote(AutoVoteOptionAnswerData data)
    {
        var options = new VoteOptions
        {
            Duration = _cfg.GetCVar(CCVars.AutomaticVoteDuration),
        };

        foreach (var proto in data.NextVoteProto)
        {
            var protoData = _prototypeManager.Index(proto);
            options.Title = Loc.GetString(protoData.Title);
            options.InitiatorText = Loc.GetString("ui-vote-initiator-server");

            foreach (var option in protoData.Options)
            {
                if (option.AnswerData.Action == AutoVoteOptionAction.GameModeStart
                    && _previousGamerules.Count != 0
                    && option.AnswerData.GamePresetProto.Id == _previousGamerules[_previousGamerules.Count - 1])
                    continue;

                options.Options.Add((Loc.GetString(option.Label), option));
            }
        }

        var vote = _voteManager.CreateVote(options);

        // Hello to recursion <3
        vote.OnFinished += (_, args) => OnFinished(args);
    }

    private void StartGamePreset(AutoVoteOptionAnswerData data)
    {
        if (_previousGamerules.Count >= 3)
            _previousGamerules.RemoveAt(0);

        _previousGamerules.Add(data.GamePresetProto.Id);

        var ticker = _entityManager.EntitySysManager.GetEntitySystem<GameTicker>();
        ticker.SetGamePreset(data.GamePresetProto);
    }

    private void OnRoundStarting(RoundStartingEvent _)
    {
        _voteTriggered = false;
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent _)
    {
        _voteTriggered = false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_gameTicker.RunLevel != GameRunLevel.PreRoundLobby)
            return;

        if (_playerManager.PlayerCount == 0)
            return;

        if (_voteTriggered)
            return;

        var remaining = _gameTicker.RoundStartTimeSpan - _timing.CurTime;

        if (-remaining >= _startAfter)
        {
            _voteTriggered = true;

            _voteManager.CreateStandardVote(null, Shared.Voting.StandardVoteType.Map);

            FindAndStartMainVotes();
        }
    }
}
