
using Content.Shared.Sprite;

namespace Content.Shared._Erida.ScaleOnSpawn;

public sealed partial class ScaleOnSpawnSystem : EntitySystem
{
    [Dependency] private readonly SharedScaleVisualsSystem _scaleVisuals = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ScaleOnSpawnComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, ScaleOnSpawnComponent component, MapInitEvent args)
    {
        var scale = _scaleVisuals.GetSpriteScale(uid) * component.Scale;
        _scaleVisuals.SetSpriteScale(uid, scale);
        RemComp<ScaleOnSpawnComponent>(uid);
    }
}
