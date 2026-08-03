using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared._Erida.Circuits.Components;
using Content.Shared.Interaction;
using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared._Erida.Circuits;

public sealed partial class SharedCircuitSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _sharedContainerSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private PowerCellSystem _powerCellSystem = default!;
    [Dependency] private SharedBatterySystem _sharedBatterySystem = default!;



    public override void Initialize()
    {
        base.Initialize();
        // Components
        SubscribeLocalEvent<CircuitComponentComponent, BeforeRangedInteractEvent>(OnPartRangedInteract);

        // Setups
        SubscribeLocalEvent<CircuitSetupComponent, PowerCellChangedEvent>(OnPowerCellChanged);
        SubscribeLocalEvent<CircuitSetupComponent, ComponentInit>(SetupInit);
        // TODO SubscribeLocalEvent<CircuitSetupComponent, ExaminedEvent>
    }

    private void OnPartRangedInteract(Entity<CircuitComponentComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        args.Handled = TryInsertCircuitComponent(ent.Owner, args.Target.Value, component: ent.Comp);
    }

    public bool GetChargeLevelFromSlot(EntityUid uid, [NotNullWhen(true)] out float? charge)
    {
        charge = null;

        if (!_powerCellSystem.TryGetBatteryFromSlotOrEntity(uid, out var comp))
            return false;

        charge = _sharedBatterySystem.GetChargeLevel((comp.Value.Owner, comp.Value.Comp));

        return true;
    }

    private void OnPowerCellChanged(Entity<CircuitSetupComponent> ent, ref PowerCellChangedEvent args)
    {
        if (!args.Ejected)
        {
            if (_powerCellSystem.TryGetBatteryFromSlotOrEntity(ent.Owner, out var comp))
            {
                ent.Comp.BatteryNetEnt = GetNetEntity(comp.Value);

                var charge = _sharedBatterySystem.GetChargeLevel((ent.Owner, comp.Value));
                UpdateSetupInterface(ent.Owner, ent.Comp, false, charge);
            }
        }

        ent.Comp.BatteryNetEnt = null;
        UpdateSetupInterface(ent.Owner, ent.Comp, false, 0);
        return;
    }

    private void SetupInit(Entity<CircuitSetupComponent> ent, ref ComponentInit args)
    {
        var newContainer = _sharedContainerSystem.EnsureContainer<Container>(ent.Owner, ent.Comp.BaseContainerId);

        if (newContainer != null)
            ent.Comp.PartsContainer = newContainer;
    }

    public bool TryInsertCircuitComponent(EntityUid componentUid, EntityUid setupUid, CircuitSetupComponent? setup = null, CircuitComponentComponent? component = null)
    {
        if (!Resolve(componentUid, ref component, false))
            return false;

        if (!Resolve(setupUid, ref setup, false))
            return false;

        return TryInsertCircuitComponent(
            (componentUid, component),
            (setupUid, setup));
    }

    private bool TryInsertCircuitComponent(Entity<CircuitComponentComponent> component, Entity<CircuitSetupComponent> setup)
    {
        if (!_sharedContainerSystem.TryGetContainer(setup.Owner, setup.Comp.BaseContainerId, out var container))
            return false;

        if (setup.Comp.PartsContainer.Count >= setup.Comp.MaxParts)
        {
            // TODO ADD POPUP
            return false;
        }

        var answerTypeLimit = setup.Comp.MaxTypesOfPart.ContainsKey(component.Comp.AnswerType)
            ? setup.Comp.MaxTypesOfPart[component.Comp.AnswerType] : setup.Comp.FallbackMaxTypesOfPart;


        if (!setup.Comp.InsertedParts.TryGetValue(component.Comp.AnswerType, out var parts))
        {
            parts = new List<NetEntity>() { };
            setup.Comp.InsertedParts[component.Comp.AnswerType] = parts;
        }

        if (parts.Count >= answerTypeLimit)
            return false;

        if (!_sharedContainerSystem.Insert(component.Owner, container))
            return false;

        if (component.Comp.EventType is { } eventType)
        {
            if (!setup.Comp.EventParts.TryGetValue(eventType, out var list))
                setup.Comp.EventParts[eventType] = new List<CircuitResponseType>();

            setup.Comp.EventParts[eventType].Add(component.Comp.AnswerType);
        }

        component.Comp.PositionInSetup = new Vector2(0, 0);

        parts.Add(GetNetEntity(component.Owner));

        Dirty(setup);

        component.Comp.NetContainer = GetNetEntity(container.Owner);

        // TODO add custom sound on insert

        return true;
    }

    private void UpdateSetupInterface(EntityUid uid, CircuitSetupComponent? component = null, bool update = true, float? charge = null)
    {
        if (!Resolve(uid, ref component, logMissing: false))
            return;

        var state = new CircuitSetupBoundUserInterfaceState(update, charge);

        _userInterface.SetUiState(uid,
            CircuitSetupUiKey.Key,
            state);
    }
}
