using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Erida.SSDAutoSendToCryostorage.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SSDAutoSendToCryostorageComponent : Component
{
    [AutoNetworkedField]
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool Active = false;

    [AutoNetworkedField, AutoPausedField]
    [Access(typeof(SSDAutoSendToCryostorageSystem))]
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan SendToCryostorageTime = TimeSpan.Zero;
}
