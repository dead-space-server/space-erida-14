using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Flammability;

[RegisterComponent, NetworkedComponent]
public sealed partial class FireImmunityComponent : Component
{
    public override bool SessionSpecific => true;
}
