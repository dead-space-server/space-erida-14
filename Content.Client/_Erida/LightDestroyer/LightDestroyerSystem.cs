using Content.Shared._Erida.LightDestroyer;
using Content.Shared._Erida.LightDestroyer.Components;
using Robust.Client.GameObjects;

namespace Content.Client._Erida.LightDestroyer;

public sealed class LightDestroyerSystem : SharedLightDestroyerSystem
{
    [Dependency] private readonly PointLightSystem _pointLight = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DestroyedByLightDestroyerComponent, ComponentInit>(OnDestroyedInit);
    }

    private void OnDestroyedInit(Entity<DestroyedByLightDestroyerComponent> ent, ref ComponentInit args)
    {
        _pointLight.SetEnabled(ent, false);
    }
}
