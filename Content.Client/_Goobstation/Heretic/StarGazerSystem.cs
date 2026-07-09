using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic.Systems;
using Content.Shared._Goobstation.Heretic;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Map;

namespace Content.Client._Goobstation.Heretic;

public sealed partial class StarGazerSystem : SharedStarGazerSystem
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private IInputManager _input = default!;

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (!Timing.IsFirstTimePredicted)
            return;

        if (!HasComp<StarGazeComponent>(_player.LocalEntity))
            return;

        var player = _player.LocalEntity.Value;

        MapCoordinates? mousePos = _eye.PixelToMap(_input.MouseScreenPosition);

        if (mousePos.Value.MapId == MapId.Nullspace)
            return;

        RaisePredictiveEvent(new LaserBeamEndpointPositionEvent(GetNetEntity(player), mousePos.Value));
    }
}
