using Robust.Shared.GameStates;

namespace Content.Shared._ADT.Silicon.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SeeingStaticComponent : Component
{
    [AutoNetworkedField]
    public float Multiplier = 1f;
}
