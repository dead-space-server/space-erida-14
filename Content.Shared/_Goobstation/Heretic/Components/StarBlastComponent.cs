using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StarBlastComponent : Component
{
    [DataField]
    public float StarMarkRadius = 3f;

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(4);

    [DataField]
    public EntityUid Action;
}
