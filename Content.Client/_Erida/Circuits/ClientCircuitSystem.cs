using Content.Shared._Erida.Circuits.Components;
using Robust.Shared.Containers;


namespace Content.Shared._Erida.Circuits;

public sealed partial class ClientCircuitSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _sharedContainerSystem = default!;
    [Dependency] private SharedUserInterfaceSystem _userInterface = default!;


    public override void Initialize()
    {
        base.Initialize();

        // We cant update UI from server, because we waiting when client get new data
        SubscribeLocalEvent<CircuitComponentComponent, AfterAutoHandleStateEvent>(OnComponentStateUpdated);
        SubscribeLocalEvent<CircuitSetupComponent, AfterAutoHandleStateEvent>(OnSetupStateUpdated);
    }

    private void OnComponentStateUpdated(EntityUid uid, CircuitComponentComponent component, ref AfterAutoHandleStateEvent args)
    {
        if (_sharedContainerSystem.TryGetContainingContainer(uid, out var container))
            UpdateSetupInterface(container.Owner);
    }

    private void OnSetupStateUpdated(EntityUid uid, CircuitSetupComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateSetupInterface(uid, component);
    }

    private void UpdateSetupInterface(EntityUid uid, CircuitSetupComponent? component = null)
    {
        if (!Resolve(uid, ref component, logMissing: false))
            return;

        _userInterface.SetUiState(uid,
            CircuitSetupUiKey.Key,
            new CircuitSetupBoundUserInterfaceState());
    }
}
