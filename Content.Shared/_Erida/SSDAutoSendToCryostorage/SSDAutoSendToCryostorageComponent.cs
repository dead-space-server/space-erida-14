using Robust.Shared.Audio;
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
    [Access(typeof(SharedSSDAutoSendToCryostorageSystem))]
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan SendToCryostorageTime = TimeSpan.Zero;

    [DataField]
    public SoundSpecifier SoundSend = new SoundPathSpecifier("/Audio/Magic/ethereal_enter.ogg");

    [DataField]
    public SoundSpecifier SoundExit = new SoundPathSpecifier("/Audio/Magic/ethereal_exit.ogg");

    [DataField]
    public string EntityEffect = "ShortPortalEffect";
}
