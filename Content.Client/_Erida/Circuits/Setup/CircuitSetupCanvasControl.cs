using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client._Erida.Circuits.Setup;

public sealed partial class CircuitSetupCanvasControl : LayoutContainer
{
    [Dependency] private IInputManager _inputManager = default!;
    [Dependency] private IEntityManager _entityManager = default!;

    private CircuitPortControl? _dragSource;
    private Vector2 _cursorPosition;

    public readonly Dictionary<EntityUid, CircuitNodeControl> NodesByEntity = new();

    public event Action<EntityUid, byte, EntityUid, byte>? OnLinkRequested;

    public CircuitSetupCanvasControl()
    {
        IoCManager.InjectDependencies(this);

        MouseFilter = MouseFilterMode.Pass;
    }

    public void ClearNodes()
    {
        _dragSource = null;
        NodesByEntity.Clear();
        RemoveAllChildren();
    }

    public void AddNode(CircuitNodeControl node)
    {
        AddChild(node);
        NodesByEntity[node.Entity] = node;
    }

    public void BeginWireDrag(CircuitPortControl port)
    {
        _dragSource = port;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        if (_dragSource is null)
            return;

        var screenPos = _inputManager.MouseScreenPosition.Position;
        _cursorPosition = screenPos - GlobalPixelPosition;
    }

    public void EndWireDrag(CircuitPortControl? targetPort)
    {
        if (_dragSource == null)
            return;

        if (targetPort == null || targetPort == _dragSource)
        {
            _dragSource = null;
            return;
        }

        if (targetPort.Direction == _dragSource.Direction)
        {
            _dragSource = null;
            return;
        }

        if (targetPort.Data.DataType != _dragSource.Data.DataType)
        {
            _dragSource = null;
            return;
        }

        var (from, to) = _dragSource.Direction == PortDirection.Output
            ? (_dragSource, targetPort)
            : (targetPort, _dragSource);

        OnLinkRequested?.Invoke(from.Owner, (byte)from.Index, to.Owner, (byte)to.Index);
        _dragSource = null;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        DrawExistingLinks(handle);

        if (_dragSource == null)
            return;

        var from = _dragSource.GlobalPixelPosition - GlobalPixelPosition + _dragSource.PixelSize / 2;
        var color = CircuitPortColors.GetColor(_dragSource.Data.DataType);
        DrawBezier(handle, from, _cursorPosition, color);
    }

    private void DrawExistingLinks(DrawingHandleScreen handle)
    {
        foreach (var node in NodesByEntity.Values)
        {
            foreach (var outPort in node.OutputPorts)
            {

                if (outPort.Data.ConnectedComponent is not { } connectedNet)
                    continue;

                if (outPort.Data.ConnectedIndex is not { } connectedIndex)
                    continue;

                if (!_entityManager.TryGetEntity(connectedNet, out var targetUid))
                    continue;

                if (!NodesByEntity.TryGetValue(targetUid.Value, out var targetNode))
                    continue;

                if (connectedIndex >= targetNode.InputPorts.Count)
                    continue;

                var inPort = targetNode.InputPorts[connectedIndex];

                var inputVect = outPort.GlobalPixelPosition - GlobalPixelPosition + outPort.PixelSize / 2;
                var outputVect = inPort.GlobalPixelPosition - GlobalPixelPosition + inPort.PixelSize / 2;

                var color = CircuitPortColors.GetColor(outPort.Data.DataType);

                DrawBezier(handle, inputVect, outputVect, color);
            }
        }
    }

    private static void DrawBezier(DrawingHandleScreen handle, Vector2 from, Vector2 to, Color color)
    {
        var dx = MathF.Max(50f, MathF.Abs(to.X - from.X) * 0.5f);
        var c1 = from + new Vector2(dx, 0);
        var c2 = to - new Vector2(dx, 0);

        const int segments = 24;
        var prev = from;
        for (var i = 1; i <= segments; i++)
        {
            var t = i / (float)segments;
            var p = CubicBezier(from, c1, c2, to, t);
            handle.DrawLine(prev, p, color);
            prev = p;
        }
    }

    private static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        var u = 1 - t;
        return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
    }
}
