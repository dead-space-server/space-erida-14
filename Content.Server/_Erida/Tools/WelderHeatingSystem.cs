using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Popups;
using Content.Shared._Erida.Tools.WelderHeating;
using Content.Shared.Interaction;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Tools.Components;
using Content.Shared.Item;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Hands.Components;

namespace Content.Server._Erida.Tools.WelderHeating;

public sealed class WelderHeatingSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _sharedHandsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WelderHeatingComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<WelderHeatingComponent, HeatingDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<WelderHeatingComponent, DoAfterAttemptEvent<HeatingDoAfterEvent>>(OnDoAfterAttempt);
    }

    private void OnAfterInteract(Entity<WelderHeatingComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        if (TryComp<WelderComponent>(ent, out var welder) && !welder.Enabled)
            return;

        if (!HasComp<ItemComponent>(args.Target))
            return;

        if (!TryComp<SolutionContainerManagerComponent>(target, out var container))
            return;

        if (!TryComp<HandsComponent>(args.User, out var hands) || !_sharedHandsSystem.IsHolding((args.User, hands), args.Target))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, ent.Comp.HeatDuration, new HeatingDoAfterEvent(), ent, target: target, used: ent)
        {
            AttemptFrequency = AttemptFrequency.EveryTick,
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
            BreakOnHandChange = true,
            BlockDuplicate = true
        });
    }
    private void OnDoAfterAttempt(Entity<WelderHeatingComponent> ent, ref DoAfterAttemptEvent<HeatingDoAfterEvent> args)
    {
        if (TryComp<WelderComponent>(ent, out var welder) && !welder.Enabled)
            args.Cancel();
    }
    private void OnDoAfter(Entity<WelderHeatingComponent> ent, ref HeatingDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target == null)
            return;

        var target = args.Args.Target.Value;
        if (!TryComp<SolutionContainerManagerComponent>(target, out var container))
            return;

        foreach (var (_, soln) in _solutionContainer.EnumerateSolutions((target, container)))
        {
            if (soln.Comp.Solution.Temperature > ent.Comp.HeatThreshold)
                continue;
            _solutionContainer.AddThermalEnergy(soln, ent.Comp.HeatPerUse);
        }

        args.Repeat = true;

        var msg = Loc.GetString("warming-with-welder",
            ("container", Name(target)),
            ("welder", Name(ent.Owner)));
        _popup.PopupEntity(msg, args.User, args.User);
    }
}
