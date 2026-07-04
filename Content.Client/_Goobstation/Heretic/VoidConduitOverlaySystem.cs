using Content.Client._Goobstation.Heretic.UI;
using Robust.Client.Graphics;

namespace Content.Client._Goobstation.Heretic;

public sealed class VoidConduitOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay.AddOverlay(new VoidConduitOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlay.RemoveOverlay<VoidConduitOverlay>();
    }
}
