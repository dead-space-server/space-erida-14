using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AffectedByVoidAuraComponent : Component
{
    [DataField]
    public EntityUid Aura;

    [DataField]
    public float? OldVelocity;
}
