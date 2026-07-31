// SPDX-FileCopyrightText: 2026 Lytheriia
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking.Presets;
using Robust.Shared.Prototypes;

namespace Content.Server._Erida.Lobby;

[Prototype]
public sealed partial class AutoVotesPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Title;

    [DataField(required: true)]
    public List<AutoVoteOptionData> Options = [];

    [DataField]
    public bool ShouldBeInFirstVote = false;
}

[DataDefinition]
public partial struct AutoVoteOptionData
{
    [DataField(required: true)]
    public AutoVoteOptionAnswerData AnswerData;

    [DataField(required: true)]
    public LocId Label;
}

[DataDefinition]
public partial struct AutoVoteOptionAnswerData
{
    [DataField(required: true)]
    public AutoVoteOptionAction Action;

    [DataField]
    public List<ProtoId<AutoVotesPrototype>> NextVoteProto = [];

    [DataField]
    public ProtoId<GamePresetPrototype> GamePresetProto = "Extended";
}

public enum AutoVoteOptionAction
{
    NextVote,
    GameModeStart
}
