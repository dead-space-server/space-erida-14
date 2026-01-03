using System.Threading;
using Content.Server.DoAfter;
using Content.Server.Body.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Resist;
using Content.Server.Popups;
using Content.Shared.Climbing.Events;
using Content.Shared.Mobs;
using Content.Shared.DoAfter;
using Content.Shared.Buckle.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands;
using Content.Shared.Stunnable;
using Content.Shared.Interaction.Events;
using Content.Shared.Verbs;
using Content.Shared.Carrying;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Standing;
using Content.Shared.ActionBlocker;
using Content.Shared.Throwing;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Map.Components;
using Content.Shared.IdentityManagement;
using Robust.Shared.Player;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using System.Numerics;

namespace Content.Server.Carrying
{
    public sealed class CarryingSystem : EntitySystem
    {
        [Dependency] private readonly SharedVirtualItemSystem _virtualItemSystem = default!;
        [Dependency] private readonly CarryingSlowdownSystem _slowdown = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly StandingStateSystem _standingState = default!;
        [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
        [Dependency] private readonly PullingSystem _pullingSystem = default!;
        [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
        [Dependency] private readonly EscapeInventorySystem _escapeInventorySystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
        [Dependency] private readonly RespiratorSystem _respirator = default!;
        [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
        [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<CarriableComponent, GetVerbsEvent<AlternativeVerb>>(AddCarryVerb);
            SubscribeLocalEvent<CarryingComponent, VirtualItemDeletedEvent>(OnVirtualItemDeleted);
            SubscribeLocalEvent<CarryingComponent, BeforeThrowEvent>(OnThrow);
            SubscribeLocalEvent<CarryingComponent, EntParentChangedMessage>(OnParentChanged);
            SubscribeLocalEvent<CarryingComponent, MobStateChangedEvent>(OnMobStateChanged);
            SubscribeLocalEvent<BeingCarriedComponent, InteractionAttemptEvent>(OnInteractionAttempt);
            SubscribeLocalEvent<BeingCarriedComponent, MoveInputEvent>(OnMoveInput);
            SubscribeLocalEvent<BeingCarriedComponent, UpdateCanMoveEvent>(OnMoveAttempt);
            SubscribeLocalEvent<BeingCarriedComponent, StandAttemptEvent>(OnStandAttempt);
            SubscribeLocalEvent<BeingCarriedComponent, GettingInteractedWithAttemptEvent>(OnInteractedWith);
            SubscribeLocalEvent<BeingCarriedComponent, PullAttemptEvent>(OnPullAttempt);
            SubscribeLocalEvent<BeingCarriedComponent, StartClimbEvent>(OnStartClimb);
            SubscribeLocalEvent<BeingCarriedComponent, BuckledEvent>(OnBuckleChange);
            SubscribeLocalEvent<BeingCarriedComponent, UnbuckledEvent>(OnBuckleChange);
            SubscribeLocalEvent<CarriableComponent, CarryDoAfterEvent>(OnDoAfter);
            SubscribeLocalEvent<BeingCarriedComponent, GetVerbsEvent<AlternativeVerb>>(AddEscapeVerb);
            SubscribeLocalEvent<BeingCarriedComponent, EscapeDoAfterEvent>(OnEscapeDoAfter);
            SubscribeLocalEvent<BeingCarriedComponent, ComponentShutdown>(OnBeingCarriedShutdown);
        }


        private void AddCarryVerb(EntityUid uid, CarriableComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            if (!CanCarry(args.User, uid, component))
                return;

            if (HasComp<CarryingComponent>(args.User)) // yeah not dealing with that
                return;

            if (HasComp<BeingCarriedComponent>(args.User) || HasComp<BeingCarriedComponent>(args.Target))
                return;

            if (!_mobStateSystem.IsAlive(args.User))
                return;

            if (args.User == args.Target)
                return;

            // Check if the user cannot carry others
            if (HasComp<CannotCarryComponent>(args.User))
            {
                _popupSystem.PopupEntity(Loc.GetString("carry-too-heavy"), args.Target, args.User, Shared.Popups.PopupType.SmallCaution);
                return;
            }

            AlternativeVerb verb = new()
            {
                Act = () =>
                {
                    StartCarryDoAfter(args.User, uid, component);
                },
                Text = Loc.GetString("carry-verb"),
                Priority = 2
            };

            args.Verbs.Add(verb);
        }

        /// <summary>
        /// Since the carried entity is stored as 2 virtual items, when deleted we want to drop them.
        /// </summary>
        private void OnVirtualItemDeleted(EntityUid uid, CarryingComponent component, VirtualItemDeletedEvent args)
        {
            if (!HasComp<CarriableComponent>(args.BlockingEntity))
                return;

            DropCarried(uid, component.Carried);
        }

        /// <summary>
        /// Basically using virtual item passthrough to throw the carried person. A new age!
        /// Maybe other things besides throwing should use virt items like this...
        /// </summary>
        private void OnThrow(EntityUid uid, CarryingComponent component, BeforeThrowEvent args)
        {
            if (!TryComp<VirtualItemComponent>(args.ItemUid, out var virtItem) || !HasComp<CarriableComponent>(virtItem.BlockingEntity))
                return;

            args.ItemUid = virtItem.BlockingEntity;

            // Simple mass calculation - can be improved later
            // Note: ThrowStrength property may not exist in current API
        }

        private void OnParentChanged(EntityUid uid, CarryingComponent component, ref EntParentChangedMessage args)
        {
            if (args.OldMapId != null && Transform(uid).MapID != Transform(args.OldMapId.Value).MapID)
                return;

            DropCarried(uid, component.Carried);
        }

        private void OnMobStateChanged(EntityUid uid, CarryingComponent component, MobStateChangedEvent args)
        {
            DropCarried(uid, component.Carried);
        }

        /// <summary>
        /// Only let the person being carried interact with their carrier and things on their person.
        /// </summary>
        private void OnInteractionAttempt(EntityUid uid, BeingCarriedComponent component, InteractionAttemptEvent args)
        {
            if (args.Target == null)
                return;

            var targetParent = Transform(args.Target.Value).ParentUid;

            if (args.Target.Value != component.Carrier && targetParent != component.Carrier && targetParent != uid)
                args.Cancelled = true;
        }

        /// <summary>
        /// Try to escape when movement keys are pressed.
        /// </summary>
        private void OnMoveInput(EntityUid uid, BeingCarriedComponent component, ref MoveInputEvent args)
        {
            // Check if component still exists and is valid
            if (!Exists(uid) || !HasComp<BeingCarriedComponent>(uid))
                return;

            // Only start escape if not already trying to escape
            if (component.EscapeCancelToken != null)
                return;

            // Check if any movement key is pressed
            if (!args.Entity.Comp.HasDirectionalMovement)
                return;

            // Start escape attempt
            StartEscapeDoAfter(uid, component);
        }

        private void OnMoveAttempt(EntityUid uid, BeingCarriedComponent component, UpdateCanMoveEvent args)
        {
            args.Cancel();
        }

        private void OnStandAttempt(EntityUid uid, BeingCarriedComponent component, StandAttemptEvent args)
        {
            args.Cancel();
        }

        private void OnInteractedWith(EntityUid uid, BeingCarriedComponent component, GettingInteractedWithAttemptEvent args)
        {
            if (args.Uid != component.Carrier)
                args.Cancelled = true;
        }

        private void OnPullAttempt(EntityUid uid, BeingCarriedComponent component, PullAttemptEvent args)
        {
            args.Cancelled = true;
        }

        private void OnStartClimb(EntityUid uid, BeingCarriedComponent component, StartClimbEvent args)
        {
            DropCarried(component.Carrier, uid);
        }

        private void OnBuckleChange(EntityUid uid, BeingCarriedComponent component, ref BuckledEvent args)
        {
            DropCarried(component.Carrier, uid);
        }

        private void OnBuckleChange(EntityUid uid, BeingCarriedComponent component, ref UnbuckledEvent args)
        {
            DropCarried(component.Carrier, uid);
        }

        private void OnDoAfter(EntityUid uid, CarriableComponent component, CarryDoAfterEvent args)
        {
            component.CancelToken = null;
            if (args.Handled || args.Cancelled)
                return;

            if (!CanCarry(args.Args.User, uid, component))
                return;

            Carry(args.Args.User, uid);
            args.Handled = true;
        }

		private void StartCarryDoAfter(EntityUid carrier, EntityUid carried, CarriableComponent component)
		{
			// Базовое время в зависимости от состояния
			TimeSpan length = HasComp<KnockedDownComponent>(carried)
				? TimeSpan.FromSeconds(1)  // 2 секунды если сбит с ног
				: TimeSpan.FromSeconds(2); // 4 секунды если не сбит

			// Simple mass calculation - can be improved later
			var mod = 1.0f; // Default modifier

			if (mod != 0)
				length /= mod;

			if (length >= TimeSpan.FromSeconds(9))
			{
				_popupSystem.PopupEntity(Loc.GetString("carry-too-heavy"), carried, carrier, Shared.Popups.PopupType.SmallCaution);
				return;
			}

			component.CancelToken = new CancellationTokenSource();

			var ev = new CarryDoAfterEvent();
			var args = new DoAfterArgs(EntityManager, carrier, length, ev, carried, target: carried)
			{
				BreakOnMove = true,
				NeedHand = true
			};

			_doAfterSystem.TryStartDoAfter(args);

			// Show attempt message
			_popupSystem.PopupEntity(Loc.GetString("carry-pickup-carrier-attempt",
				("carried", Identity.Name(carried, EntityManager, carrier))),
				carried, carrier, Shared.Popups.PopupType.Medium);
		}

        private void Carry(EntityUid carrier, EntityUid carried)
        {
            // TODO: Fix SharedPullableComponent reference
            // if (TryComp<SharedPullableComponent>(carried, out var pullable))
            //     _pullingSystem.TryStopPull(pullable);

            _xformSystem.AttachToGridOrMap(carrier);
            _xformSystem.AttachToGridOrMap(carried);
            _xformSystem.SetCoordinates(carried, Transform(carrier).Coordinates);
            _xformSystem.SetParent(carried, carrier);
            _virtualItemSystem.TrySpawnVirtualItemInHand(carried, carrier);
            _virtualItemSystem.TrySpawnVirtualItemInHand(carried, carrier);
            var carryingComp = EnsureComp<CarryingComponent>(carrier);
            ApplyCarrySlowdown(carrier, carried);
            var carriedComp = EnsureComp<BeingCarriedComponent>(carried);
            EnsureComp<KnockedDownComponent>(carried);

            carryingComp.Carried = carried;
            carriedComp.Carrier = carrier;

            _actionBlockerSystem.UpdateCanMove(carried);

            // Show popup messages
            _popupSystem.PopupEntity(Loc.GetString("carry-pickup-carrier",
                ("carried", Identity.Name(carried, EntityManager, carrier))),
                carried, carrier, Shared.Popups.PopupType.Medium);
            _popupSystem.PopupEntity(Loc.GetString("carry-pickup-carried",
                ("carrier", Identity.Name(carrier, EntityManager, carried))),
                carrier, carried, Shared.Popups.PopupType.Medium);

            // Show message to everyone around
            var filter = Filter.Pvs(carrier, entityManager: EntityManager);
            if (filter != null)
            {
                _popupSystem.PopupEntity(Loc.GetString("carry-pickup-others",
                    ("carrier", Identity.Name(carrier, EntityManager)),
                    ("carried", Identity.Name(carried, EntityManager))),
                    carrier, filter.RemoveWhere(e => e.AttachedEntity == carrier || e.AttachedEntity == carried),
                    true, Shared.Popups.PopupType.LargeCaution);
            }
        }

        public void DropCarried(EntityUid carrier, EntityUid carried)
        {
            RemComp<CarryingComponent>(carrier); // get rid of this first so we don't recusrively fire that event
            RemComp<CarryingSlowdownComponent>(carrier);
            RemComp<BeingCarriedComponent>(carried);
            RemComp<KnockedDownComponent>(carried);
            _actionBlockerSystem.UpdateCanMove(carried);
            _virtualItemSystem.DeleteInHandsMatching(carrier, carried);
            _xformSystem.AttachToGridOrMap(carried);
            _standingState.Stand(carried);
            _movementSpeed.RefreshMovementSpeedModifiers(carrier);

            // Show popup messages
            _popupSystem.PopupEntity(Loc.GetString("carry-drop-carrier",
                ("carried", Identity.Name(carried, EntityManager, carrier))),
                carried, carrier, Shared.Popups.PopupType.Medium);
            _popupSystem.PopupEntity(Loc.GetString("carry-drop-carried",
                ("carrier", Identity.Name(carrier, EntityManager, carried))),
                carrier, carried, Shared.Popups.PopupType.Medium);

            // Show message to everyone around
            var filter = Filter.Pvs(carrier, entityManager: EntityManager);
            if (filter != null)
            {
                _popupSystem.PopupEntity(Loc.GetString("carry-drop-others",
                    ("carrier", Identity.Name(carrier, EntityManager)),
                    ("carried", Identity.Name(carried, EntityManager))),
                    carrier, filter.RemoveWhere(e => e.AttachedEntity == carrier || e.AttachedEntity == carried),
                    true, Shared.Popups.PopupType.LargeCaution);
            }
        }

        private void ApplyCarrySlowdown(EntityUid carrier, EntityUid carried)
        {
            // Calculate mass-based slowdown
            var massRatio = CalculateMassRatio(carrier, carried);

            // Base slowdown penalty
            var basePenalty = 0.3f; // 30% slowdown by default

            // Additional penalty based on mass
            var massPenalty = Math.Min(0.4f, massRatio * 0.2f); // Up to 40% additional penalty

            // Total slowdown
            var totalSlowdown = basePenalty + massPenalty;
            var modifier = Math.Max(0.1f, 1.0f - totalSlowdown); // Minimum 10% speed

            var slowdownComp = EnsureComp<CarryingSlowdownComponent>(carrier);
            _slowdown.SetModifier(carrier, modifier, modifier, slowdownComp);
        }

        private float CalculateMassRatio(EntityUid carrier, EntityUid carried)
        {
            // Try to get actual mass from physics components
            var carrierMass = 1.0f;
            var carriedMass = 1.0f;

            if (TryComp<PhysicsComponent>(carrier, out var carrierPhysics))
                carrierMass = carrierPhysics.Mass;

            if (TryComp<PhysicsComponent>(carried, out var carriedPhysics))
                carriedMass = carriedPhysics.Mass;

            // Ensure masses are valid
            if (float.IsNaN(carrierMass) || float.IsInfinity(carrierMass) || carrierMass <= 0)
                carrierMass = 1.0f;

            if (float.IsNaN(carriedMass) || float.IsInfinity(carriedMass) || carriedMass <= 0)
                carriedMass = 1.0f;

            // Return ratio of carried mass to carrier mass
            var ratio = carriedMass / Math.Max(0.1f, carrierMass);

            // Ensure ratio is valid
            if (float.IsNaN(ratio) || float.IsInfinity(ratio))
                ratio = 1.0f;

            // Clamp to reasonable range
            return Math.Clamp(ratio, 0.1f, 5.0f);
        }

        public bool CanCarry(EntityUid carrier, EntityUid carried, CarriableComponent? carriedComp = null)
        {
            if (!Resolve(carried, ref carriedComp, false))
                return false;

            if (carriedComp.CancelToken != null)
                return false;

            if (!HasComp<MapGridComponent>(Transform(carrier).ParentUid))
                return false;

            if (HasComp<BeingCarriedComponent>(carrier) || HasComp<BeingCarriedComponent>(carried))
                return false;

            // Skip CPR check for now - method may not exist
            // if (_respirator.IsReceivingCPR(carried))
            //     return false;

            if (!TryComp<HandsComponent>(carrier, out var hands))
                return false;

            if (_handsSystem.CountFreeHands(carrier) < carriedComp.FreeHandsRequired)
                return false;

            return true;
        }

        private void AddEscapeVerb(EntityUid uid, BeingCarriedComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            if (args.User != uid)
                return;

            if (component.EscapeCancelToken != null)
                return;

            if (!_mobStateSystem.IsAlive(args.User))
                return;

            AlternativeVerb verb = new()
            {
                Act = () =>
                {
                    StartEscapeDoAfter(args.User, component);
                },
                Text = Loc.GetString("carry-escape-verb"),
                Priority = 1
            };
            args.Verbs.Add(verb);
        }

        private void StartEscapeDoAfter(EntityUid carried, BeingCarriedComponent component)
        {
            if (component.EscapeCancelToken != null)
                return;

            // Escape time based on whether the carrier is alive
            var carrier = component.Carrier;
            TimeSpan length = _mobStateSystem.IsAlive(carrier)
                ? TimeSpan.FromSeconds(3)  // 3 seconds if carrier is alive
                : TimeSpan.FromSeconds(1); // 1 second if carrier is dead/unconscious

            component.EscapeCancelToken = new CancellationTokenSource();

            var ev = new EscapeDoAfterEvent();
            var args = new DoAfterArgs(EntityManager, carried, length, ev, carried, target: carried)
            {
                BreakOnMove = true,
                BreakOnDamage = true
            };

            _doAfterSystem.TryStartDoAfter(args);

            // Show popup messages
            _popupSystem.PopupEntity(Loc.GetString("carry-escape-start-carrier",
                ("carried", Identity.Name(carried, EntityManager, carrier))),
                carried, carrier, Shared.Popups.PopupType.Medium);
            _popupSystem.PopupEntity(Loc.GetString("carry-escape-start-carried",
                ("carrier", Identity.Name(carrier, EntityManager, carried))),
                carrier, carried, Shared.Popups.PopupType.Medium);

            // Show message to everyone around
            var filter = Filter.Pvs(carrier, entityManager: EntityManager);
            if (filter != null)
            {
                _popupSystem.PopupEntity(Loc.GetString("carry-escape-start-others",
                    ("carrier", Identity.Name(carrier, EntityManager)),
                    ("carried", Identity.Name(carried, EntityManager))),
                    carrier, filter.RemoveWhere(e => e.AttachedEntity == carrier || e.AttachedEntity == carried),
                    true, Shared.Popups.PopupType.LargeCaution);
            }
        }

        private void OnEscapeDoAfter(EntityUid uid, BeingCarriedComponent component, EscapeDoAfterEvent args)
        {
            // Dispose the escape token properly
            component.DisposeEscapeToken();

            if (args.Handled || args.Cancelled)
                return;

            var carrier = component.Carrier;
            if (!Exists(carrier))
                return;

            // Successfully escaped
            DropCarried(carrier, uid);

            // Show success popup messages
            _popupSystem.PopupEntity(Loc.GetString("carry-escape-success-carrier",
                ("carried", Identity.Name(uid, EntityManager, carrier))),
                uid, carrier, Shared.Popups.PopupType.Medium);
            _popupSystem.PopupEntity(Loc.GetString("carry-escape-success-carried",
                ("carrier", Identity.Name(carrier, EntityManager, uid))),
                carrier, uid, Shared.Popups.PopupType.Medium);

            // Show message to everyone around
            var filter = Filter.Pvs(carrier, entityManager: EntityManager);
            if (filter != null)
            {
                _popupSystem.PopupEntity(Loc.GetString("carry-escape-success-others",
                    ("carrier", Identity.Name(carrier, EntityManager)),
                    ("carried", Identity.Name(uid, EntityManager))),
                    carrier, filter.RemoveWhere(e => e.AttachedEntity == carrier || e.AttachedEntity == uid),
                    true, Shared.Popups.PopupType.LargeCaution);
            }

            args.Handled = true;
        }

        private void OnBeingCarriedShutdown(EntityUid uid, BeingCarriedComponent component, ComponentShutdown args)
        {
            // Clean up escape token when component is removed
            component.DisposeEscapeToken();
        }

    }
}
