using Content.Shared._Erida.Circuits;
using Content.Shared._Erida.Circuits.Components;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client._Erida.Circuits.Setup;

public sealed class CircuitSetupBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private CircuitSetupWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CircuitSetupWindow>();
        _window.SetEntity(Owner);
        _window.UpdatePanels();
        _window.OpenCentered();

        _window.OnLinkRequested += OnLinkRequested;
        _window.OnDeleteRequested += OnDeleteRequested;
    }

    private void OnLinkRequested(EntityUid outputUid, byte outputIndex, EntityUid inputUid, byte inputIndex)
    {
        SendMessage(new CircuitCreateLinkMessage
        {
            LinkData = new CircuiNetLinkData()
            {
                OutputComponent = EntMan.GetNetEntity(outputUid),
                OutputPortIndex = outputIndex,
                InputComponent = EntMan.GetNetEntity(inputUid),
                InputPortIndex = inputIndex
            }
        });
    }

    private void OnDeleteRequested(EntityUid uid)
    {
        if (EntMan.HasComponent<CircuitComponentComponent>(uid))
        {
            SendMessage(new CircuitDeleteComponentMessage
            {
                CompEntity = EntMan.GetNetEntity(uid)
            });
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not CircuitSetupBoundUserInterfaceState setupState || _window == null)
            return;

        if (setupState.UpdateNodes)
            _window.UpdatePanels();

        if (setupState.Charge is not null)
        {
            _window.CircuitHud.SetPercent(setupState.Charge.Value * 100);
        }
    }
}
