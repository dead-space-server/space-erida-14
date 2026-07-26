using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared._White.Grab; // Goobstation
using Content.Shared.Alert;
using Content.Shared.Buckle.Components;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.CombatMode; // Goobstation
using Content.Shared.Damage; // Goobstation
using Content.Shared.Damage.Systems; // Goobstation
using Content.Shared.Database;
using Content.Shared.Effects; // Goobstation
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs.Components; // Goobstation
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Pulling.Events;
using Content.Shared.Speech; // Goobstation
using Content.Shared.Throwing; // Goobstation
using Robust.Shared.Audio; // Goobstation
using Robust.Shared.Audio.Systems; // Goobstation
using Content.Shared.Standing;
using Robust.Shared.Network; // Goobstation
using Robust.Shared.Random; // Goobstation
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Numerics; // goobstation

namespace Content.Shared.Movement.Pulling.Systems;

/// <summary>
/// Allows one entity to pull another behind them via a physics distance joint.
/// </summary>
public sealed partial class PullingSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private AlertsSystem _alertsSystem = default!;
    [Dependency] private MovementSpeedModifierSystem _modifierSystem = default!;
    [Dependency] private SharedJointSystem _joints = default!;
    [Dependency] private SharedContainerSystem _containerSystem = default!;
    [Dependency] private SharedHandsSystem _handsSystem = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private HeldSpeedModifierSystem _clothingMoveSpeed = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedVirtualItemSystem _virtual = default!;
    // Goobstation start
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private SharedColorFlashEffectSystem _color = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private INetManager _netManager = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private GrabThrownSystem _grabThrown = default!;
    [Dependency] private SharedCombatModeSystem _combatMode = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    // Goobstation end

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(SharedPhysicsSystem));
        UpdatesOutsidePrediction = true;

        SubscribeLocalEvent<PullableComponent, MoveInputEvent>(OnPullableMoveInput);
        SubscribeLocalEvent<PullableComponent, CollisionChangeEvent>(OnPullableCollisionChange);
        SubscribeLocalEvent<PullableComponent, JointRemovedEvent>(OnJointRemoved);
        SubscribeLocalEvent<PullableComponent, GetVerbsEvent<Verb>>(AddPullVerbs);
        SubscribeLocalEvent<PullableComponent, EntGotInsertedIntoContainerMessage>(OnPullableContainerInsert);
        SubscribeLocalEvent<PullableComponent, ModifyUncuffDurationEvent>(OnModifyUncuffDuration);
        SubscribeLocalEvent<PullableComponent, StopBeingPulledAlertEvent>(OnStopBeingPulledAlert);
        SubscribeLocalEvent<PullableComponent, GetInteractingEntitiesEvent>(OnGetInteractingEntities);

        SubscribeLocalEvent<PullerComponent, MobStateChangedEvent>(OnStateChanged, after: [typeof(MobThresholdSystem)]);
        SubscribeLocalEvent<PullerComponent, AfterAutoHandleStateEvent>(OnAfterState);
        SubscribeLocalEvent<PullerComponent, EntGotInsertedIntoContainerMessage>(OnPullerContainerInsert);
        SubscribeLocalEvent<PullerComponent, EntityUnpausedEvent>(OnPullerUnpaused);
        SubscribeLocalEvent<PullerComponent, VirtualItemDeletedEvent>(OnVirtualItemDeleted);
        SubscribeLocalEvent<PullerComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
        SubscribeLocalEvent<PullerComponent, DropHandItemsEvent>(OnDropHandItems);
        SubscribeLocalEvent<PullerComponent, StopPullingAlertEvent>(OnStopPullingAlert);

        SubscribeLocalEvent<HandsComponent, PullStartedMessage>(HandlePullStarted);
        SubscribeLocalEvent<HandsComponent, PullStoppedMessage>(HandlePullStopped);

        SubscribeLocalEvent<PullableComponent, StrappedEvent>(OnBuckled);
        SubscribeLocalEvent<PullableComponent, BuckledEvent>(OnGotBuckled);
        SubscribeLocalEvent<ActivePullerComponent, TargetHandcuffedEvent>(OnTargetHandcuffed);

        SubscribeLocalEvent<PullableComponent, UpdateCanMoveEvent>(OnGrabbedMoveAttempt); // Goobstation
        SubscribeLocalEvent<PullableComponent, SpeakAttemptEvent>(OnGrabbedSpeakAttempt); // Goobstation
        SubscribeLocalEvent<PullerComponent, BeforeThrowEvent>(OnPullerBeforeThrow); // Goobstation - Grab Intent
        SubscribeLocalEvent<PullerComponent, VirtualItemDropAttemptEvent>(OnVirtualItemDropAttempt); // Goobstation - Grab Intent
        SubscribeLocalEvent<PullerComponent, AddCuffDoAfterEvent>(OnAddCuffDoAfterEvent); // Goobstation - Grab Intent
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.ReleasePulledObject, InputCmdHandler.FromDelegate(OnReleasePulledObject, handle: false))
            .Register<PullingSystem>();
    }

    // Goobstation - Grab Intent
    private void OnAddCuffDoAfterEvent(Entity<PullerComponent> ent, ref AddCuffDoAfterEvent args)
    {
        if (args.Handled)
            return;

        if (!args.Cancelled
            && TryComp<PullableComponent>(ent.Comp.Pulling, out var comp)
            && ent.Comp.Pulling != null)
        {
            if (_netManager.IsServer)
                StopPulling((EntityUid)ent.Comp.Pulling, comp);
        }
    }
    // Goobstation


    private void OnTargetHandcuffed(Entity<ActivePullerComponent> ent, ref TargetHandcuffedEvent args)
    {
        if (!TryComp<PullerComponent>(ent, out var comp))
            return;

        if (comp.Pulling == null)
            return;

        if (CanPull(ent, comp.Pulling.Value, comp))
            return;

        if (!TryComp<PullableComponent>(comp.Pulling, out var pullableComp))
            return;

        TryStopPull(comp.Pulling.Value, pullableComp);
    }

    private void HandlePullStarted(EntityUid uid, HandsComponent component, PullStartedMessage args)
    {
        if (args.PullerUid != uid)
            return;

        if (TryComp(args.PullerUid, out PullerComponent? pullerComp) && !pullerComp.NeedsHands)
            return;

        if (!_virtual.TrySpawnVirtualItemInHand(args.PulledUid, uid))
        {
            DebugTools.Assert("Unable to find available hand when starting pulling??");
        }
    }

    private void HandlePullStopped(EntityUid uid, HandsComponent component, PullStoppedMessage args)
    {
        if (args.PullerUid != uid)
            return;

        // Try find hand that is doing this pull.
        // and clear it.
        _virtual.DeleteInHandsMatching(uid, args.PulledUid);  // Goobstation
    }

    private void OnStateChanged(EntityUid uid, PullerComponent component, ref MobStateChangedEvent args)
    {
        if (component.Pulling == null)
            return;

        if (TryComp<PullableComponent>(component.Pulling, out var comp) && (args.NewMobState == MobState.Critical || args.NewMobState == MobState.Dead))
        {
            TryStopPull(component.Pulling.Value, comp);
        }
    }

    private void OnBuckled(Entity<PullableComponent> ent, ref StrappedEvent args)
    {
        // Prevent people from pulling the entity they are buckled to
        if (ent.Comp.Puller == args.Buckle.Owner && !args.Buckle.Comp.PullStrap)
            StopPulling(ent, ent);
    }

    private void OnGotBuckled(Entity<PullableComponent> ent, ref BuckledEvent args)
    {
        StopPulling(ent, ent);
    }

    private void OnGetInteractingEntities(Entity<PullableComponent> ent, ref GetInteractingEntitiesEvent args)
    {
        if (ent.Comp.Puller != null)
            args.InteractingEntities.Add(ent.Comp.Puller.Value);
    }

    private void OnAfterState(Entity<PullerComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent.Comp.Pulling == null)
            RemComp<ActivePullerComponent>(ent.Owner);
        else
            EnsureComp<ActivePullerComponent>(ent.Owner);
    }

    private void OnDropHandItems(EntityUid uid, PullerComponent pullerComp, DropHandItemsEvent args)
    {
        if (pullerComp.Pulling == null || pullerComp.NeedsHands)
            return;

        if (!TryComp(pullerComp.Pulling, out PullableComponent? pullableComp))
            return;

        TryStopPull(pullerComp.Pulling.Value, pullableComp, uid);
    }

    private void OnStopPullingAlert(Entity<PullerComponent> ent, ref StopPullingAlertEvent args)
    {
        if (args.Handled)
            return;
        if (!TryComp<PullableComponent>(ent.Comp.Pulling, out var pullable))
            return;
        args.Handled = TryStopPull(ent.Comp.Pulling.Value, pullable, ent.Owner, ignoreGrab: true);
    }

    private void OnPullerContainerInsert(Entity<PullerComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (ent.Comp.Pulling == null)
            return;

        if (!TryComp(ent.Comp.Pulling.Value, out PullableComponent? pulling))
            return;

        // Goobstation - Grab Intent
        foreach (var item in ent.Comp.GrabVirtualItems)
            QueueDel(item);

        TryStopPull(ent.Comp.Pulling.Value, pulling, ent.Owner, true);
        // Goobstation
    }

    private void OnPullableContainerInsert(Entity<PullableComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        TryStopPull(ent.Owner, ent.Comp, ignoreGrab: true); // Goobstation
    }

    private void OnModifyUncuffDuration(Entity<PullableComponent> ent, ref ModifyUncuffDurationEvent args)
    {
        if (!ent.Comp.BeingPulled)
            return;

        // We don't care if the person is being uncuffed by someone else
        if (args.User != args.Target)
            return;

        args.Duration *= 2;
    }

    private void OnStopBeingPulledAlert(Entity<PullableComponent> ent, ref StopBeingPulledAlertEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryStopPull(ent, ent, ent);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        CommandBinds.Unregister<PullingSystem>();
    }

    private void OnPullerUnpaused(EntityUid uid, PullerComponent component, ref EntityUnpausedEvent args)
    {
        component.NextThrow += args.PausedTime;
    }

    // Grab intent - ОХ БЛЯТЬ
    private void OnPullerBeforeThrow(EntityUid uid, PullerComponent component, ref BeforeThrowEvent args)
    {
        if (uid != args.PlayerUid || component.Pulling == null)
            return;

        if (!TryComp<VirtualItemComponent>(args.ItemUid, out var virtItem) || virtItem.BlockingEntity != component.Pulling)
            return;

        args.Cancelled = true;

        if (component.GrabStage <= GrabStage.Soft)
        {
            TryLowerGrabStage(component.Pulling.Value, uid);
            return;
        }

        if (!TryComp(component.Pulling.Value, out PullableComponent? pullable))
            return;

        TryThrowGrabbed((uid, component), (component.Pulling.Value, pullable), args.Direction);
    }

    private void TryThrowGrabbed(
        Entity<PullerComponent> puller,
        Entity<PullableComponent> pullable,
        Vector2 direction)
    {
        if (_timing.CurTime < puller.Comp.NextStageChange)
            return;

        if (!_combatMode.IsInCombatMode(puller.Owner) ||
            HasComp<GrabThrownComponent>(pullable.Owner) ||
            puller.Comp.GrabStage <= GrabStage.Soft)
            return;

        var pullablePos = _transform.ToMapCoordinates(Transform(pullable).Coordinates).Position;
        var pullerPos = _transform.GetWorldPosition(puller.Owner);
        var vecBetween = pullablePos - pullerPos;

        var dirAngle = direction.ToWorldAngle().Degrees;
        var betweenAngle = vecBetween.ToWorldAngle().Degrees;

        var angle = dirAngle - betweenAngle;
        if (angle < 0)
            angle = -angle;

        var maxDistance = 3f;
        var damageModifier = 1f;

        if (angle < 30)
        {
            damageModifier = 0.3f;
            maxDistance = 1f;
        }
        else if (angle < 90)
        {
            damageModifier = 0.7f;
            maxDistance = 1.5f;
        }
        else
        {
            maxDistance = 2.25f;
        }

        var distance = Math.Clamp(direction.Length(), 0.5f, maxDistance);
        if (direction != Vector2.Zero)
            direction *= distance / direction.Length();

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 5);
        damage *= damageModifier;

        const float throwbackforce = 0.15f;
        TryStopPull(pullable, pullable.Comp, puller.Owner, ignoreGrab: true);
        _grabThrown.Throw(
            pullable.Owner,
            puller.Owner,
            direction * 2f,
            120f,
            damage * puller.Comp.GrabThrowDamageModifier,
            damage * puller.Comp.GrabThrowDamageModifier);
        _throwing.TryThrow(puller.Owner, -direction * throwbackforce);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg"), puller.Owner);
        puller.Comp.NextStageChange = _timing.CurTime + TimeSpan.FromSeconds(2f);
        Dirty(puller);
    }

    private void OnVirtualItemDropAttempt(EntityUid uid, PullerComponent component, VirtualItemDropAttemptEvent args)
    {
        if (component.Pulling == null)
            return;

        if (component.Pulling != args.BlockingEntity)
            return;

        if (!args.Throw)
        {
            if (component.GrabStage > GrabStage.No
                && TryComp(args.BlockingEntity, out PullableComponent? comp))
            {
                TryLowerGrabStage(component.Pulling.Value, uid);
                args.Cancel();  // VirtualItem is NOT being deleted
            }
        }
        else
        {
            if (component.GrabStage <= GrabStage.Soft)
            {
                TryLowerGrabStage(component.Pulling.Value, uid);
                args.Cancel();  // VirtualItem is NOT being deleted
            }
        }
    }
    // grab intent end

    private void OnVirtualItemDeleted(EntityUid uid, PullerComponent component, VirtualItemDeletedEvent args)
    {
        // If client deletes the virtual hand then stop the pull.
        if (component.Pulling == null)
            return;

        if (component.Pulling != args.BlockingEntity)
            return;

        if (TryComp(args.BlockingEntity, out PullableComponent? comp)) // Goobstation
        {
            TryLowerGrabStage(component.Pulling.Value, uid);// Goobstation
        }
    }

    private void AddPullVerbs(EntityUid uid, PullableComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Are they trying to pull themselves up by their bootstraps?
        if (args.User == args.Target)
            return;

        //TODO VERB ICONS add pulling icon
        if (component.Puller == args.User)
        {
            Verb verb = new()
            {
                Text = Loc.GetString("pulling-verb-get-data-text-stop-pulling"),
                Act = () => TryStopPull(uid, component, user: args.User, ignoreGrab: true),  // Goobstation
                DoContactInteraction = false // pulling handle its own contact interaction.
            };
            args.Verbs.Add(verb);
        }
        else if (CanPull(args.User, args.Target))
        {
            Verb verb = new()
            {
                Text = Loc.GetString("pulling-verb-get-data-text"),
                Act = () => TryStartPull(args.User, args.Target),
                DoContactInteraction = false // pulling handle its own contact interaction.
            };
            args.Verbs.Add(verb);
        }
    }

    private void OnRefreshMovespeed(EntityUid uid, PullerComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (TryComp<HeldSpeedModifierComponent>(component.Pulling, out var itemHeldSpeed) && component.Pulling.HasValue)
        {
            var (walkMod, sprintMod) =
                _clothingMoveSpeed.GetHeldMovementSpeedModifiers(component.Pulling.Value, itemHeldSpeed);
            args.ModifySpeed(walkMod, sprintMod);
        }

        // Goobstation start
        if (TryComp<HeldSpeedModifierComponent>(component.Pulling, out var heldMoveSpeed) && component.Pulling.HasValue)
        {
            var (walkMod, sprintMod) = (args.WalkSpeedModifier, args.SprintSpeedModifier);

            switch (component.GrabStage)
            {
                case GrabStage.No:
                    args.ModifySpeed(walkMod, sprintMod);
                    break;
                case GrabStage.Soft:
                    args.ModifySpeed(walkMod * 0.9f, sprintMod * 0.9f);
                    break;
                case GrabStage.Hard:
                    args.ModifySpeed(walkMod * 0.7f, sprintMod * 0.7f);
                    break;
                case GrabStage.Suffocate:
                    args.ModifySpeed(walkMod * 0.4f, sprintMod * 0.4f);
                    break;
                default:
                    args.ModifySpeed(walkMod, sprintMod);
                    break;
            }
            // Goobstation end
            return;
        }

        // Goobstation start
        switch (component.GrabStage)
        {
            case GrabStage.No:
                args.ModifySpeed(component.WalkSpeedModifier, component.SprintSpeedModifier);
                break;
            case GrabStage.Soft:
                args.ModifySpeed(component.WalkSpeedModifier * 0.9f, component.SprintSpeedModifier * 0.9f);
                break;
            case GrabStage.Hard:
                args.ModifySpeed(component.WalkSpeedModifier * 0.7f, component.SprintSpeedModifier * 0.7f);
                break;
            case GrabStage.Suffocate:
                args.ModifySpeed(component.WalkSpeedModifier * 0.4f, component.SprintSpeedModifier * 0.4f);
                break;
            default:
                args.ModifySpeed(component.WalkSpeedModifier, component.SprintSpeedModifier);
                break;
                // Goobstation end
        }
    }

    private void OnPullableMoveInput(EntityUid uid, PullableComponent component, ref MoveInputEvent args)
    {
        // If someone moves then break their pulling.
        if (!component.BeingPulled || !component.BreakOnMove) // Erida BreakOnMove
            return;

        var entity = args.Entity;

        if (!_blocker.CanMove(entity))
            return;

        TryStopPull(uid, component, user: uid);
    }

    private void OnPullableCollisionChange(EntityUid uid, PullableComponent component, ref CollisionChangeEvent args)
    {
        // IDK what this is supposed to be.
        if (!_timing.ApplyingState && component.PullJointId != null && !args.CanCollide)
        {
            _joints.RemoveJoint(uid, component.PullJointId);
        }
    }

    private void OnJointRemoved(EntityUid uid, PullableComponent component, JointRemovedEvent args)
    {
        // Just handles the joint getting nuked without going through pulling system (valid behavior).

        // Not relevant / pullable state handle it.
        if (component.Puller != args.OtherEntity ||
            args.Joint.ID != component.PullJointId ||
            _timing.ApplyingState)
        {
            return;
        }

        if (args.Joint.ID != component.PullJointId || component.Puller == null)
            return;

        StopPulling(uid, component);
    }

    /// <summary>
    /// Forces pulling to stop and handles cleanup.
    /// </summary>
    private void StopPulling(EntityUid pullableUid, PullableComponent pullableComp)
    {

        if (!_timing.ApplyingState)
        {
            // Joint shutdown
            if (pullableComp.PullJointId != null)
            {
                _joints.RemoveJoint(pullableUid, pullableComp.PullJointId);
                pullableComp.PullJointId = null;
            }

            if (TryComp<PhysicsComponent>(pullableUid, out var pullablePhysics))
            {
                _physics.SetFixedRotation(pullableUid, pullableComp.PrevFixedRotation, body: pullablePhysics);
            }
        }

        var oldPuller = pullableComp.Puller;
        if (oldPuller != null)
            RemComp<ActivePullerComponent>(oldPuller.Value);

        pullableComp.PullJointId = null;
        pullableComp.Puller = null;

        // Goobstation - Grab Intent
        pullableComp.GrabStage = GrabStage.No;
        pullableComp.GrabEscapeChance = 1f;
        _blocker.UpdateCanMove(pullableUid);
        // Goobstation

        Dirty(pullableUid, pullableComp);

        // No more joints with puller -> force stop pull.
        if (TryComp<PullerComponent>(oldPuller, out var pullerComp))
        {
            var pullerUid = oldPuller.Value;
            if (_netManager.IsServer) // Goobstation
                _alertsSystem.ClearAlert(pullerUid, pullerComp.PullingAlert);
            pullerComp.Pulling = null;

            // Goobstation - Grab Intent
            pullerComp.GrabStage = GrabStage.No;
            pullerComp.NextSuffocateDamage = TimeSpan.Zero;
            _virtual.DeleteInHandsMatching(pullerUid, pullableUid);
            pullerComp.GrabVirtualItems.Clear();
            // Goobstation

            Dirty(oldPuller.Value, pullerComp);

            // Messaging
            var message = new PullStoppedMessage(pullerUid, pullableUid);
            _modifierSystem.RefreshMovementSpeedModifiers(pullerUid);
            _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(pullerUid):user} stopped pulling {ToPrettyString(pullableUid):target}");

            RaiseLocalEvent(pullerUid, message);
            RaiseLocalEvent(pullableUid, message);
        }

        if (_netManager.IsServer) // Goobstation
            _alertsSystem.ClearAlert(pullableUid, pullableComp.PulledAlert);
    }

    public bool IsPulled(EntityUid uid, PullableComponent? component = null)
    {
        return Resolve(uid, ref component, false) && component.BeingPulled;
    }

    public bool IsPulling(EntityUid puller, PullerComponent? component = null)
    {
        return Resolve(puller, ref component, false) && component.Pulling != null;
    }

    public EntityUid? GetPuller(EntityUid puller, PullableComponent? component = null)
    {
        return !Resolve(puller, ref component, false) ? null : component.Puller;
    }

    public EntityUid? GetPulling(EntityUid puller, PullerComponent? component = null)
    {
        return !Resolve(puller, ref component, false) ? null : component.Pulling;
    }

    private void OnReleasePulledObject(ICommonSession? session)
    {
        if (session?.AttachedEntity is not { Valid: true } player)
        {
            return;
        }

        if (!TryComp(player, out PullerComponent? pullerComp) ||
            !TryComp(pullerComp.Pulling, out PullableComponent? pullableComp))
        {
            return;
        }

        TryStopPull(pullerComp.Pulling.Value, pullableComp, user: player, true); // Goobstation
    }

    public bool CanPull(EntityUid puller, EntityUid pullableUid, PullerComponent? pullerComp = null)
    {
        if (!Resolve(puller, ref pullerComp, false))
        {
            return false;
        }

        if (pullerComp.NeedsHands
            && !_handsSystem.TryGetEmptyHand(puller, out _)
            && pullerComp.Pulling == null)
        {
            return false;
        }

        if (!_blocker.CanInteract(puller, pullableUid))
        {
            return false;
        }

        if (!TryComp<PhysicsComponent>(pullableUid, out var physics))
        {
            return false;
        }

        if (physics.BodyType == BodyType.Static)
        {
            return false;
        }

        if (puller == pullableUid)
        {
            return false;
        }

        if (!_containerSystem.IsInSameOrNoContainer(puller, pullableUid))
        {
            return false;
        }

        var getPulled = new BeingPulledAttemptEvent(puller, pullableUid);
        RaiseLocalEvent(pullableUid, getPulled, true);
        var startPull = new StartPullAttemptEvent(puller, pullableUid);
        RaiseLocalEvent(puller, startPull, true);
        return !startPull.Cancelled && !getPulled.Cancelled;
    }

    // Goobstation - Grab Intent
    public bool TogglePull(Entity<PullableComponent?> pullable, EntityUid pullerUid)
    {
        if (!Resolve(pullable, ref pullable.Comp, false))
            return false;

        if (pullable.Comp.Puller != pullerUid)
            return TryStartPull(pullerUid, pullable, pullableComp: pullable.Comp);

        if (TryGrab((pullable, pullable.Comp), pullerUid))
            return true;

        if (!_combatMode.IsInCombatMode(pullable))
            return TryStopPull(pullable, pullable.Comp, ignoreGrab: true);

        return false;
    }

    public bool TogglePull(EntityUid pullerUid, PullerComponent puller)
    {
        if (!TryComp<PullableComponent>(puller.Pulling, out var pullable))
            return false;

        return TogglePull((puller.Pulling.Value, pullable), pullerUid);
    }

    public bool TryStartPull(EntityUid pullerUid, EntityUid pullableUid,
        PullerComponent? pullerComp = null, PullableComponent? pullableComp = null)
    {
        if (!Resolve(pullerUid, ref pullerComp, false) ||
            !Resolve(pullableUid, ref pullableComp, false))
        {
            return false;
        }

        if (pullerComp.Pulling == pullableUid)
            return true;

        if (!CanPull(pullerUid, pullableUid))
            return false;

        if (!TryComp(pullerUid, out PhysicsComponent? pullerPhysics) || !TryComp(pullableUid, out PhysicsComponent? pullablePhysics))
            return false;

        // Ensure that the puller is not currently pulling anything.
        if (TryComp<PullableComponent>(pullerComp.Pulling, out var oldPullable)
            && !TryStopPull(pullerComp.Pulling.Value, oldPullable, pullerUid, true)) // Goobstation
            return false;

        // Stop anyone else pulling the entity we want to pull
        if (pullableComp.Puller != null)
        {
            // We're already pulling this item
            if (pullableComp.Puller == pullerUid)
                return false;

            // Goobstation - Grab Intent
            if (!TryStopPull(pullableUid, pullableComp, pullableComp.Puller))
            {
                // Not succeed to retake grabbed entity
                if (_netManager.IsServer)
                {
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-fail",
                            ("puller", Identity.Entity(pullableComp.Puller.Value, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullerUid, pullerUid, PopupType.MediumCaution);
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-fail-puller",
                            ("puller", Identity.Entity(pullerUid, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullableComp.Puller.Value, pullableComp.Puller.Value, PopupType.MediumCaution);
                }
                return false;
            }

            else if (pullableComp.GrabStage != GrabStage.No)
            {
                // Successful retake
                if (_netManager.IsServer)
                {
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-success",
                            ("puller", Identity.Entity(pullableComp.Puller.Value, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullerUid, pullerUid, PopupType.MediumCaution);
                    _popup.PopupEntity(Loc.GetString("popup-grab-retake-success-puller",
                            ("puller", Identity.Entity(pullerUid, EntityManager)),
                            ("pulled", Identity.Entity(pullableUid, EntityManager))),
                        pullableComp.Puller.Value, pullableComp.Puller.Value, PopupType.MediumCaution);
                }
            }
            // Goobstation
        }

        var pullAttempt = new PullAttemptEvent(pullerUid, pullableUid);
        RaiseLocalEvent(pullerUid, pullAttempt);

        if (pullAttempt.Cancelled)
            return false;

        RaiseLocalEvent(pullableUid, pullAttempt);

        if (pullAttempt.Cancelled)
            return false;

        // Pulling confirmed

        _interaction.DoContactInteraction(pullableUid, pullerUid);

        // Use net entity so it's consistent across client and server.
        pullableComp.PullJointId = $"pull-joint-{GetNetEntity(pullableUid)}";

        EnsureComp<ActivePullerComponent>(pullerUid);
        pullerComp.Pulling = pullableUid;
        pullableComp.Puller = pullerUid;

        // store the pulled entity's physics FixedRotation setting in case we change it
        pullableComp.PrevFixedRotation = pullablePhysics.FixedRotation;

        // joint state handling will manage its own state
        if (!_timing.ApplyingState)
        {
            var joint = _joints.CreateDistanceJoint(pullableUid, pullerUid,
                    pullablePhysics.LocalCenter, pullerPhysics.LocalCenter,
                    id: pullableComp.PullJointId);
            joint.CollideConnected = false;
            // This maximum has to be there because if the object is constrained too closely, the clamping goes backwards and asserts.
            // Internally, the joint length has been set to the distance between the pivots.
            // Add an additional 15cm (pretty arbitrary) to the maximum length for the hard limit.
            joint.MaxLength = joint.Length + 0.15f;
            joint.MinLength = 0f;
            // Set the spring stiffness to zero. The joint won't have any effect provided
            // the current length is beteen MinLength and MaxLength. At those limits, the
            // joint will have infinite stiffness.
            joint.Stiffness = 0f;

            _physics.SetFixedRotation(pullableUid, pullableComp.FixedRotationOnPull, body: pullablePhysics);
        }

        // Messaging
        var message = new PullStartedMessage(pullerUid, pullableUid);
        _modifierSystem.RefreshMovementSpeedModifiers(pullerUid);
        _alertsSystem.ShowAlert(pullerUid, pullerComp.PullingAlert, 0); // Goobstation
        _alertsSystem.ShowAlert(pullableUid, pullableComp.PulledAlert, 0); // Goobstation

        RaiseLocalEvent(pullerUid, message);
        RaiseLocalEvent(pullableUid, message);

        Dirty(pullerUid, pullerComp);
        Dirty(pullableUid, pullableComp);

        var pullingMessage =
            Loc.GetString("getting-pulled-popup", ("puller", Identity.Entity(pullerUid, EntityManager)));
        _popup.PopupEntity(pullingMessage, pullableUid, pullableUid);

        _adminLogger.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(pullerUid):user} started pulling {ToPrettyString(pullableUid):target}");

        if (_combatMode.IsInCombatMode(pullerUid)) // Goobstation
            TryGrab(pullableUid, pullerUid); // Goobstation

        return true;
    }

    public bool TryStopPull(EntityUid pullableUid, PullableComponent pullable, EntityUid? user = null, bool ignoreGrab = false)
    {
        var pullerUidNull = pullable.Puller;

        if (pullerUidNull == null)
            return true;

        var msg = new AttemptStopPullingEvent(user);
        RaiseLocalEvent(pullableUid, ref msg, true);

        if (msg.Cancelled)
            return false;

        // Goobstation - Grab Intent
        if (!ignoreGrab)
        {
            if (_netManager.IsServer && user != null && user.Value == pullableUid)
            {
                var releaseAttempt = AttemptGrabRelease(pullableUid);
                if (!releaseAttempt)
                {
                    _popup.PopupEntity(Loc.GetString("popup-grab-release-fail-self"),
                        pullableUid,
                        pullableUid,
                        PopupType.SmallCaution);
                    return false;
                }

                _popup.PopupEntity(Loc.GetString("popup-grab-release-success-self"),
                    pullableUid,
                    pullableUid,
                    PopupType.SmallCaution);
                _popup.PopupEntity(
                    Loc.GetString("popup-grab-release-success-puller",
                        ("target", Identity.Entity(pullableUid, EntityManager))),
                    pullerUidNull.Value,
                    pullerUidNull.Value,
                    PopupType.MediumCaution);
            }
        }
        // Goobstation

        StopPulling(pullableUid, pullable);
        return true;
    }

    /// <summary>
    /// Copies compatible datafields of <see cref="PullerComponent"/> onto the target entity.
    /// </summary>
    /// <param name="source">The entity who's component will be taken.</param>
    /// <param name="target">The entity to apply it to.</param>
    public void CopyPullerComponent(Entity<PullerComponent?> source, EntityUid target)
    {
        if (!Resolve(source, ref source.Comp))
            return;

        var targetComp = EnsureComp<PullerComponent>(target);
        targetComp.ThrowCooldown = source.Comp.ThrowCooldown;
        targetComp.NeedsHands = source.Comp.NeedsHands;
        targetComp.PullingAlert = source.Comp.PullingAlert;
        Dirty(target, targetComp);
    }

    // Gobstation start
    public void StopAllPulls(EntityUid uid, bool stopPullable = true, bool stopPuller = true) // Goobstation
    {
        if (stopPullable && TryComp<PullableComponent>(uid, out var pullable) && IsPulled(uid, pullable))
            TryStopPull(uid, pullable);

        if (stopPuller && TryComp<PullerComponent>(uid, out var puller) &&
            TryComp(puller.Pulling, out PullableComponent? pullableEnt))
            TryStopPull(puller.Pulling.Value, pullableEnt);
    }
    // Gobstation end

    // Goobstation start
    /// <summary>
    /// Trying to grab the target
    /// </summary>
    /// <param name="pullable">Target that would be grabbed</param>
    /// <param name="puller">Performer of the grab</param>
    /// <param name="ignoreCombatMode">If true, will ignore disabled combat mode</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public bool TryGrab(Entity<PullableComponent?> pullable, Entity<PullerComponent?> puller, bool ignoreCombatMode = false)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp))
            return false;

        if (!Resolve(puller.Owner, ref puller.Comp))
            return false;

        if (pullable.Comp.Puller != puller.Owner ||
            puller.Comp.Pulling != pullable.Owner)
            return false;

        if (puller.Comp.NextStageChange > _timing.CurTime)
            return true;

        // You can't choke crates
        if (!HasComp<MobStateComponent>(pullable))
            return false;

        // Delay to avoid spamming
        puller.Comp.NextStageChange = _timing.CurTime + puller.Comp.StageChangeCooldown;
        Dirty(puller);

        // Don't grab without grab intent
        if (!ignoreCombatMode)
            if (!_combatMode.IsInCombatMode(puller.Owner))
                return false;


        // It's blocking stage update, maybe better UX?
        if (puller.Comp.GrabStage == GrabStage.Suffocate)
        {
            ApplySuffocateGrabDamage((puller.Owner, puller.Comp!), pullable.Owner);
            return true;
        }

        // Update stage
        // TODO: Change grab stage direction
        var nextStageAddition = puller.Comp.GrabStageDirection switch
        {
            GrabStageDirection.Increase => 1,
            GrabStageDirection.Decrease => -1,
            _ => throw new ArgumentOutOfRangeException(),
        };

        var newStage = puller.Comp.GrabStage + nextStageAddition;

        if (!TrySetGrabStages((puller.Owner, puller.Comp), (pullable.Owner, pullable.Comp), newStage))
            return false;

        _color.RaiseEffect(Color.Yellow, new List<EntityUid> { pullable }, Filter.Pvs(pullable, entityManager: EntityManager));
        return true;
    }

    private bool TrySetGrabStages(Entity<PullerComponent> puller, Entity<PullableComponent> pullable, GrabStage stage)
    {
        puller.Comp.GrabStage = stage;
        pullable.Comp.GrabStage = stage;

        if (!TryUpdateGrabVirtualItems(puller, pullable))
            return false;

        var filter = Filter.Empty()
            .AddPlayersByPvs(Transform(puller).Coordinates)
            .RemovePlayerByAttachedEntity(puller.Owner)
            .RemovePlayerByAttachedEntity(pullable.Owner);

        var popupType = stage switch
        {
            GrabStage.No => PopupType.Small,
            GrabStage.Soft => PopupType.Small,
            GrabStage.Hard => PopupType.MediumCaution,
            GrabStage.Suffocate => PopupType.LargeCaution,
            _ => throw new ArgumentOutOfRangeException()
        };

        pullable.Comp.GrabEscapeChance = puller.Comp.EscapeChances[stage];

        puller.Comp.NextSuffocateDamage = stage == GrabStage.Suffocate
            ? _timing.CurTime
            : TimeSpan.Zero;

        _alertsSystem.ShowAlert(puller.Owner, puller.Comp.PullingAlert, puller.Comp.PullingAlertSeverity[stage]);
        _alertsSystem.ShowAlert(pullable.Owner, pullable.Comp.PulledAlert, pullable.Comp.PulledAlertAlertSeverity[stage]);

        _blocker.UpdateCanMove(pullable);
        _modifierSystem.RefreshMovementSpeedModifiers(puller);

        // I'm lazy to write client code
        if (!_netManager.IsServer)
            return true;

        _popup.PopupEntity(Loc.GetString($"popup-grab-{puller.Comp.GrabStage.ToString().ToLower()}-target", ("puller", Identity.Entity(puller, EntityManager))), pullable, pullable, popupType);
        _popup.PopupEntity(Loc.GetString($"popup-grab-{puller.Comp.GrabStage.ToString().ToLower()}-self", ("target", Identity.Entity(pullable, EntityManager))), pullable, puller, PopupType.Medium);
        _popup.PopupEntity(Loc.GetString($"popup-grab-{puller.Comp.GrabStage.ToString().ToLower()}-others", ("target", Identity.Entity(pullable, EntityManager)), ("puller", Identity.Entity(puller, EntityManager))), pullable, filter, true, popupType);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg"), pullable);

        Dirty(pullable);
        Dirty(puller);

        return true;
    }

    private bool TryUpdateGrabVirtualItems(Entity<PullerComponent> puller, Entity<PullableComponent> pullable)
    {
        // Updating virtual items
        var virtualItemsCount = puller.Comp.GrabVirtualItems.Count;

        var newVirtualItemsCount = puller.Comp.NeedsHands ? 0 : 1;
        if (puller.Comp.GrabVirtualItemStageCount.TryGetValue(puller.Comp.GrabStage, out var count))
            newVirtualItemsCount += count;

        if (virtualItemsCount != newVirtualItemsCount)
        {
            var delta = newVirtualItemsCount - virtualItemsCount;

            // Adding new virtual items
            if (delta > 0)
            {
                for (var i = 0; i < delta; i++)
                {
                    var emptyHand = _handsSystem.TryGetEmptyHand(puller.Owner, out _);
                    if (!emptyHand)
                    {
                        if (_netManager.IsServer)
                            _popup.PopupEntity(Loc.GetString("popup-grab-need-hand"), puller, puller, PopupType.Medium);

                        return false;
                    }

                    if (!_virtual.TrySpawnVirtualItemInHand(pullable, puller.Owner, out var item, true))
                    {
                        // I'm lazy write client code
                        if (_netManager.IsServer)
                            _popup.PopupEntity(Loc.GetString("popup-grab-need-hand"), puller, puller, PopupType.Medium);

                        return false;
                    }

                    puller.Comp.GrabVirtualItems.Add(item.Value);
                }
            }

            if (delta < 0)
            {
                for (var i = 0; i < Math.Abs(delta); i++)
                {
                    if (i >= puller.Comp.GrabVirtualItems.Count)
                        break;

                    var item = puller.Comp.GrabVirtualItems[i];
                    puller.Comp.GrabVirtualItems.Remove(item);
                    QueueDel(item);
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Attempts to release entity from grab
    /// </summary>
    /// <param name="pullable">Grabbed entity</param>
    /// <returns></returns>
    public bool AttemptGrabRelease(Entity<PullableComponent?> pullable)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp))
            return false;

        if (_timing.CurTime < pullable.Comp.NextEscapeAttempt)  // No autoclickers! Mwa-ha-ha
            return false;

        if (_random.Prob(pullable.Comp.GrabEscapeChance))
            return true;

        pullable.Comp.NextEscapeAttempt = _timing.CurTime.Add(TimeSpan.FromSeconds(1));
        Dirty(pullable.Owner, pullable.Comp);
        return false;
    }

    private void OnGrabbedMoveAttempt(EntityUid uid, PullableComponent component, UpdateCanMoveEvent args)
    {
        if (component.GrabStage == GrabStage.No)
            return;

        args.Cancel();

    }

    private void OnGrabbedSpeakAttempt(EntityUid uid, PullableComponent component, SpeakAttemptEvent args)
    {
        if (component.GrabStage != GrabStage.Suffocate)
            return;

        _popup.PopupEntity(Loc.GetString("popup-grabbed-cant-speak"), uid, uid, PopupType.MediumCaution);   // You cant speak while someone is choking you

        args.Cancel();
    }

    /// <summary>
    /// Tries to lower grab stage for target or release it
    /// </summary>
    /// <param name="pullable">Grabbed entity</param>
    /// <param name="puller">Performer</param>
    /// <param name="ignoreCombatMode">If true, will NOT release target if combat mode is off</param>
    /// <returns></returns>
    public bool TryLowerGrabStage(Entity<PullableComponent?> pullable, Entity<PullerComponent?> puller, bool ignoreCombatMode = false)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp))
            return false;

        if (!Resolve(puller.Owner, ref puller.Comp))
            return false;

        if (pullable.Comp.Puller != puller.Owner ||
            puller.Comp.Pulling != pullable.Owner)
            return false;

        pullable.Comp.NextEscapeAttempt = _timing.CurTime.Add(TimeSpan.FromSeconds(1f));
        Dirty(pullable);

        if (!ignoreCombatMode && !_combatMode.IsInCombatMode(puller.Owner))
        {
            TryStopPull(pullable, pullable.Comp, puller.Owner, ignoreGrab: true);
            return true;
        }

        if (puller.Comp.GrabStage == GrabStage.No)
        {
            TryStopPull(pullable, pullable.Comp, puller.Owner, ignoreGrab: true);
            return true;
        }

        var newStage = puller.Comp.GrabStage - 1;
        TrySetGrabStages((puller.Owner, puller.Comp), (pullable.Owner, pullable.Comp), newStage);
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_netManager.IsServer)
            return;

        var time = _timing.CurTime;
        var query = EntityQueryEnumerator<PullerComponent>();
        while (query.MoveNext(out var pullerUid, out var puller))
        {
            if (puller.Pulling is not { } pulled || puller.GrabStage != GrabStage.Suffocate)
                continue;

            if (time < puller.NextSuffocateDamage)
                continue;

            if (!TryComp<MobStateComponent>(pulled, out var mobState)
                || mobState.CurrentState == MobState.Dead)
            {
                continue;
            }

            puller.NextSuffocateDamage = time + puller.SuffocateGrabDamageInterval;
            Dirty(pullerUid, puller);
            ApplySuffocateGrabDamage((pullerUid, puller), pulled);
        }
    }

    private void ApplySuffocateGrabDamage(Entity<PullerComponent> puller, EntityUid pullable)
    {
        _stamina.TakeStaminaDamage(
            pullable,
            puller.Comp.SuffocateGrabStaminaDamage,
            source: puller.Owner);

        _damageable.TryChangeDamage(pullable, puller.Comp.SuffocateGrabDamage, origin: puller.Owner);
    }
}

public enum GrabStage
{
    No = 0,
    Soft = 1,
    Hard = 2,
    Suffocate = 3,
}

public enum GrabStageDirection
{
    Increase,
    Decrease,
}

// Goobstation - Grab Intent

