using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Erida.LightDestroyer.Components;

[RegisterComponent, Access(typeof(LightDestroyerSystem))]
public sealed partial class LightDestroyerComponent : Component
{
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    [DataField(serverOnly: true)]
    public EntProtoId? Entity = "Ash";
}
