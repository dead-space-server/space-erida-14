using Content.Shared._Lavaland.Chemistry.Systems;
using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Lavaland.Chemistry.Components;

[RegisterComponent, AutoGenerateComponentPause, AutoGenerateComponentState, NetworkedComponent]
[Access(typeof(BloodstreamRegenerationSystem))]
public sealed partial class BloodstreamRegenerationComponent : Component
{
    [DataField(required: true)]
    public Solution Generated = default!;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    [DataField("nextChargeTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField, AutoNetworkedField]
    public TimeSpan NextRegenTime;
}
