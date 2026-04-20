using System.Runtime.CompilerServices;
using Content.Shared._Erida.LightDestroyer.Components;
using Robust.Shared.ComponentTrees;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Shared._Erida.LightDestroyer;

public abstract class SharedLightDestroyerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DestroyedByLightDestroyerComponent, AttemptPointLightToggleEvent>(OnDestroyedToggle);
    }

    private void OnDestroyedToggle(Entity<DestroyedByLightDestroyerComponent> ent, ref AttemptPointLightToggleEvent args)
    {
        if (args.Enabled)
            args.Cancelled = true;
    }
}
