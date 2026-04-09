using Content.Server.Administration.Logs;
using Content.Shared.Actions;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.Lock;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server.Silicons.Borgs;

public sealed partial class BorgSystem
{
    private const string ReturnToCoreAction = "ActionStationAiReturnToCore";

    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly LockSystem _lock = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    private void InitializeStationAiControl()
    {
        SubscribeLocalEvent<BorgChassisComponent, StationAiControlBorgEvent>(OnStationAiControlBorg);
        SubscribeLocalEvent<BorgChassisComponent, StationAiSetBorgLockEvent>(OnStationAiSetBorgLock);
        SubscribeLocalEvent<BorgControlComponent, StationAiReturnToCoreEvent>(OnStationAiReturnToCore);
        SubscribeLocalEvent<BorgControlComponent, MindUnvisitedMessage>(OnMindUnvisited);
    }

    private void OnStationAiControlBorg(Entity<BorgChassisComponent> ent, ref StationAiControlBorgEvent args)
    {
        if (!args.TakeControl)
            return;

        if (!TryComp<BorgControlComponent>(ent.Owner, out var control) ||
            !HasComp<StationAiHeldComponent>(args.User) ||
            !_mindSystem.TryGetMind(args.User, out var aiMindId, out var aiMind) ||
            aiMind.OwnedEntity != args.User)
        {
            PopupStationAi(args.User, "ai-device-not-responding");
            return;
        }

        if (aiMind.IsVisitingEntity)
        {
            PopupStationAi(args.User, "station-ai-borg-control-busy");
            return;
        }

        if (control.OriginalAi != null || HasComp<VisitingMindComponent>(ent.Owner))
        {
            PopupStationAi(args.User, "station-ai-borg-already-controlled");
            return;
        }

        if (_playerManager.TryGetSessionByEntity(ent.Owner, out _) ||
            TryComp<MindContainerComponent>(ent.Owner, out var mindContainer) && mindContainer.HasMind)
        {
            PopupStationAi(args.User, "station-ai-borg-occupied");
            return;
        }

        if (_mobState.IsIncapacitated(ent.Owner) || !_powerCell.HasDrawCharge(ent.Owner))
        {
            PopupStationAi(args.User, "ai-device-not-responding");
            return;
        }

        _mindSystem.Visit(aiMindId, ent.Owner, aiMind);

        if (aiMind.VisitingEntity != ent.Owner)
        {
            PopupStationAi(args.User, "ai-device-not-responding");
            return;
        }

        control.OriginalAi = args.User;

        if (!_actions.AddAction(ent.Owner, ref control.ReturnToAiAction, ReturnToCoreAction, ent.Owner))
        {
            control.OriginalAi = null;
            _mindSystem.UnVisit(aiMindId, aiMind);
            PopupStationAi(args.User, "ai-device-not-responding");
            return;
        }

        if (!ent.Comp.Active)
            TryActivate(ent, args.User);

        _adminLogger.Add(LogType.Action, LogImpact.High,
            $"{ToPrettyString(args.User):user} took temporary control of borg {ToPrettyString(ent.Owner)} via the Station AI radial.");
    }

    private void OnStationAiSetBorgLock(Entity<BorgChassisComponent> ent, ref StationAiSetBorgLockEvent args)
    {
        if (!HasComp<StationAiHeldComponent>(args.User))
            return;

        if (!TryComp<LockComponent>(ent.Owner, out var lockComp))
        {
            PopupStationAi(args.User, "ai-device-not-responding");
            return;
        }

        // Match normal alt-click lock behavior:
        // decide the action on the server from the current lock state instead of trusting a client-side snapshot.
        var wasLocked = lockComp.Locked;

        var success = wasLocked
            ? _lock.TryUnlock(ent.Owner, args.User, lockComp, skipDoAfter: true)
            : _lock.TryLock(ent.Owner, args.User, lockComp, skipDoAfter: true);

        if (!success || lockComp.Locked == wasLocked)
            return;

        if (TryGetStationAiSession(args.User, out var session))
        {
            _popup.PopupEntity(Loc.GetString(lockComp.Locked
                    ? "lock-comp-do-lock-success"
                    : "lock-comp-do-unlock-success",
                ("entityName", Identity.Name(ent.Owner, EntityManager))),
                ent.Owner,
                session!);

            _audio.PlayEntity(lockComp.Locked ? lockComp.LockSound : lockComp.UnlockSound, session!, ent.Owner);
        }

        _adminLogger.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(args.User):user} {(lockComp.Locked ? "locked" : "unlocked")} borg {ToPrettyString(ent.Owner)} via the Station AI radial.");
    }

    private void OnStationAiReturnToCore(Entity<BorgControlComponent> ent, ref StationAiReturnToCoreEvent args)
    {
        args.Handled = true;

        if (!TryGetControllingAiMind(ent, out var mindId, out var mind))
        {
            ClearStationAiControl(ent);
            return;
        }

        var aiEntity = ent.Comp.OriginalAi;
        _mindSystem.UnVisit(mindId, mind);

        if (aiEntity != null)
        {
            _adminLogger.Add(LogType.Action, LogImpact.Low,
                $"{ToPrettyString(aiEntity.Value):user} returned from borg {ToPrettyString(ent.Owner)} to their Station AI core.");
        }
    }

    private void OnMindUnvisited(Entity<BorgControlComponent> ent, ref MindUnvisitedMessage args)
    {
        ClearStationAiControl(ent);
    }

    private bool TryGetControllingAiMind(Entity<BorgControlComponent> ent, out EntityUid mindId, out MindComponent? mind)
    {
        mindId = default;
        mind = null;

        if (ent.Comp.OriginalAi == null)
            return false;

        if (!TryComp<VisitingMindComponent>(ent.Owner, out var visiting) ||
            visiting.MindId == null)
        {
            return false;
        }

        mindId = visiting.MindId.Value;

        if (!TryComp(mindId, out mind))
            return false;

        return mind.IsVisitingEntity &&
               mind.VisitingEntity == ent.Owner &&
               mind.OwnedEntity == ent.Comp.OriginalAi;
    }

    private void ClearStationAiControl(Entity<BorgControlComponent> ent)
    {
        _actions.RemoveAction(ent.Comp.ReturnToAiAction);
        ent.Comp.ReturnToAiAction = null;
        ent.Comp.OriginalAi = null;
    }

    private void PopupStationAi(EntityUid user, string message)
    {
        if (!TryGetStationAiSession(user, out var session))
            return;

        _popup.PopupCursor(Loc.GetString(message), session!, PopupType.MediumCaution);
    }

    private bool TryGetStationAiSession(EntityUid user, out ICommonSession? session)
    {
        if (_playerManager.TryGetSessionByEntity(user, out session))
            return true;

        if (_mindSystem.TryGetMind(user, out _, out var mind) &&
            mind.UserId is { } userId &&
            _playerManager.TryGetSessionById(userId, out session))
        {
            return true;
        }

        session = null;
        return false;
    }
}
