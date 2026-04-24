using Content.Shared._Erida.Botany.SeedDna;
using Content.Shared.Containers.ItemSlots;
using static Content.Shared._Erida.Botany.SeedDna.Components.SeedDnaConsoleComponent;

namespace Content.Client._Erida.Botany.UI;

/// <summary>
/// SeedDnaConsole UI port from Adventure time.
/// </summary>
public sealed class SeedDnaConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private SeedDnaConsoleWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = new SeedDnaConsoleWindow(this);

        _window.SeedButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(SeedSlotId));
        _window.DnaDiskButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(DnaDiskSlotId));

        _window.OnClose += Close;
        _window.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _window?.Close();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        var castState = (SeedDnaConsoleBoundUserInterfaceState)state;
        _window?.UpdateState(castState);
    }

    public void SubmitData(TargetSeedData target, SeedDataDto seedDataDto)
    {
        SendMessage(new WriteToTargetSeedDataMessage(target, seedDataDto));
    }
}
