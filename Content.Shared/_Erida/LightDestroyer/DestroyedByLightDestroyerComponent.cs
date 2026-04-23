using Robust.Shared.GameStates;

namespace Content.Shared._Erida.LightDestroyer.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DestroyedByLightDestroyerComponent : Component
{
    [DataField]
    public float TimeNeedToDestroy = 300f;

    public TimeSpan? TimeToDestroy;
}
