using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class FleshMimickedComponent : Component
{
    [DataField]
    public List<EntityUid> FleshMimics = new();
}
