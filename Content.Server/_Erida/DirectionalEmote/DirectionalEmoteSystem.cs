using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Shared._Erida.DirectionalEmote;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Examine;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Erida.DirectionalEmote;

public sealed partial class DirectionalEmoteSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IGameTiming _gameTicking = default!;
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ChatBrainRotSystem _chatBrainRotSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<SendDirectionalEmoteEvent>(OnSendDirectionalEmoteEvent);
    }

    private void OnSendDirectionalEmoteEvent(SendDirectionalEmoteEvent args)
    {
        var source = GetEntity(args.Source);
        var target = GetEntity(args.Target);
        if (!TryComp<ActorComponent>(source, out var sourceActor) ||
            !TryComp<ActorComponent>(target, out var targetActor) ||
            !TryComp<DirectionalEmoteTargetComponent>(GetEntity(args.Source), out var directEmote)) return;

        var curTime = _gameTicking.CurTime;
        if (directEmote.LastSend + directEmote.Cooldown > curTime) return;

        var rangeError = Loc.GetString("directional-emote-range-error");
        if (!_examineSystem.InRangeUnOccluded(source, target, 3))
        {
            _chatManager.ChatMessageToOne(ChatChannel.Emotes, rangeError, rangeError, default, false, sourceActor.PlayerSession.Channel);
            return;
        }

        var lengthError = Loc.GetString("directional-emote-length-error");
        if (args.Text.Length > 10000)
        {
            _chatManager.ChatMessageToOne(ChatChannel.Emotes, lengthError, lengthError, default, false, sourceActor.PlayerSession.Channel);
            return;
        }

        var wrappedMessage = Loc.GetString("directional-emote-wrap-message", ("source", MetaData(source).EntityName), ("message", args.Text));

        _chatManager.ChatMessageToMany(ChatChannel.Emotes, args.Text, wrappedMessage, source, false, true, [targetActor.PlayerSession.Channel, sourceActor.PlayerSession.Channel]);
        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"{ToPrettyString(source):source} send directional emote to {ToPrettyString(target):target}: {args.Text}");
        _chatBrainRotSystem.CheckBrainRot(source, args.Text);
        directEmote.LastSend = curTime;
    }
}
