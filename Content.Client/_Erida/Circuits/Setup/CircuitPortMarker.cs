using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;


namespace Content.Client._Erida.Circuits.Setup;

public sealed class CircuitPortMarker : Control
{
    public Color TypeColor { get; set; } = Color.Gray;
    public bool Connected { get; set; }

    public CircuitPortMarker()
    {
        MinSize = new Vector2(12, 12);
        MouseFilter = MouseFilterMode.Ignore;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var size = PixelSize;
        if (size.X <= 0 || size.Y <= 0)
            return;

        var center = size / 2f;
        var r = MathF.Min(size.X, size.Y) / 2f - 1f;

        handle.DrawCircle(center, r, TypeColor.WithAlpha(Connected ? 1f : 0.55f), filled: false);

        if (Connected)
            handle.DrawCircle(center, r * 0.42f, TypeColor, filled: true);
    }
}
