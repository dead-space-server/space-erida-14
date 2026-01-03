using Robust.Shared.GameStates;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Erida.OnGhostAttemptDamage;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class OnGhostAttemptDamageComponent : Component
{
    [DataField]
    public ProtoId<DamageTypePrototype> BloodlossDamageType = "Bloodloss";
}
