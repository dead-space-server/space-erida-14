using System.Runtime.CompilerServices;
using Content.Shared._Erida.LightDestroyer.Components;
using Robust.Shared.ComponentTrees;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Shared._Erida.LightDestroyer;

public abstract class SharedLightDestroyerSystem : EntitySystem
{
    [Dependency] private readonly SharedPointLightSystem _sharedPointLight = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DestroyedByLightDestroyerComponent, AttemptPointLightToggleEvent>(OnDestroyedToggle);
       // SubscribeLocalEvent<DestroyedByLightDestroyerComponent, ComponentInit>(OnDestroyedInit);
    }

    private void OnDestroyedToggle(Entity<DestroyedByLightDestroyerComponent> ent, ref AttemptPointLightToggleEvent args)
    {
        if (args.Enabled)
            args.Cancelled = true;
    }

    //private void OnDestroyedInit(Entity<DestroyedByLightDestroyerComponent> ent, ref ComponentInit args)
    //{
        //_sharedPointLight.SetEnabled(ent, false);
        //Dirty(ent, ent.Comp);
    //}
}
