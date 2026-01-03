using Content.Client._Erida.Overlays;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays;
using Robust.Client.Graphics;

namespace Content.Client.Overlays;

public sealed partial class DesaturationSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private DesaturationOverlay _overlay = default!;
    public override void Initialize()
    {
        base.Initialize();

        _overlay = new();
        _overlayMan.AddOverlay(_overlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlayMan.RemoveOverlay(_overlay);
    }
}
