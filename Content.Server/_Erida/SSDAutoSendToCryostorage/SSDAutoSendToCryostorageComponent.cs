using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Erida.SSDAutoSendToCryostorage.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class SSDAutoSendToCryostorageComponent : Component
{
    [Access(typeof(SSDAutoSendToCryostorageSystem))]
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool Active = false;

    [AutoPausedField]
    [Access(typeof(SSDAutoSendToCryostorageSystem))]
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan SendToCryostorageTime = TimeSpan.Zero;

    [DataField]
    public SoundSpecifier SoundSend = new SoundPathSpecifier("/Audio/Magic/ethereal_enter.ogg");

    [DataField]
    public SoundSpecifier SoundExit = new SoundPathSpecifier("/Audio/Magic/ethereal_exit.ogg");

    [DataField]
    public string EntityEffect = "ShortPortalEffect";
}
