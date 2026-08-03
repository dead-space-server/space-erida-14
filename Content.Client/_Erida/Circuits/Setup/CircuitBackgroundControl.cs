using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;

namespace Content.Client._Erida.Circuits.Setup;

public sealed class CircuitBackgroundControl : Control
{
    private static readonly Color BgColor = Color.FromHex("#0a0810");

    public CircuitBackgroundControl()
    {
        MouseFilter = MouseFilterMode.Pass;

    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var size = PixelSize;
        if (size.X <= 0 || size.Y <= 0)
            return;

        handle.DrawRect(new UIBox2(0, 0, size.X, size.Y), BgColor);
    }

}
