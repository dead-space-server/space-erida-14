// SPDX-FileCopyrightText: 2026 Lytheriia
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared._Erida.Lactation;

public sealed partial class LactationSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private MobStateSystem _mobStateSystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private InventorySystem _inventorySystem = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private HungerSystem _hungerSystem = default!;
    [Dependency] private IngestionSystem _ingestionSystem = default!;
    [Dependency] private BodySystem _bodySystem = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _containerSystem = default!;

    private readonly string _underwearTopSlotName = "undershirt";


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LactationComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<LactationComponent, GetVerbsEvent<AlternativeVerb>>(VerbInit);
        SubscribeLocalEvent<LactationComponent, DoAfterAttemptEvent<LactationDoAfterEvent>>(OnDoAfterAttempt);
        SubscribeLocalEvent<InteractionWhitelistComponent, LactationDoAfterEvent>(OnDoAfter);
    }

    private void OnMapInit(Entity<LactationComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextGrowth = _timing.CurTime + ent.Comp.GrowthDelay;

        if (!TryComp<SolutionManagerComponent>(ent.Owner, out var comp))
            return;

        var container = _containerSystem.EnsureContainer<Container>(ent.Owner, comp.Container);

        var solution = _solutionContainerSystem.CreateSolution(ent.Comp.SolutionName, container);

        solution.Comp.Solution.MaxVolume = ent.Comp.MaxQuantity;
        solution.Comp.Solution.AddReagent(ent.Comp.ReagentId, ent.Comp.MaxQuantity);

        if (TryComp<HumanoidProfileComponent>(ent.Owner, out var profileComponent))
            ent.Comp.IsMilkIncreased = ent.Comp.IncreasedMilkRaces.Contains(profileComponent.Species);
    }

    #region Verbs
    private void VerbInit(Entity<LactationComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!TryComp<InteractionWhitelistComponent>(args.User, out var interactionWhitelistComponent))
            return;

        if (!interactionWhitelistComponent.Lactation)
            return;

        if (!_mobStateSystem.IsAlive(ent.Owner) || _mobStateSystem.IsCritical(ent.Owner))
            return;

        // Checks whether the target body is accessible
        if (!_inventorySystem.TryGetSlotContainer(ent.Owner, _underwearTopSlotName, out var slotContainer, out var slotDef)
            || slotContainer.Count != 0
            || slotDef.StripHiddenForce
            || slotDef.StripBlocked)
            return;

        // We cant use ref in future
        var used = args.Using;
        var user = args.User;
        SolutionComponent? solutionComponent = null;

        // isContainerValid is indicator of whether we drinking or collecting
        var isContainerValid = used != null
            && HasComp<RefillableSolutionComponent>(used)
            && TryComp(used, out solutionComponent);

        if (!isContainerValid
            && args.Target == args.User
            || !_ingestionSystem.HasMouthAvailable(ent.Owner, args.Target)
            || !_bodySystem.TryGetOrgansWithComponent<StomachComponent>(ent.Owner, out var _))
            return;

        AlternativeVerb verb = new()
        {
            Priority = 3,
            Act = () =>
            {
                OnVerbAct(ent, user, isContainerValid, used, solutionComponent);
            },
            Text = Loc.GetString(isContainerValid ? "lactation-verb-collect" : "lactation-verb-drink")
        };

        args.Verbs.Add(verb);
    }

    private void OnVerbAct(Entity<LactationComponent> ent, EntityUid user, bool isContainerValid, EntityUid? used, SolutionComponent? solutionComponent)
    {
        var lactationDoAfter = new LactationDoAfterEvent()
        {
            Status = isContainerValid ? LactationStatus.Collecting : LactationStatus.Drink,
            Repeat = true,
        };

        var doArgs = new DoAfterArgs(EntityManager, user,
            ent.Comp.CollectingTime,
            lactationDoAfter,
            user, ent.Owner, used)
        {
            AttemptFrequency = AttemptFrequency.StartAndEnd,
            BreakOnMove = true,
            BreakOnDamage = true,
            MovementThreshold = 1.0f,
        };

        var name = Identity.Entity(user, EntityManager);

        _popupSystem.PopupClient(Loc.GetString(isContainerValid ? "lactation-trying-collect" : "lactation-trying-milk", ("name", name)),
            ent.Owner, PopupType.Medium);

        _doAfterSystem.TryStartDoAfter(doArgs);
    }
    #endregion

    #region DoAfter
    private void OnDoAfterAttempt(Entity<LactationComponent> entity, ref DoAfterAttemptEvent<LactationDoAfterEvent> args)
    {
        TryComp<LactationComponent>(args.Event.Target!.Value, out var comp);

        if (!_solutionContainerSystem.ResolveSolution(args.Event.Target.Value, comp!.SolutionName, ref comp.Solution, out var solution))
        {
            args.Cancel();
            return;
        }

        if (!_inventorySystem.TryGetSlotContainer(args.Event.Target.Value, _underwearTopSlotName, out var slotContainer, out var slotDef)
            || slotContainer.Count != 0
            || slotDef.StripHiddenForce
            || slotDef.StripBlocked)
        {
            args.Cancel();
            return;
        }

        var reagent = new ReagentId(comp.ReagentId, null);

        if (solution.GetReagentQuantity(reagent) < comp.QuantityPerUse)
        {
            _popupSystem.PopupClient(Loc.GetString("lactation-verb-not-enough"), args.Event.User, PopupType.Medium);
            args.Cancel();
            return;
        }
    }

    private void OnDoAfter(Entity<InteractionWhitelistComponent> entity, ref LactationDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        if (!TryComp<LactationComponent>(args.Target!.Value, out var lactationComp))
            return;

        if (!_solutionContainerSystem.ResolveSolution(args.Target!.Value, lactationComp.SolutionName, ref lactationComp.Solution, out var solution))
            return;

        if (solution.Volume < lactationComp.QuantityPerUse)
        {
            _popupSystem.PopupClient(Loc.GetString("lactation-verb-not-enough"), args.User, PopupType.Medium);
            return;
        }

        if (args.Status == LactationStatus.Collecting)
        {
            if (!_solutionContainerSystem.TryGetRefillableSolution(args.Used!.Value, out var targetSoln, out var _))
                return;

            var split = _solutionContainerSystem.SplitSolution(lactationComp.Solution.Value, lactationComp.QuantityPerUse);

            _solutionContainerSystem.TryAddSolution(targetSoln.Value, split);
        }
        else
        {
            // After wizden refactor, eating from nullspace cant be forced with othet methods
            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, TimeSpan.Zero, new EatingDoAfterEvent(), args.User, lactationComp.Solution.Value)
            {
                BreakOnHandChange = false,
                BreakOnMove = false,
                BreakOnDamage = true,
                MovementThreshold = 4f,
                DistanceThreshold = null,
                NeedHand = false,
            };

            _doAfterSystem.TryStartDoAfter(doAfterArgs);
        }
    }
    #endregion

    #region Update
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LactationComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextGrowth)
                continue;

            comp.NextGrowth += comp.GrowthDelay;

            if (_mobStateSystem.IsDead(uid))
                continue;

            if (!_solutionContainerSystem.ResolveSolution(uid, comp.SolutionName, ref comp.Solution, out var solution))
                continue;

            if (solution.AvailableVolume == 0)
                continue;

            if (TryComp(uid, out HungerComponent? hungerComp))
            {
                if (_hungerSystem.GetHungerThreshold(hungerComp) < HungerThreshold.Okay)
                    continue;

                _hungerSystem.ModifyHunger(uid, -comp.HungerUsage, hungerComp);
            }

            var quantityToAdd = comp.IsMilkIncreased ? comp.QuantityPerUpdate * comp.MilkIncreasedMultiplier : comp.QuantityPerUpdate;

            _solutionContainerSystem.TryAddReagent(comp.Solution.Value, comp.ReagentId, quantityToAdd, out _);
        }
    }
    #endregion
}
