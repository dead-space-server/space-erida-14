using Robust.Shared.GameStates;

namespace Content.Shared._Erida.LightDestroyer.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DestroyedByLightDestroyerComponent : Component
{
    [DataField]
    public float timeNeedToDestroy = 300f;

    public TimeSpan? timeToDestroy;
}
